#!/usr/bin/env bash
# Hosted macOS Android smoke run. Owns only processes/AVDs created by this invocation.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$REPO_ROOT"

: "${AGENT_TEMPDIRECTORY:?Run this helper from the Azure mobile job}"
: "${MOBILE_CI_RESULTS:?Set the mobile artifact directory}"
: "${ANDROID_HOME:?Android SDK is required}"
: "${APPIUM_VERSION:?Pin the Appium version}"
: "${UIAUTOMATOR2_VERSION:?Pin the UiAutomator2 version}"

work="$(mktemp -d "$AGENT_TEMPDIRECTORY/mobile-ci.XXXXXX")"
logs="$MOBILE_CI_RESULTS/logs"
mkdir -p "$logs" "$MOBILE_CI_RESULTS/trx" "$MOBILE_CI_RESULTS/allure-results"
export APPIUM_HOME="$work/appium-home"
export ANDROID_AVD_HOME="$work/avd"
mkdir -p "$ANDROID_AVD_HOME"
export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"
export JAVA_HOME="${JAVA_HOME_17_X64:-$(/usr/libexec/java_home -v 17)}"
configuration="${BUILD_CONFIGURATION:-Release}"
serial="emulator-5554"
appium_pid=""
emulator_pid=""
logcat_pid=""
emulator_ready=false

cleanup() {
  local result=$?
  trap - EXIT INT TERM
  set +e
  if [[ "$emulator_ready" == true ]]; then
    python3 - "$serial" "$logs/device-properties.txt" <<'PYTHON'
import subprocess, sys
with open(sys.argv[2], 'w') as out:
    try:
        subprocess.run(['adb', '-s', sys.argv[1], 'shell', 'getprop'], stdout=out, stderr=subprocess.STDOUT, timeout=10)
    except subprocess.TimeoutExpired:
        out.write('Device properties collection timed out\n')
PYTHON
  fi
  local allure="Tests/UI/Mobile/bin/$configuration/net10.0/allure-results"
  if [[ -d "$allure" ]]; then
    cp -R "$allure/." "$MOBILE_CI_RESULTS/allure-results/"
    cp allure-categories.json "$MOBILE_CI_RESULTS/allure-results/categories.json"
  fi
  for owned_pid in "$logcat_pid" "$appium_pid" "$emulator_pid"; do
    if [[ -n "$owned_pid" ]]; then
      kill "$owned_pid" 2>/dev/null
      for attempt in {1..20}; do
        kill -0 "$owned_pid" 2>/dev/null || break
        sleep 0.2
      done
      if kill -0 "$owned_pid" 2>/dev/null; then kill -KILL "$owned_pid" 2>/dev/null; fi
      wait "$owned_pid" 2>/dev/null
    fi
  done
  rm -rf "$work"
  exit "$result"
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

sdkmanager="$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager"
avdmanager="$ANDROID_HOME/cmdline-tools/latest/bin/avdmanager"
[[ -x "$sdkmanager" && -x "$avdmanager" ]]
case "$(uname -m)" in
  x86_64) abi=x86_64 ;;
  arm64) abi=arm64-v8a ;;
  *) echo "Unsupported host architecture" >&2; exit 1 ;;
esac
image="system-images;android-34;google_apis;$abi"
# Finite input avoids yes exiting with SIGPIPE under pipefail.
printf 'y\n%.0s' {1..100} | "$sdkmanager" --licenses > "$logs/sdk-licenses.log" 2>&1
"$sdkmanager" --install platform-tools emulator "$image" > "$logs/sdk-install.log" 2>&1
printf 'no\n' | "$avdmanager" create avd -n ci_android_14 -k "$image" --force > "$logs/avd-create.log" 2>&1
# Note: hosted agents have fewer than 6 logical cores, and the emulator then always gives the guest 1 vCPU
# (run 110: -cores 2 and hw.cpu.ncore=2 were both overridden). Memory and devices can still be set.
cat >> "$ANDROID_AVD_HOME/ci_android_14.avd/config.ini" <<'AVD'
hw.ramSize=3072
hw.camera.back=none
hw.camera.front=none
AVD
cp "$ANDROID_AVD_HOME/ci_android_14.avd/config.ini" "$logs/avd-config.ini"

