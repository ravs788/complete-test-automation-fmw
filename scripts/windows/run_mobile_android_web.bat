@echo off
setlocal ENABLEDELAYEDEXPANSION

REM ================================================
REM Mobile Web (Android) smoke run helper
REM Prereqs:
REM  - Android emulator/real device available via ADB
REM  - Node.js with Appium v2 installed
REM     npm i -g appium
REM     appium driver install uiautomator2
REM  - Appium server running on http://127.0.0.1:4723/
REM  - Chrome installed on the device/emulator
REM ================================================

echo.
echo [INFO] Mobile Web (Android) smoke test run starting...
echo [INFO] Ensure the following are running:
echo        - Android emulator or device (adb devices)
echo        - Appium server (appium)
echo.

REM Optionally display connected devices
for /f "skip=1 tokens=1" %%i in ('adb devices') do (
  if NOT "%%i"=="" (
    echo [INFO] Detected device: %%i
  )
)

REM Run only the Android Web sample tests
dotnet test UI\Mobile\UI.Mobile.Tests.csproj -v minimal --filter "FullyQualifiedName~UI.Mobile.Tests.Samples.AndroidWeb.SmokeMobileWebTests"
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% NEQ 0 (
  echo.
  echo [ERROR] Tests failed with exit code %EXIT_CODE%.
  exit /b %EXIT_CODE%
) else (
  echo.
  echo [INFO] Tests completed successfully.
)

endlocal
