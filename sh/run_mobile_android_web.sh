#!/usr/bin/env bash

# ================================================
# Mobile Web (Android) smoke run helper
# Prereqs:
#  - Android emulator/real device available via ADB
#  - Node.js with Appium v2 installed
#     npm i -g appium
#     appium driver install uiautomator2
#  - Appium server running on http://127.0.0.1:4723/
#  - Chrome installed on the device/emulator
# ================================================

set +e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT" || exit 1

echo
echo "[INFO] Mobile Web (Android) smoke test run starting..."
echo "[INFO] Ensure the following are running:"
echo "       - Android emulator or device (adb devices)"
echo "       - Appium server (appium)"
echo

if command -v adb >/dev/null 2>&1; then
  adb devices | awk 'NR > 1 && $1 != "" { print "[INFO] Detected device: " $1 }'
else
  echo "[WARN] adb was not found on PATH."
fi

dotnet test UI/Mobile/UI.Mobile.Tests.csproj \
  -v minimal \
  --filter "FullyQualifiedName~UI.Mobile.Tests.Samples.AndroidWeb.SmokeMobileWebTests"
EXIT_CODE=$?

if [[ "$EXIT_CODE" -ne 0 ]]; then
  echo
  echo "[ERROR] Tests failed with exit code $EXIT_CODE."
  exit "$EXIT_CODE"
fi

echo
echo "[INFO] Tests completed successfully."
