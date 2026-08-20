# Appium Android environment quick fix for Windows
# Usage:
#   - Set env vars only:  powershell -NoProfile -ExecutionPolicy Bypass -File .\bat\appium_env_fix.ps1
#   - Set env + install Appium: powershell -NoProfile -ExecutionPolicy Bypass -File .\bat\appium_env_fix.ps1 -InstallAppium

param(
  [switch]$InstallAppium = $false,
  [string]$JavaHome = $null,
  [string]$SdkRoot = $null
)

$ErrorActionPreference = 'SilentlyContinue'

function Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Ok($msg)   { Write-Host "[OK]  $msg" -ForegroundColor Green }
function Err($msg)  { Write-Host "[ERR] $msg" -ForegroundColor Red }

function Set-UserEnvVar([string]$name, [string]$value) {
  try {
    [Environment]::SetEnvironmentVariable($name, $value, 'User')
    Ok ("Set user env var {0}={1}" -f $name, $value)
    return $true
  } catch {
    Err "Failed to set ${name}: $($_.Exception.Message)"
    return $false
  }
}

function Ensure-UserPathEntry([string]$pathToAdd) {
  try {
    $existing = [Environment]::GetEnvironmentVariable('Path', 'User')
    if ([string]::IsNullOrEmpty($existing)) { $existing = '' }

    $separator = ';'
    $parts = $existing.Split($separator) | Where-Object { $_ -ne '' }
    $normalized = $parts | ForEach-Object { $_.TrimEnd('\').ToLowerInvariant() }
    $candidate = $pathToAdd.TrimEnd('\').ToLowerInvariant()

    if ($normalized -contains $candidate) {
      Info "PATH already contains: $pathToAdd"
      return $false
    }

    $newPath = if ($existing.EndsWith($separator) -or $existing -eq '') { "$existing$pathToAdd" } else { "$existing$separator$pathToAdd" }
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
    Ok "Added to user PATH: $pathToAdd"
    return $true
  } catch {
    Err "Failed to update PATH: $($_.Exception.Message)"
    return $false
  }
}

function Find-JdkHome {
  $candidates = @()
  $roots = @(
    'C:\Program Files\Java',
    'C:\Program Files\Eclipse Adoptium',
    'C:\Program Files\Microsoft',
    'C:\Program Files\Zulu',
    'C:\Program Files\AdoptOpenJDK',
    'C:\Program Files\Amazon Corretto',
    'C:\Program Files\BellSoft',
    'C:\Program Files (x86)\Java'
  )
  foreach ($root in $roots) {
    if (Test-Path $root) {
      Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Name -match 'jdk') {
          $candidates += $_.FullName
        }
      }
    }
  }
  # Also check Microsoft\jdk-* path
  $msJdk = Get-ChildItem -Path 'C:\Program Files\Microsoft' -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like 'jdk*' } | Select-Object -ExpandProperty FullName -ErrorAction SilentlyContinue
  if ($msJdk) { $candidates += $msJdk }

  # Filter those containing bin\java.exe
  $good = @()
  foreach ($c in $candidates | Select-Object -Unique) {
    if (Test-Path (Join-Path $c 'bin\java.exe')) { $good += $c }
  }
  # Prefer JDK 17+, then 11, etc.
  $ordered = $good | Sort-Object { 
    $v = ($_ -replace '[^\d\.]', '')
    if ($v) { [version]($v -replace '^(\d+)(\..*)?$', '$1.0') } else { [version]'0.0' }
  } -Descending
  if ($ordered.Count -gt 0) { return $ordered[0] } else { return $null }
}

function Find-AndroidSdk {
  $candidates = @()
  function __add([string]$p) { if ($p -and (Test-Path $p)) { $script:candidates += $p } }

  __add $env:ANDROID_SDK_ROOT
  __add $env:ANDROID_HOME
  __add (Join-Path $env:LOCALAPPDATA 'Android\Sdk')
  __add (Join-Path $env:USERPROFILE 'AppData\Local\Android\Sdk')
  __add (Join-Path ${env:ProgramFiles} 'Android\android-sdk')
  __add (Join-Path ${env:ProgramFiles(x86)} 'Android\android-sdk')
  __add 'C:\Android\Sdk'

  foreach ($c in $candidates | Select-Object -Unique) {
    if (Test-Path (Join-Path $c 'platform-tools\adb.exe')) { return $c }
  }

  $roots = @($env:USERPROFILE, $env:LOCALAPPDATA, ${env:ProgramFiles}, ${env:ProgramFiles(x86)})
  foreach ($r in $roots) {
    if (-not (Test-Path $r)) { continue }
    try {
      $pts = Get-ChildItem -LiteralPath $r -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -ieq 'platform-tools' } |
        Select-Object -ExpandProperty FullName -ErrorAction SilentlyContinue
      foreach ($pt in $pts) {
        if (Test-Path (Join-Path $pt 'adb.exe')) {
          return (Split-Path $pt -Parent)
        }
      }
    } catch {}
  }
  return $null
}

