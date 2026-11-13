# Getting Started

How to set up and run the framework on **Windows**, **macOS** and **Linux**, and what each platform can and can't run.
For the short version, see "Quick Start" in the root README.

---

## 1  What runs where

| Suite / tool | Windows | macOS | Linux |
|---|---|---|---|
| API tests (`Tests/Api`, Playwright `APIRequestContext`) | ✅ | ✅ | ✅ |
| Web UI on Playwright's **Chromium** | ✅ | ✅ | ✅ ¹ |
| Web UI on **Firefox** / **WebKit** (Playwright builds) | ✅ | ✅ ² | ✅ ¹ |
| Web UI on installed **Google Chrome** (`BROWSER=chrome`) | ✅ | ✅ | ✅ x64 only ³ |
| Web UI on installed **Microsoft Edge** (`BROWSER=msedge`) | ✅ | ✅ | ✅ x64 only ³ |
| Mobile web on **Android** (Appium, `Tests/UI/Mobile`) | ✅ | ✅ | ✅ ⁴ |
| Mobile on **iOS** (Safari / native, XCUITest) | ❌ | ✅ ⁵ | ❌ |
| Desktop UI (`Tests/Desktop`) | Placeholder, no tests yet ⁶ | ❌ | ❌ |
| Run scripts | `scripts/windows/*.bat` | `scripts/unix/*.sh` | `scripts/unix/*.sh` |
| Appium environment check/fix scripts | ✅ `appium_env_*.ps1` | – (manual checks below) | – (manual checks below) |
| Allure report (`allure serve`) | ✅ | ✅ | ✅ |
| Local Elastic/OpenSearch logging (`docker compose`) | ✅ Docker Desktop | ✅ Docker Desktop / Colima | ✅ Docker Engine |
| CI (Azure Pipelines) | – | – | ✅ `ubuntu-latest`, Chrome |

1. Playwright's browser builds officially support **Ubuntu 22.04/24.04 and Debian 12**. They often work on other distros, but missing system libraries are up to you. `--with-deps` installs those libraries only on Debian and Ubuntu, and only with `sudo`.
2. WebKit needs a recent macOS release. Playwright drops older macOS versions over time, so if WebKit fails to launch, update macOS or use another browser.
3. Google Chrome and Microsoft Edge have no Linux **arm64** builds. On arm64 Linux (e.g. a Raspberry Pi, or an ARM VM on an Apple Silicon Mac), use `chromium`, `firefox` or `webkit`.
4. The Android emulator on Linux needs hardware acceleration (**KVM**). Inside a VM or container without nested virtualization, use a real device over `adb`.
5. iOS automation needs **Xcode**, which only runs on macOS. The driver code supports iOS, but the repo has no iOS tests yet.
6. A desktop suite (e.g. FlaUI / UI Automation) would be **Windows-only**. The project exists but has no code yet.

**Headed vs headless.** Headless (the default) works everywhere. `HEADLESS=false` needs a display: on Linux servers or CI, either stay headless or run under `xvfb-run`.

---

## 2  Common prerequisites

| Tool | Needed for | Version |
|---|---|---|
| .NET SDK | Everything | 10.0.x (`dotnet --version`) |
| PowerShell | Installing Playwright's browsers (`playwright.ps1`) | 7+ (`pwsh --version`) |
| Java (JRE) | The Allure CLI | 17+ (`java -version`) |
| Allure CLI | Viewing reports | 2.24+ (`allure --version`) |
| Node.js + Appium 2 | Mobile only | Node 20+, `appium --version` |
| Android SDK (platform-tools, emulator) | Mobile (Android) only | `adb version` |
| Docker | Local Elastic/OpenSearch only (optional) | `docker --version` |

You don't need PowerShell if you only use an installed Chrome or Edge (`BROWSER=chrome` / `msedge`), because nothing has to be downloaded. CI works this way.

---

## 3  Windows

### Windows: Install

```powershell
winget install Microsoft.DotNet.SDK.10
winget install Microsoft.PowerShell          # pwsh 7
winget install EclipseAdoptium.Temurin.17.JRE
scoop install allure                         # or: npm install -g allure-commandline
```

### Windows: Build and install browsers

```powershell
git clone https://github.com/ravs788/complete-test-automation-fmw.git
cd complete-test-automation-fmw
dotnet build TestAutomationPlatform.slnx
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium   # or "install" for chromium, firefox and webkit
```

### Windows: Run

```bat
scripts\windows\run_all_tests_gen_report.bat
scripts\windows\run_api_tests_gen_report.bat
scripts\windows\run_web_tests_gen_report.bat
scripts\windows\run_ui_web_all_browsers.bat
```

Pick a browser or show it. The syntax depends on the shell:

```bat
:: cmd.exe
set BROWSER=chrome
set HEADLESS=false
scripts\windows\run_web_tests_gen_report.bat
```

```powershell
# PowerShell
$env:BROWSER = "firefox"; $env:HEADLESS = "false"
dotnet test Tests/Tests.csproj
```

### Mobile (Android) on Windows

1. Install Android Studio and set `ANDROID_HOME` (e.g. `%LOCALAPPDATA%\Android\Sdk`). Add `platform-tools` and `emulator` to `PATH`.
2. `npm install -g appium` and then `appium driver install uiautomator2`.
3. Diagnose and fix common problems:
   `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\windows\appium_env_check.ps1`
   (and `appium_env_fix.ps1`).
