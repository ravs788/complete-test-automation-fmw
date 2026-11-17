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
# Hosted agents have fewer than 6 logical cores, so the emulator would give the guest a single vCPU,
# which is too slow to boot Android 14 (R32). Ask for 2 cores and 3 GB in the AVD as well as on the command line.
cat >> "$ANDROID_AVD_HOME/ci_android_14.avd/config.ini" <<'AVD'
hw.cpu.ncore=2
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
# Headless, software rendering without a window, 2 cores / 3 GB, cold boot (R32).
emulator -avd ci_android_14 -port 5554 -no-window -no-snapshot -no-audio -no-boot-anim -no-metrics \
  -gpu swiftshader_indirect -cores 2 -memory 3072 > "$logs/emulator.log" 2>&1 &
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
    # Shows how far boot got if it times out: adb state, boot animation service, package manager readiness.
    state="$(adb -s "$serial" get-state 2>/dev/null || echo offline)"
    anim="$(adb -s "$serial" shell getprop init.svc.bootanim 2>/dev/null | tr -d '\r')"
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