npm install --prefix "$work/appium" --no-save --no-package-lock "appium@$APPIUM_VERSION" > "$logs/appium-install.log" 2>&1
appium="$work/appium/node_modules/.bin/appium"
"$appium" driver install "uiautomator2@$UIAUTOMATOR2_VERSION" > "$logs/driver-install.log" 2>&1
{
  dotnet --version
  node --version
  java -version
  "$appium" --version
  "$appium" driver list --installed
  emulator -version
} > "$logs/tool-versions.txt" 2>&1

dotnet build Tests/UI/Mobile/Tests.UI.Mobile.csproj --configuration "$configuration" -p:TreatWarningsAsErrors=true
# The fresh hosted agent owns this port. Do not attach to an unrelated existing emulator.
if adb devices | awk '{print $1}' | grep -qx "$serial"; then
  echo "$serial already exists; refusing to take ownership" >&2
  exit 1
fi
emulator -accel-check > "$logs/acceleration.log" 2>&1
# Headless, software rendering without a window, 3 GB, cold boot (R32).
emulator -avd ci_android_14 -port 5554 -no-window -no-snapshot -no-audio -no-boot-anim -no-metrics \
  -gpu swiftshader_indirect -memory 3072 > "$logs/emulator.log" 2>&1 &
emulator_pid=$!
boot_timeout="${BOOT_TIMEOUT_SECONDS:-600}"
boot_start=$SECONDS
deadline=$((SECONDS + boot_timeout))
next_report=$((SECONDS + 30))
until [[ "$(adb -s "$serial" shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" == 1 ]]; do
  if ! kill -0 "$emulator_pid" 2>/dev/null; then
    echo "Emulator process exited during boot after $((SECONDS - boot_start)) seconds; see emulator.log" >&2
    exit 1
  fi
  if (( SECONDS >= deadline )); then
    echo "Emulator failed to boot within $boot_timeout seconds; see emulator.log and boot-progress.log" >&2
    exit 1
  fi
  if (( SECONDS >= next_report )); then
    # Shows how far boot got if it times out. The device is often offline here, so these adb calls must never
    # fail the script: with set -e and pipefail, a failing adb inside $(...) would exit silently (run 110).
    state="$(adb -s "$serial" get-state 2>/dev/null || echo offline)"
    anim="$( (adb -s "$serial" shell getprop init.svc.bootanim 2>/dev/null || true) | tr -d '\r')"
    echo "[boot] $((SECONDS - boot_start))s: adb=$state bootanim=${anim:-?}" | tee -a "$logs/boot-progress.log"
    next_report=$((SECONDS + 30))
  fi
  sleep 2
done
echo "[boot] Android booted in $((SECONDS - boot_start)) seconds" | tee -a "$logs/boot-progress.log"
emulator_ready=true
adb -s "$serial" logcat -v threadtime > "$logs/logcat.log" 2>&1 &
logcat_pid=$!
adb -s "$serial" shell input keyevent 82
adb -s "$serial" shell settings put global window_animation_scale 0
adb -s "$serial" shell settings put global transition_animation_scale 0
adb -s "$serial" shell settings put global animator_duration_scale 0
adb -s "$serial" shell pm path com.android.chrome | grep -q '^package:'

# R33: right after boot, Android keeps compiling apps (dex2oat) on its single vCPU, so app installs are slow and
# Appium's 20 s helper-app install timed out in run 115. Let the compilation settle (bounded), skip install
# verification, and install Appium's helper apps here with plain adb install, which has no 20 s limit.
settle_start=$SECONDS
settle_deadline=$((SECONDS + ${SETTLE_TIMEOUT_SECONDS:-240}))
idle_checks=0
while (( idle_checks < 2 && SECONDS < settle_deadline )); do
  if adb -s "$serial" shell pidof dex2oat64 dex2oat > /dev/null 2>&1; then
    idle_checks=0
  else
    idle_checks=$((idle_checks + 1))
  fi
  sleep 5
