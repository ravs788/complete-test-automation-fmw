# Getting Started

How to set up and run the framework on **Windows**, **macOS** and **Linux**, and what each platform can and can't run.
For the short version, see "Quick Start" in the root README.

---

## 1  What runs where

| Suite / tool | Windows | macOS | Linux |
|---|---|---|---|
| API tests (`Tests/Api`; Playwright `APIRequestContext` or `HttpClient`) | ✅ | ✅ | ✅ |
| Web UI on Playwright's **Chromium** | ✅ | ✅ | ✅ ¹ |
| Web UI on **Firefox** / **WebKit** (Playwright builds) | ✅ | ✅ ² | ✅ ¹ |
| Web UI on installed **Google Chrome** (`BROWSER=chrome`) | ✅ | ✅ | ✅ x64 only ³ |
| Web UI on installed **Microsoft Edge** (`BROWSER=msedge`) | ✅ | ✅ | ✅ x64 only ³ |
| Mobile web on **Android** (Appium, `Tests/UI/Mobile`) | ✅ | ✅ | ✅ ⁴ |
| Mobile on **iOS** (Safari / native, XCUITest) | ❌ | ✅ ⁵ | ❌ |
| Desktop UI (not in the framework yet) | Possible later ⁶ | ❌ | ❌ |
| Run scripts | `scripts/windows/*.bat` | `scripts/unix/*.sh` | `scripts/unix/*.sh` |
| Appium environment check/fix scripts | ✅ `appium_env_*.ps1` | – (manual checks below) | – (manual checks below) |
| Allure report (`allure serve`) | ✅ | ✅ | ✅ |
| Local Elasticsearch + Kibana logging (`docker compose`) | ✅ Docker Desktop | ✅ Docker Desktop / Colima | ✅ Docker Engine |
| CI (Azure Pipelines) | – | Android smoke on `macOS-15` (scheduled/manual; first run pending) | ✅ `ubuntu-latest`, Chrome |

1. Playwright's browser builds officially support **Ubuntu 22.04/24.04 and Debian 12**. They often work on other distros, but missing system libraries are up to you. `--with-deps` installs those libraries only on Debian and Ubuntu, and only with `sudo`.
2. WebKit needs a recent macOS release. Playwright drops older macOS versions over time, so if WebKit fails to launch, update macOS or use another browser.
3. Google Chrome and Microsoft Edge have no Linux **arm64** builds. On arm64 Linux (e.g. a Raspberry Pi, or an ARM VM on an Apple Silicon Mac), use `chromium`, `firefox` or `webkit`.
4. The Android emulator on Linux needs hardware acceleration (**KVM**). Inside a VM or container without nested virtualization, use a real device over `adb`.
5. iOS automation needs **Xcode**, which only runs on macOS. The driver code supports iOS, but the repo has no iOS tests yet.
6. There is no desktop suite yet. One would use FlaUI / UI Automation, which only works on **Windows**.

**Headed vs headless.** Headless (the default) works everywhere. `HEADLESS=false` needs a display: on Linux servers or CI, either stay headless or run under `xvfb-run`.

---

## 2  Common prerequisites

| Tool | Needed for | Version |
|---|---|---|
| .NET SDK | Everything | 10.0.x (`dotnet --version`) |
| PowerShell | Installing Playwright's browsers (`playwright.ps1`) | 7+ (`pwsh --version`) |
| Java (JRE) | The Allure CLI | 17+ (`java -version`) |
| Allure CLI | Viewing reports | 2.24+ (`allure --version`) |
| Node.js + Appium (2, 3 or 4) + UiAutomator2 driver | Mobile only | Node 20+, `appium --version`, `appium driver list --installed` |
| Android SDK (platform-tools, emulator) | Mobile (Android) only | `adb version` |
| Docker | Local Elasticsearch + Kibana only (optional) | `docker --version` |

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
  - **Driver version must match Appium:** `appium driver install uiautomator2` takes the newest driver, which needs the newest Appium (4.x). With an older Appium already installed, pick a matching driver: Appium 3 → `appium driver install uiautomator2@8.7.3`; Appium 2 → `uiautomator2@4.2.9` (what CI uses with Appium 2.19.0). Restart `appium` after installing a driver.
  - **SDK tools and an emulator from the command line (Homebrew):** `brew install --cask android-commandlinetools`, then install everything into the one SDK folder so `avdmanager` finds the images:
    ```bash
    export ANDROID_HOME=~/Library/Android/sdk
    yes | sdkmanager --sdk_root=$ANDROID_HOME --licenses
    sdkmanager --sdk_root=$ANDROID_HOME "cmdline-tools;latest" platform-tools emulator "system-images;android-34;google_apis;arm64-v8a"
    printf 'no\n' | $ANDROID_HOME/cmdline-tools/latest/bin/avdmanager create avd -n android_14 -k "system-images;android-34;google_apis;arm64-v8a" -d pixel_6
    $ANDROID_HOME/emulator/emulator -avd android_14 -port 5554 &
    ```
    The brew copy of `avdmanager` only looks in its own SDK folder, hence the `cmdline-tools;latest` install into `ANDROID_HOME`. `mobilesettings.json` expects Android 14 on `emulator-5554`; on an M-series Mac this boots in about a minute.
