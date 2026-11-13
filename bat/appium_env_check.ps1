# Appium Android environment diagnostic for Windows
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\bat\appium_env_check.ps1

$ErrorActionPreference = 'SilentlyContinue'

function Run([string]$cmd) {
  Write-Host ("> $cmd")
  try {
    Invoke-Expression $cmd 2>&1
  } catch {
    Write-Host ("ERROR: $($_.Exception.Message)")
  }
  Write-Host ""
}

Write-Host "===== Appium Android Environment Check (Windows) ====="

Write-Host "`n=== OS ==="
try {
  Get-CimInstance Win32_OperatingSystem | Select-Object Caption, Version, OSArchitecture | Format-List
} catch {
  cmd /c ver
}

Write-Host "`n=== Environment Variables ==="
Write-Host ("JAVA_HOME=" + $env:JAVA_HOME)
Write-Host ("ANDROID_HOME=" + $env:ANDROID_HOME)
Write-Host ("ANDROID_SDK_ROOT=" + $env:ANDROID_SDK_ROOT)
Write-Host ("PATH contains adb: " + ([bool](Get-Command adb -ErrorAction SilentlyContinue)))
Write-Host ("PATH contains node: " + ([bool](Get-Command node -ErrorAction SilentlyContinue)))
Write-Host ("PATH contains appium: " + ([bool](Get-Command appium -ErrorAction SilentlyContinue)))

Write-Host "`n=== Virtualization / Hypervisor (summary) ==="
Run 'systeminfo | Select-String -Pattern "Hyper-V Requirements","A hypervisor has been detected"'
Run 'DISM /Online /Get-Features /Format:Table | findstr /i "Hyper-V HypervisorPlatform VirtualMachinePlatform"'

Write-Host "`n=== Java/JDK ==="
Run 'java -version'
Run 'where java'

Write-Host "`n=== Node/NPM ==="
Run 'node -v'
Run 'npm -v'
Run 'where node'
Run 'where npm'

Write-Host "`n=== Appium Server ==="
Run 'appium -v'
Run 'where appium'
Run 'appium driver list'

Write-Host "`n=== Appium Doctor (if installed) ==="
Run 'appium-doctor --android'

Write-Host "`n=== Android SDK / Emulator ==="
$emu = Join-Path $env:LOCALAPPDATA 'Android\Sdk\emulator\emulator.exe'
$adb = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
Write-Host ("Emulator path: " + $emu + " (exists: " + (Test-Path $emu) + ")")
Write-Host ("ADB path: " + $adb + " (exists: " + (Test-Path $adb) + ")")
if (Test-Path $emu) {
  Run "`"$emu`" -version"
  Run "`"$emu`" -list-avds"
  Run "`"$emu`" -accel-check"
}
if (Test-Path $adb) {
  Run "`"$adb`" version"
  Run "`"$adb`" devices -l"
}

Write-Host "`n=== dotnet/Gradle (for building test binaries/apps) ==="
Run 'dotnet --info'
Run 'where dotnet'
Run 'gradle -v'
Run 'where gradle'

Write-Host "`n=== Java Keystore (optional for signing) ==="
Run 'keytool -list -help'

Write-Host "`n=== Summary Hints ==="
Write-Host "- If appium/appium-doctor missing: npm install -g appium appium-doctor"
Write-Host "- Ensure ANDROID_SDK_ROOT points to your SDK (e.g. C:\Users\%USERNAME%\AppData\Local\Android\Sdk)"
Write-Host "- Ensure JAVA_HOME points to JDK (e.g. C:\Program Files\Java\jdk-17)"
Write-Host "- Use x86_64 AVD and Graphics=Automatic/Hardware when using WHPX"