done
echo "[settle] background compilation $( (( idle_checks >= 2 )) && echo finished || echo 'still running') after $((SECONDS - settle_start)) seconds" | tee -a "$logs/boot-progress.log"
adb -s "$serial" shell settings put global verifier_verify_adb_installs 0
adb -s "$serial" shell settings put global package_verifier_enable 0

driver_dir="$APPIUM_HOME/node_modules/appium-uiautomator2-driver"
helper_apks=()
while IFS= read -r apk; do helper_apks+=("$apk"); done < <(find "$driver_dir" -path '*/apks/*.apk' | sort)
if (( ${#helper_apks[@]} == 0 )); then
  echo "No Appium helper APKs found under $driver_dir" >&2
  exit 1
fi
for apk in "${helper_apks[@]}"; do
  install_start=$SECONDS
  if adb -s "$serial" install -r -g --no-incremental "$apk" >> "$logs/preinstall.log" 2>&1; then
    echo "[preinstall] $(basename "$apk") in $((SECONDS - install_start)) s" | tee -a "$logs/boot-progress.log"
  else
    # Not fatal: Appium installs it again during session creation, with the longer timeouts below.
    echo "[preinstall] $(basename "$apk") FAILED after $((SECONDS - install_start)) s; see preinstall.log" | tee -a "$logs/boot-progress.log"
  fi
done

# R33 (run 118): inside the session Appium spent most of its time downloading a Chromedriver for the device's
# Chrome and starting Chrome for the first time, and the client's 120 s timeout ran out. Do both here instead.
# sed quits after the first match (no head -1, which could SIGPIPE under pipefail).
chrome_version="$( (adb -s "$serial" shell dumpsys package com.android.chrome 2>/dev/null || true) | sed -n '/versionName=/{s/.*versionName=\([0-9.]*\).*/\1/p;q;}')"
echo "[chrome] device Chrome $chrome_version" | tee -a "$logs/boot-progress.log"
case "$(uname -m)" in arm64) cft_platform=mac-arm64; legacy_zip=chromedriver_mac_arm64.zip ;; *) cft_platform=mac-x64; legacy_zip=chromedriver_mac64.zip ;; esac
# Chrome for Testing ships Chromedriver from 115 on; older versions are on the legacy index (same source Appium uses).
chromedriver_url="$(python3 - "$chrome_version" "$cft_platform" "$legacy_zip" <<'PY'
import json, sys, urllib.request
version, platform, legacy_zip = sys.argv[1:4]
parts = version.split(".")
try:
    if int(parts[0]) >= 115:
        builds = json.load(urllib.request.urlopen(
            "https://googlechromelabs.github.io/chrome-for-testing/latest-patch-versions-per-build-with-downloads.json", timeout=30))["builds"]
        entry = builds[".".join(parts[:3])]
        print(next(d["url"] for d in entry["downloads"]["chromedriver"] if d["platform"] == platform))
    else:
        latest = urllib.request.urlopen(f"https://chromedriver.storage.googleapis.com/LATEST_RELEASE_{parts[0]}", timeout=30).read().decode().strip()
        print(f"https://chromedriver.storage.googleapis.com/{latest}/{legacy_zip}")
except Exception as error:
    print(f"lookup failed: {error}", file=sys.stderr)
PY
)" || true
if [[ -n "$chromedriver_url" ]] && curl --silent --fail --location --max-time 120 -o "$work/chromedriver.zip" "$chromedriver_url"; then
  mkdir -p "$work/chromedriver"
  unzip -q -o "$work/chromedriver.zip" -d "$work/chromedriver"
  chromedriver_bin="$(find "$work/chromedriver" -type f -name chromedriver -print -quit)"
fi
if [[ -n "${chromedriver_bin:-}" ]]; then
  chmod +x "$chromedriver_bin"
  export MOBILE_Android__ChromedriverExecutable="$chromedriver_bin"
  echo "[chrome] Chromedriver $("$chromedriver_bin" --version 2>/dev/null | awk '{print $2}') from $chromedriver_url" | tee -a "$logs/boot-progress.log"