4. Start an emulator, run `appium`, then `scripts\windows\run_mobile_android_web.bat`.

**Can't do on Windows:** iOS tests (they need macOS and Xcode).

---

## 4  macOS

### macOS: Install

```bash
brew install --cask dotnet-sdk          # .NET 10
brew install --cask powershell          # pwsh 7
brew install openjdk@17 allure
```

### macOS: Build and install browsers

```bash
git clone https://github.com/ravs788/complete-test-automation-fmw.git
cd complete-test-automation-fmw
dotnet build TestAutomationPlatform.slnx
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install chromium   # or "install" for all three
```

### macOS: Run

```bash
scripts/unix/run_all_tests_gen_report.sh
scripts/unix/run_api_tests_gen_report.sh
scripts/unix/run_web_tests_gen_report.sh
BROWSERS="chromium webkit" scripts/unix/run_ui_web_all_browsers.sh
BROWSER=webkit HEADLESS=false scripts/unix/run_web_tests_gen_report.sh
```

### Mobile on macOS

- **Android:** install Android Studio and set `ANDROID_HOME=~/Library/Android/sdk`. Add `platform-tools` and `emulator` to `PATH`. Then run `npm i -g appium && appium driver install uiautomator2`, start an emulator and `appium`, and run `scripts/unix/run_mobile_android_web.sh`. On Apple Silicon, use an **arm64** system image for the emulator.
- **iOS:** install Xcode and its command-line tools, then `appium driver install xcuitest`. Set `"Platform": "iOS"` (and `"AppType"`, `"IOS"`) in `Tests/UI/Mobile/mobilesettings.json`. There are no iOS tests in the repo yet, so you'd add them under `Tests/UI/Mobile/<App>/`.

**Can't do on macOS:** desktop (Windows UI Automation) tests.

---

## 5  Linux (Ubuntu / Debian)

### Linux: Install

```bash
# .NET 10: use Microsoft's packages or the install script (https://learn.microsoft.com/dotnet/core/install/linux)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
export PATH="$HOME/.dotnet:$PATH"

dotnet tool install --global PowerShell  # pwsh 7, if your distro has no package
sudo apt-get install -y openjdk-17-jre
npm install -g allure-commandline        # or download the Allure zip
```

### Linux: Build and install browsers

```bash
git clone https://github.com/ravs788/complete-test-automation-fmw.git
cd complete-test-automation-fmw
dotnet build TestAutomationPlatform.slnx
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium   # --with-deps uses sudo apt to add system libraries
```

### Linux: Run

Same scripts as macOS (`scripts/unix/*.sh`). They are committed as executable; if not, run `chmod +x scripts/unix/*.sh`.

```bash
scripts/unix/run_all_tests_gen_report.sh
BROWSER=firefox scripts/unix/run_web_tests_gen_report.sh
xvfb-run -a env HEADLESS=false scripts/unix/run_web_tests_gen_report.sh   # headed on a machine with no display
```

### Mobile (Android) on Linux

Install the Android SDK command-line tools and set `ANDROID_HOME`. Make sure KVM works (`ls /dev/kvm`; add your user to the `kvm` group). Then run `npm i -g appium && appium driver install uiautomator2`, start the emulator and Appium, and run `scripts/unix/run_mobile_android_web.sh`.

**Can't do on Linux:** iOS tests, desktop tests, and Chrome/Edge on arm64.

---

## 6  View the report

```bash
allure serve allure-results
```

This builds the report in a temporary folder and opens it in your browser. The run scripts collect the results into `./allure-results`.

---

## 7  Troubleshooting

**"Executable doesn't exist" / browser not found.** Playwright's browsers aren't installed for this Playwright version. Run the `playwright.ps1 install` command for your OS, or use `BROWSER=chrome`.

**Firefox won't start on macOS ("Could not find profile folder").** Run `pwsh Tests/bin/Debug/net10.0/playwright.ps1 install --force firefox`, or use another browser.

**Linux: "Host system is missing dependencies".** Re-run the install with `--with-deps`, or on a non-Debian distro install the listed libraries yourself.

**Mobile: session can't be created.** Check that `adb devices` lists the device and that Appium listens on `Server.Url` in `Tests/UI/Mobile/mobilesettings.json` (default `http://127.0.0.1:4723/`). Check that `Device.DeviceName` matches the device too. Leave `Android.ChromedriverExecutable` empty so Appium downloads a matching Chromedriver.

**"Elasticsearch connection refused".** No action needed: logging falls back to the console. To log to a cluster, start one (`docker compose up -d`) and set it in `logging-config.json`.

---

## 8  Next steps

- Read `Tests/UI/Web/SauceDemo/CheckoutFlowTests.cs`, `Pages/SauceDemo/` and `Tests/Api/BookingApiPostTests.cs`.
- See [ARCHITECTURE.md](ARCHITECTURE.md) for the design and [CONTRIBUTING.md](CONTRIBUTING.md) for where new code goes.
- Adjust `Tests/testsettings.json` (URLs, browser, credentials) and `Tests/UI/Mobile/mobilesettings.json` for your environment.
