# Mobile Suite (Appium)

The mobile suite lives in `Tests/UI/Mobile` as its own project (`Tests.UI.Mobile.csproj`). It uses Appium 2 through the Appium .NET client, which is built on Selenium WebDriver. It keeps its own pages (rather than using the top-level `Pages/` project) because `Pages` is built on Playwright and mobile pages use Appium types. Like every other suite, it uses xUnit and Allure, and gets logging, timing and results publishing from `Core` (`Core.Logging`, `Core.Testing.TestRunner`).

- **Status:** Android mobile web works (one SauceDemo smoke test). iOS and native apps are supported by the driver code but have no tests yet.
- **Not in CI:** hosted agents have no Android emulator. See the platform matrix in [GETTING_STARTED.md](GETTING_STARTED.md).

---

## Layout

It follows the same conventions as the Playwright side: namespace = folder (`Tests.UI.Mobile.<Folder>`), one type per file, settings record + provider, pages per app and tests per app.

```text
Tests/UI/Mobile/
├── Tests.UI.Mobile.csproj
├── mobilesettings.json            # Appium server, platform, device, android/ios, behavior
├── Configuration/                 # MobileSettings (+ Server/Device/Android/IOS/Behavior settings records)
│                                  # MobileSettingsProvider (lazy, cached; reads mobilesettings.json)
├── Driver/                        # MobilePlatform, MobileAppType, MobileOptionsBuilder, MobileDriverFactory
├── Interactions/                  # WaitHelper (explicit waits), GestureHelper (tap/swipe/scroll)
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
| `Core.Configuration.TestSettings` + `ConfigurationProvider` (`testsettings.json`) | `MobileSettings` + `MobileSettingsProvider` (`mobilesettings.json`) |
| `Core.Driver.PlaywrightDriverFactory` | `Driver/MobileDriverFactory` (+ `MobileOptionsBuilder`) |
| `Pages.Common.BasePage`, `Pages/<App>/` | `Pages/Common/MobileBasePage`, `Pages/<App>/` |
| `Tests/Infrastructure/UiTestBase` (`RunAsync`) | `Infrastructure/MobileTestBase` (`Run`) |
| `Tests/UI/Web/<App>/<Feature>Tests.cs` | `Tests/UI/Mobile/<App>/<Feature>Tests.cs` |

---

## How a test runs

```text
SauceDemoTests.SauceDemo_LoginPage_ShouldLoad()
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

The file is copied to the output directory. Property names match the C# records. Missing values fall back to the defaults in `Configuration/*.cs`. Any value can be overridden from the environment with the `MOBILE_` prefix (`MOBILE_Device__Udid`, `MOBILE_Platform=iOS`), and `APPIUM_URL` / `DEVICE_NAME` are short aliases for `Server.Url` / `Device.DeviceName`.

| Section | Keys | Notes |
|---|---|---|
| (root) | `Platform`, `AppType`, `BaseUrl` | `Android` / `iOS`; `Web` / `Native`; `BaseUrl` is opened for Web |
| `Server` | `Url`, `CommandTimeoutSec`, `NewCommandTimeoutSec` | Default Appium URL `http://127.0.0.1:4723/` |
| `Device` | `DeviceName`, `PlatformVersion`, `Udid`, `Orientation` | `DeviceName` should match `adb devices` (e.g. `emulator-5554`) |
| `Android` | `AutomationName`, `App`, `AppPackage`, `AppActivity`, `BrowserName`, `ChromedriverExecutable`, `ChromedriverExecutableDir`, `ChromedriverArgs`, `GeckodriverExecutable`, `FirefoxPackage`, `FirefoxActivity` | Leave both Chromedriver settings empty to let Appium download a matching Chromedriver |
| `IOS` | `AutomationName`, `App`, `BundleId`, `BrowserName` | Needs macOS + Xcode |
| `Behavior` | `NoReset`, `FullReset`, `AcceptAlertsAutomatically` | |

Logging is configured in the repository-wide `logging-config.json`, like the other suites.

---

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

- An emulator job in CI (self-hosted agent or a device cloud such as BrowserStack/Sauce Labs). Point `Server.Url` at the provider.
- Native Android and iOS sample tests.
