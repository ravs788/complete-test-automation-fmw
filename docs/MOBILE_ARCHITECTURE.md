# Mobile Suite (Appium)

The mobile suite lives in `Tests/UI/Mobile` as its own project (`Tests.UI.Mobile.csproj`). It uses Appium 2 through the Appium .NET client, which is built on Selenium WebDriver. It keeps its own pages (rather than using the top-level `Pages/` project) because `Pages` is built on Playwright and mobile pages use Appium types. Like every other suite, it uses xUnit and Allure, and gets logging, timing and results publishing from `Core` (`Core.Logging`, `Core.Testing.TestRunner`).

The reusable Appium bootstrapping (settings + provider, driver factory/options, wait and gesture helpers) lives in a separate library, **`Core.Mobile`** (R38), so any mobile suite can reference it while `Core` stays free of Appium/Selenium. Page objects, the test base and the tests stay in `Tests/UI/Mobile`.

- **Status:** Android mobile web works (one SauceDemo smoke test). iOS and native apps are supported by the driver code but have no tests yet.
- **CI:** all runs compile the project in the Linux job. Scheduled/manual runs also execute Android smoke in a separate `macOS-15` job using `scripts/unix/run_mobile_ci.sh`; results and diagnostics are published as `mobile-results`. As of run 118 (2026-11-17) the emulator boots and Appium starts a session; the remaining session timeout (Chromedriver download and Chrome's first start inside the session) is addressed by the Chromedriver pre-download and Chrome warm-up below (R33), pending a manual run to confirm. See [GETTING_STARTED.md](GETTING_STARTED.md).
- **Hosted emulator:** `scripts/unix/run_mobile_ci.sh` boots it headless (`-no-window -gpu swiftshader_indirect`) with 3 GB. Hosted agents have fewer than 6 cores, so the emulator always gives Android a single vCPU (it ignores `-cores`), which makes the first boot slow and uneven (446–505 s in runs 115/118, more than 600 s in run 127). The boot budget is `BOOT_TIMEOUT_SECONDS` (script default 600; the pipeline sets 900 and gives the job 45 minutes), and progress is logged to `boot-progress.log` in the `mobile-results` artifact. After boot it waits for background app compilation to settle, then pre-installs Appium's helper apps (`[settle]` / `[preinstall]` lines in `boot-progress.log`), because on one core Appium's 20 s install limit is too short (R33). Before Appium starts it also reads the device's Chrome version, pre-downloads the matching Chromedriver (legacy index below Chrome 115, Chrome for Testing from 115) and passes it as `MOBILE_Android__ChromedriverExecutable`, and pre-warms Chrome once with first-run screens disabled (`[chrome]` lines). The .NET client's command timeout is raised to 300 s (`MOBILE_Server__CommandTimeoutSec`) on CI.

---

## Layout

It follows the same conventions as the Playwright side: namespace = folder (`Core.Mobile.<Folder>` / `Tests.UI.Mobile.<Folder>`), one type per file, settings record + provider, pages per app and tests per app.

```text
Core/Mobile/                       # reusable Appium bootstrapping (Core.Mobile.csproj → Core)
├── Configuration/                 # MobileSettings (+ Server/Device/Android/IOS/Behavior/Resilience sections)
│                                  # MobileSettingsProvider (lazy, cached; reads mobilesettings.json)
├── Driver/                        # MobilePlatform, MobileAppType, MobileOptionsBuilder, MobileDriverFactory
└── Interactions/                  # WaitHelper (explicit waits), GestureHelper (tap/swipe/scroll)

Tests/UI/Mobile/                   # the mobile test suite (Tests.UI.Mobile.csproj → Core.Mobile)
├── Tests.UI.Mobile.csproj
├── mobilesettings.json            # Appium server, platform, device, android/ios, behavior, resilience
├── Pages/
│   ├── Common/MobileBasePage.cs   # waits + retrying SafeClick/SafeType/SafeGetText, Load, GetPageTitle
│   └── SauceDemo/LoginPage.cs
├── Infrastructure/
│   └── MobileTestBase.cs          # session lifecycle, screenshot on failure, results publishing
└── SauceDemo/
    └── SauceDemoTests.cs
```

| Playwright side | Mobile equivalent |
|---|---|
| `Core.Configuration.TestSettings` + `ConfigurationProvider` (`testsettings.json`) | `Core.Mobile.Configuration.MobileSettings` + `MobileSettingsProvider` (`mobilesettings.json`) |
| `Core.Driver.PlaywrightDriverFactory` | `Core.Mobile.Driver.MobileDriverFactory` (+ `MobileOptionsBuilder`) |
| `Pages.Common.BasePage`, `Pages/<App>/` | `Pages/Common/MobileBasePage`, `Pages/<App>/` |
| `Tests/Infrastructure/UiTestBase` (`RunAsync`) | `Infrastructure/MobileTestBase` (`Run`) |
| `Tests/UI/Web/<App>/<Feature>Tests.cs` | `Tests/UI/Mobile/<App>/<Feature>Tests.cs` |

---

## How a test runs

```text
SauceDemoTests.LoginPage_Should_Load()
  └─ MobileTestBase.Run(body)  →  Core.Testing.TestRunner.Run(...)
        1. MobileSettingsProvider.Current          (mobilesettings.json + MOBILE_* / APPIUM_URL / DEVICE_NAME, loaded once)
        2. MobileOptionsBuilder.Parse/Build        → (Android|iOS, Web|Native) → AppiumOptions
        3. MobileDriverFactory.CreateDriver        → AndroidDriver / IOSDriver (Appium server at Server.Url)
        4. Web: navigate to BaseUrl · Native: the app is launched from the capabilities
        5. Allure test name gets " (Android Web)"
        6. body() — pages + assertions
        on failure: screenshot attached to Allure
        always: result published (ResultsPublisherFactory), driver quit
```

---

## Configuration: `mobilesettings.json`

The file is copied to the output directory. Property names match the C# records. Missing values fall back to the defaults in `Core/Mobile/Configuration/*.cs`. Any value can be overridden from the environment with the `MOBILE_` prefix (`MOBILE_Device__Udid`, `MOBILE_Platform=iOS`), and `APPIUM_URL` / `DEVICE_NAME` are short aliases for `Server.Url` / `Device.DeviceName`.

| Section | Keys | Notes |
|---|---|---|
| (root) | `Platform`, `AppType`, `BaseUrl` | `Android` / `iOS`; `Web` / `Native`; `BaseUrl` is opened for Web |
| `Server` | `Url`, `CommandTimeoutSec`, `NewCommandTimeoutSec` | Default Appium URL `http://127.0.0.1:4723/` |
| `Device` | `DeviceName`, `PlatformVersion`, `Udid`, `Orientation` | `Udid` selects an Android serial from `adb devices -l`; override it with `MOBILE_Device__Udid`. `DEVICE_NAME` changes only `DeviceName`, not `Udid`. The shared UDID default is empty |
| `Android` | `AutomationName`, `App`, `AppPackage`, `AppActivity`, `BrowserName`, `ChromedriverExecutable`, `ChromedriverExecutableDir`, `ChromedriverArgs`, `GeckodriverExecutable`, `FirefoxPackage`, `FirefoxActivity`, `AdbExecTimeoutMs`, `ServerInstallTimeoutMs`, `ServerLaunchTimeoutMs` | Leave both Chromedriver settings empty to let Appium download a matching Chromedriver The three timeouts are Appium's UiAutomator2 limits (defaults 20 s / 20 s / 30 s); CI raises them to 120 s. |
| `IOS` | `AutomationName`, `App`, `BundleId`, `BrowserName` | Needs macOS + Xcode |
| `Behavior` | `NoReset`, `FullReset`, `AcceptAlertsAutomatically` | |

Logging is configured in the repository-wide `logging-config.json`, like the other suites.

---

For device selection commands and clearing stale overrides, see [mobile troubleshooting](GETTING_STARTED.md).

## Adding tests

1. **Page:** add `Pages/<App>/<Name>Page.cs` deriving from `MobileBasePage`, with locators as private `By` fields and intent methods/queries as public members (keep assertions in tests).
2. **Test:** add `<App>/<Feature>Tests.cs` deriving from `MobileTestBase`, with the same labels as the other suites:

   ```csharp
   [AllureParentSuite("UI Mobile")]
   [AllureSuite("<App>")]
   [AllureSubSuite("<Feature>")]
   [AllureTag("ui", "mobile", "<app>", "<feature>", "<category>")]
   [Trait("Category", "smoke")]   // and/or "regression"
   public class <Feature>Tests : MobileTestBase
   {
       [Fact]
       public void Something_Should_Happen() => Run(() => { ... });
   }
   ```

3. **Native or iOS:** switch `Platform` / `AppType` (and the `Android.App*` / `IOS.*` values) in `mobilesettings.json`. The same base classes apply. Use `GestureHelper` for swipes and scrolls.

---

## Running

Prerequisites per OS are in [GETTING_STARTED.md](GETTING_STARTED.md). In short:

```bash
npm i -g appium && appium driver install uiautomator2   # xcuitest for iOS
appium                                                   # in its own terminal
emulator -avd <your_avd>                                 # or a device on adb
scripts/unix/run_mobile_android_web.sh                  # scripts\windows\run_mobile_android_web.bat
```

The scripts run `dotnet test Tests/UI/Mobile/Tests.UI.Mobile.csproj --filter "Category=smoke"`.

---

## Next steps

- Confirm the first hosted macOS Android smoke run, then consider a broader device/browser matrix.
- Native Android and iOS sample tests.