else
  # Not fatal: Appium falls back to downloading one itself (slower).
  echo "[chrome] could not pre-download a Chromedriver for $chrome_version (url: ${chromedriver_url:-none}); Appium will try" | tee -a "$logs/boot-progress.log"
fi

# Pre-warm Chrome: skip its first-run screens and let it create its profile now, not inside the session.
adb -s "$serial" shell 'echo "_ --disable-fre --no-default-browser-check --no-first-run" > /data/local/tmp/chrome-command-line'
adb -s "$serial" shell am set-debug-app --persistent com.android.chrome
warm_start=$SECONDS
adb -s "$serial" shell am start -W -a android.intent.action.VIEW -d about:blank com.android.chrome > "$logs/chrome-warmup.log" 2>&1 || true
warm_deadline=$((SECONDS + ${CHROME_WARMUP_TIMEOUT_SECONDS:-180}))
until adb -s "$serial" shell dumpsys window 2>/dev/null | grep -q 'mCurrentFocus=.*com.android.chrome'; do
  if (( SECONDS >= warm_deadline )); then break; fi
  sleep 3
done
sleep 5
adb -s "$serial" shell am force-stop com.android.chrome
echo "[chrome] warm-up took $((SECONDS - warm_start)) s" | tee -a "$logs/boot-progress.log"

if lsof -iTCP:4723 -sTCP:LISTEN >/dev/null 2>&1; then
  echo "Port 4723 is already in use; refusing to attach to an unrelated server" >&2
  exit 1
fi
"$appium" --address 127.0.0.1 --port 4723 --allow-insecure uiautomator2:chromedriver_autodownload > "$logs/appium.log" 2>&1 &
appium_pid=$!
deadline=$((SECONDS + 60))
until curl --silent --fail --max-time 5 http://127.0.0.1:4723/status | python3 -c 'import json,sys; sys.exit(0 if json.load(sys.stdin).get("value", {}).get("ready") is True else 1)' 2>/dev/null; do
  if ! kill -0 "$appium_pid" 2>/dev/null || (( SECONDS >= deadline )); then
    echo "Appium did not become ready within 60 seconds; see appium.log" >&2
    exit 1
  fi
  sleep 2
done

export APPIUM_URL=http://127.0.0.1:4723/
export MOBILE_Platform=Android MOBILE_AppType=Web
export MOBILE_Device__Udid="$serial" MOBILE_Device__DeviceName="$serial"
export MOBILE_Device__PlatformVersion=14 MOBILE_Android__BrowserName=Chrome
export LOGGING_PROVIDER=console
# R33: Appium's defaults (20 s for each adb command and the helper-app install) are too short for this emulator.
export MOBILE_Android__AdbExecTimeoutMs=120000
export MOBILE_Android__ServerInstallTimeoutMs=120000
export MOBILE_Android__ServerLaunchTimeoutMs=120000
# R33 (run 118): the .NET client's own command timeout must cover a slow session start (default 120 s).
export MOBILE_Server__CommandTimeoutSec=300

dotnet test Tests/UI/Mobile/Tests.UI.Mobile.csproj --configuration "$configuration" --no-build \
  --filter 'Category=smoke' --results-directory "$MOBILE_CI_RESULTS/trx" \
  --logger 'trx;LogFileName=mobile_results.trx'
# dotnet test can return success when a filter discovers no tests. Require actual execution.
python3 - "$MOBILE_CI_RESULTS/trx/mobile_results.trx" <<'PY'
import sys
import xml.etree.ElementTree as ET
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
counters = ET.parse(sys.argv[1]).find('.//t:Counters', ns)
if counters is None or int(counters.get('executed', '0')) == 0 or int(counters.get('passed', '0')) != int(counters.get('total', '0')):
    raise SystemExit('Mobile smoke run must execute and pass every discovered test')
print('Mobile smoke tests executed and passed:', counters.get('passed'))
PY