- **iOS:** install Xcode and its command-line tools, then `appium driver install xcuitest`. Set `"Platform": "iOS"` (and `"AppType"`, `"IOS"`) in `Tests/UI/Mobile/mobilesettings.json`. There are no iOS tests in the repo yet, so you'd add them under `Tests/UI/Mobile/<App>/`.

**Can't do on macOS:** desktop (Windows UI Automation) tests, if a desktop suite is added.

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

**Can't do on Linux:** iOS tests, Chrome/Edge on arm64 (and desktop tests, if a desktop suite is added).

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

**Mobile: session can't be created.** Confirm that `adb devices -l` lists the intended Android device with state `device` (not `offline` or `unauthorized`), and that Appium listens on `Server.Url` (default `http://127.0.0.1:4723/`). An installed AVD must be started and finish booting before it can be used.

To select a specific Android device, set `MOBILE_Device__Udid` to its exact serial from `adb devices -l`. `DEVICE_NAME` only sets `Device.DeviceName`; it does not override a nonempty `Device.Udid`. Match `Device.PlatformVersion` to the selected device as well.

macOS/Linux (replace the example serial):

```bash
adb devices -l
MOBILE_Device__Udid=emulator-5556 dotnet test Tests/UI/Mobile/Tests.UI.Mobile.csproj --filter "Category=smoke"
# Clear a previously exported override when it is no longer needed:
unset MOBILE_Device__Udid
```

Windows PowerShell:

```powershell
adb devices -l
$env:MOBILE_Device__Udid = "emulator-5556"
dotnet test Tests/UI/Mobile/Tests.UI.Mobile.csproj --filter "Category=smoke"
Remove-Item Env:MOBILE_Device__Udid -ErrorAction SilentlyContinue
```

Environment values override JSON. Removing the environment override restores the value from `Tests/UI/Mobile/mobilesettings.json`, so also clear any stale `Device.Udid` there to `""`. The shared default is empty (R29), allowing Appium to choose a matching available device; use an explicit UDID when more than one is connected. See the [mobile configuration reference](MOBILE_ARCHITECTURE.md) for the settings and aliases.

**Secrets from Azure Key Vault.** If settings use `secret://NAME` placeholders kept in Key Vault, run `az login`, add `"azure-keyvault"` to `Secrets` in `providers.json` (e.g. `"Secrets": ["environment", "azure-keyvault"]`, or `PROVIDERS_Secrets__1=azure-keyvault`) and set `KEYVAULT_URI=https://<vault>.vault.azure.net/` (Windows: `set` / `$env:`). Environment variables with the same name still take precedence. To log to the console instead of Elasticsearch, set `PROVIDERS_Logging=console`.

**Broker integration tests (optional).** `Tests/Unit` has RabbitMQ and Kafka listener tests that are skipped unless a broker is configured. To run them locally with Docker:

```bash
docker run -d --name rabbit -p 5672:5672 rabbitmq:3.13
docker run -d --name kafka -p 9092:9092 apache/kafka:3.8.0
RABBITMQ_URI=amqp://guest:guest@localhost:5672/ KAFKA_BOOTSTRAP=localhost:9092 \
  dotnet test Tests/Unit/Tests.Unit.csproj --filter "Category=integration"
```

**"Elasticsearch connection refused".** No action needed: logging falls back to the console. To log to a cluster, run `docker compose up -d` (Elasticsearch on `localhost:9200`, Kibana on `localhost:5601`, no password). For another cluster, set `ELASTIC_URL`, plus `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` if it needs them. Don't put credentials in `logging-config.json`.

---

## 8  Next steps

- Read `Tests/UI/Web/SauceDemo/CheckoutFlowTests.cs`, `Pages/SauceDemo/` and `Tests/Api/BookingApiPostTests.cs`.
- See [ARCHITECTURE.md](ARCHITECTURE.md) for the design and [CONTRIBUTING.md](CONTRIBUTING.md) for where new code goes.
- Adjust `Tests/testsettings.json` (URLs, browser, credentials) and `Tests/UI/Mobile/mobilesettings.json` for your environment.