Write-Host "===== Appium Android Environment Quick Fix ====="

# Resolve SDK
$sdk = $null
if ($SdkRoot -and (Test-Path $SdkRoot)) {
  $sdk = $SdkRoot
} elseif ($env:ANDROID_SDK_ROOT -and (Test-Path $env:ANDROID_SDK_ROOT)) {
  $sdk = $env:ANDROID_SDK_ROOT
} elseif ($env:ANDROID_HOME -and (Test-Path $env:ANDROID_HOME)) {
  $sdk = $env:ANDROID_HOME
} else {
  $sdk = Find-AndroidSdk
}
if ($sdk -and (Test-Path $sdk)) {
  Info "Detected Android SDK at: $sdk"
  Set-UserEnvVar -name 'ANDROID_SDK_ROOT' -value $sdk | Out-Null
  Set-UserEnvVar -name 'ANDROID_HOME' -value $sdk | Out-Null
  $pt = Join-Path $sdk 'platform-tools'
  if (Test-Path $pt) { Ensure-UserPathEntry -pathToAdd $pt | Out-Null } else { Warn "platform-tools not found at $pt; open SDK Manager and install Platform Tools." }
  $bt = Get-ChildItem (Join-Path $sdk 'build-tools') -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending | Select-Object -First 1
  if ($bt) {
    $btPath = $bt.FullName
    Ensure-UserPathEntry -pathToAdd $btPath | Out-Null
  } else {
    Warn "No build-tools found in $sdk\build-tools; install via SDK Manager."
  }
} else {
  Warn ("Android SDK not found. Install via Android Studio SDK Manager. Expected at {0}\Android\Sdk" -f ${env:LOCALAPPDATA})
}

# Resolve JDK
$jdkHome = if ($JavaHome -and (Test-Path (Join-Path $JavaHome 'bin\java.exe'))) { $JavaHome } else { $env:JAVA_HOME }
if (-not $jdkHome -or -not (Test-Path (Join-Path $jdkHome 'bin\java.exe'))) {
  $jdkHome = Find-JdkHome
  if ($jdkHome) {
    Set-UserEnvVar -name 'JAVA_HOME' -value $jdkHome | Out-Null
    Ensure-UserPathEntry -pathToAdd (Join-Path $jdkHome 'bin') | Out-Null
  } else {
    Warn "JDK not found. Install a JDK (17 recommended). For example:
  - winget install EclipseAdoptium.Temurin.17.JDK
  - winget install Microsoft.OpenJDK.17"
  }
} else {
  Set-UserEnvVar -name 'JAVA_HOME' -value $jdkHome | Out-Null
  Info "JAVA_HOME set: $jdkHome"
  Ensure-UserPathEntry -pathToAdd (Join-Path $jdkHome 'bin') | Out-Null
}

# Node/npm/appium
$node = Get-Command node -ErrorAction SilentlyContinue
if (-not $node) {
  Warn "Node.js not found. Install from https://nodejs.org or with:
  winget install OpenJS.NodeJS.LTS"
} else {
  Info "Node: $($node.Source)"
}

$appium = Get-Command appium -ErrorAction SilentlyContinue
if (-not $appium) {
  Warn "Appium not found. Will install if -InstallAppium is specified."
} else {
  Info "Appium already present: $($appium.Source)"
}

if ($InstallAppium) {
  try {
    Info "Installing Appium and appium-doctor globally..."
    npm i -g appium appium-doctor
    Ok "Installed Appium and appium-doctor."
  } catch {
    Err "npm global install failed: $($_.Exception.Message)"
  }
}

Write-Host ""
Write-Host "Next steps:"
Write-Host "- Restart your terminal/VS Code to load updated environment variables."
Write-Host "- In Android Studio SDK Manager, ensure: Platform-Tools, Emulator, Build-Tools, and desired platforms are installed."
Write-Host "- Run the env check again: powershell -NoProfile -ExecutionPolicy Bypass -File .\bat\appium_env_check.ps1"
Write-Host "- Optionally install Appium now: powershell -NoProfile -ExecutionPolicy Bypass -File .\bat\appium_env_fix.ps1 -InstallAppium"
Write-Host ""
Write-Host "Done."
