# Modern Test Framework

<!-- Repo Metadata -->
[![Main Regression](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/main-regression.yml/badge.svg)](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/main-regression.yml)
[![PR Smoke](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/pr-smoke.yml/badge.svg)](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/pr-smoke.yml)
[![Issues](https://img.shields.io/github/issues/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/issues)
[![Last Commit](https://img.shields.io/github/last-commit/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/commits/main)

A modular C# / .NET 10 test automation framework covering REST API, Web UI, and Android mobile-web testing, with Allure reporting, pluggable logging (Console / Elasticsearch), and CI on both Azure DevOps and GitHub Actions.

---

## 🚀 Features

- **.NET 10** across all projects (`net10.0`, `LangVersion=latest`, nullable enabled)
- **Two web UI stacks side by side**
  - **Selenium + NUnit + Allure** (`UI/Web`) running against [saucedemo.com](https://www.saucedemo.com/) on Chrome, Firefox, and Edge
  - **Playwright + xUnit** (`src/` + `tests/`) running against the [Playwright TodoMVC demo](https://demo.playwright.dev/todomvc), using async page objects, web-first assertions, and an Object Mother/Builder for test data
- **Mobile tests** (`UI/Mobile`) with Appium 2 + NUnit: Android mobile web (Chrome/Firefox) against saucedemo.com, with iOS and native-app support in the driver bootstrap
- **REST API tests** (`API`) with RestSharp + NUnit against [Restful Booker](https://restful-booker.herokuapp.com/)
- **Page Object Model** in all UI suites
- **Selenium Grid support**: set `GridUrl` in `UI/Web/config.json` to run against a remote grid instead of local drivers
- **JSON test data** loaded by convention from each project's `test-data/` folder (`Core.Utilities.TestDataLoader`)
- **Per-project configuration**: each NUnit project owns its `config.json` plus a strongly typed `ConfigSettings` class, loaded through `Core.Utilities.ConfigLoader`
- **Pluggable logging and result publishing**: providers (`console`, `elastic`) are discovered by reflection and picked in `logging-config.json`, with automatic fallback to console
- **Allure reporting**: per-test start/end/duration parameters, screenshots on failure, tag-based categories
- **CI pipelines**: Azure DevOps (`azure-pipelines.yml`) and GitHub Actions (`.github/workflows/`)
- **Run scripts for Windows (`scripts/windows/`) and macOS/Linux (`scripts/unix/`)** for clean → build → test → Allure merge. The batch scripts use a bundled `tools/dotnet` if one exists and fall back to the system `dotnet` otherwise.
- **Local OpenSearch stack** (`docker-compose.yml`) for the Elastic logging provider

---

## 📂 Project Structure

```plaintext
TestAutomationProj/
│
├── Core/                              # Shared library for the NUnit (API + UI/Web) suites
│   ├── Models/                        # LoggingConfig, ElasticSection, ConsoleSection, LogMetadata
│   └── Utilities/
│       ├── Config/ConfigLoader.cs           # Loads <output>/config.json into a typed settings class
│       ├── TestData/                        # ITestDataLoader + TestDataLoader (reads <output>/test-data/...)
│       ├── Logging/
│       │   ├── Common/                      # ILoggingService, IResultsPublisher, provider factories + selectors
│       │   ├── Console/                     # Console logging + results publisher
│       │   └── Elastic/                     # Elasticsearch client, connectivity probe, logging + publisher
│       └── Screenshots/                     # IScreenshotHelper + Core stub (UI/Web supplies the real one)
│
├── API/                               # API test suite (NUnit + RestSharp + Allure)
│   ├── Models/                        # Booking, BookingDates, Auth
│   ├── Tests/                         # BaseApiTest + Get/Post/Put/Patch/Delete/HealthCheck tests
│   ├── Utilities/                     # ApiClient, AuthHelper, ConfigSettings
│   ├── test-data/<TestClass>/*.json
│   ├── config.json
│   └── API.Tests.csproj
│
├── UI/
│   ├── Web/                           # Selenium web suite (NUnit + Allure)
│   │   ├── Pages/                     # BasePage, LoginPage, InventoryPage, CartPage, Checkout* pages
│   │   ├── Models/                    # User, Product, CheckoutInfo, *TestData
│   │   ├── Tests/                     # BaseWebTest, SauceDemo, AddToCart, RemoveFromCart, CheckoutFlow, NegativeLogin
│   │   ├── Utilities/                 # AllureScreenshotHelper, AssertHelper, ConfigSettings
│   │   ├── test-data/<TestClass>/*.json
│   │   ├── config.json
│   │   └── UI.Web.Tests.csproj
│   ├── Mobile/                        # Appium mobile suite (NUnit + Allure)
│   │   ├── Drivers/Bootstrap.cs       # Builds AppiumOptions/driver for Android/iOS, Native/Web
│   │   ├── Pages/                     # Native/Android/AndroidBasePage, Web/MobileWebBasePage
│   │   ├── Tests/                     # BaseMobileTest + Samples/AndroidWeb smoke tests
│   │   ├── Utilities/                 # Typed config models, GestureHelper, WaitHelper
│   │   ├── config.json                # Appium server, platform, device, android/ios, behavior
│   │   └── package.json               # npm scripts: appium, appium-doctor, driver install
│   └── Desktop/                       # Placeholder (xUnit project, no tests yet)
│
├── src/                               # Playwright stack: libraries
│   ├── Automation.Core/
│   │   ├── Configuration/             # TestSettings record + ConfigurationProvider (lazy singleton)
│   │   ├── Driver/                    # PlaywrightDriverFactory (browser + isolated context)
│   │   └── TestData/                  # TodoItem, TodoItemBuilder, TodoItemMother
│   └── Automation.Pages/
│       ├── Common/BasePage.cs
│       └── Todo/TodoPage.cs
│
├── tests/                             # Playwright stack: tests
│   └── Automation.Tests/
│       ├── Fixtures/PlaywrightFixture.cs   # Shared browser per class, fresh context per test
│       └── Todo/TodoTests.cs
│
├── scripts/
│   ├── windows/                       # .bat run scripts, kill drivers, Appium env check/fix (.ps1)
│   └── unix/                          # .sh equivalents for macOS/Linux
├── docs/
│   ├── ARCHITECTURE.md                # Overall design
│   ├── MOBILE_ARCHITECTURE.md         # Mobile layer design + Appium setup
│   ├── GETTING_STARTED.md             # Step-by-step local setup + troubleshooting
│   └── CONTRIBUTING.md                # Code-style rules
├── .github/workflows/                 # GitHub Actions (PR smoke, main regression, manual runs)
├── azure-pipelines.yml                # Azure DevOps pipeline
├── Directory.Build.props              # Shared Allure results directory for all projects
├── Directory.Packages.props           # Central NuGet versions for every project
├── allure-categories.json             # Allure categories (UI vs API by tag)
├── logging-config.json                # Global logging provider configuration
├── docker-compose.yml                 # 2-node OpenSearch + Dashboards (ports 9200 / 5601)
├── package.json                       # Node deps used by the Appium tooling (appium-chromedriver)
├── CHANGELOG.md
├── LICENSE                            # MIT
└── TestAutomationPlatform.slnx        # The solution: all 8 projects
```

NuGet versions are managed centrally in `Directory.Packages.props`. Projects reference packages without a `Version`, so to upgrade a package, change it there.

Code style for `.cs` files comes from `.editorconfig` (4-space indent, LF line endings, UTF-8, and nullable warnings CS8600/CS8602/CS8603 raised to `warning`).

---

## 🧰 Prerequisites

- [.NET SDK 10.x](https://dotnet.microsoft.com/download)
- For the Selenium suite: Chrome, Firefox, and/or Edge installed locally. Selenium Manager resolves the drivers.
- For the Playwright suite: Playwright browsers, installed after the first build:

  ```bash
  dotnet build tests/Automation.Tests/Automation.Tests.csproj
  pwsh tests/Automation.Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
  ```
- Optional: the [Allure CLI](https://allurereport.org/docs/install/) for viewing reports
- For the Mobile suite: Node.js, Appium 2 (`npm i -g appium`), the UiAutomator2 driver (`appium driver install uiautomator2`), Android SDK and a running emulator or device. On Windows, `scripts/windows/appium_env_check.ps1` diagnoses the setup and `scripts/windows/appium_env_fix.ps1` repairs common problems. See [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md).
- Optional: Elasticsearch/OpenSearch for the `elastic` logging provider. `docker compose up -d` starts a local OpenSearch cluster on `http://localhost:9200`, with Dashboards on `http://localhost:5601`.

For a guided walkthrough, see [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md).

---

## ⚡ Quick Start

```bash
# Restore & build everything
dotnet build TestAutomationPlatform.slnx

# API tests (NUnit)
dotnet test API/API.Tests.csproj

# Selenium web tests (NUnit)
dotnet test UI/Web/UI.Web.Tests.csproj

# Mobile web smoke tests (Appium server + Android emulator must be running)
dotnet test UI/Mobile/UI.Mobile.Tests.csproj

# Playwright web tests (xUnit)
dotnet test tests/Automation.Tests/Automation.Tests.csproj

# View Allure results (after a run)
allure serve allure-results
```

---

## ⚙️ Configuration

### NUnit suites (API, UI/Web, UI/Mobile): per-project `config.json`

Each project copies its `config.json` and `test-data/**` to the build output. `ConfigLoader.Load<T>()` reads `config.json` from `AppContext.BaseDirectory`.

**UI/Web** (`UI/Web/config.json`):
```json
{
  "RunTestsInParallel": false,
  "Browser": "chrome",
  "Headless": false,
  "PageLoadTimeoutSec": 90,
  "BaseUrl": "https://www.saucedemo.com/"
}
```

Other optional `UI/Web` settings (from `ConfigSettings`): `GridUrl` (Selenium Grid; empty = local), `DriverCommandTimeoutSec` (60), `PageLoadTimeoutSec` (30), `ScriptTimeoutSec` (30), `ImplicitWaitTimeoutSec` (0), `ChromeDriverPath` / `FirefoxDriverPath` / `EdgeDriverPath` / `FirefoxBinaryPath` (empty = Selenium Manager resolves them).

**API** (`API/config.json`):
```json
{
  "BaseUrl": "https://restful-booker.herokuapp.com/",
  "DefaultUsername": "admin",
  "DefaultPassword": "password123"
}
```

Usage:
```csharp
using Core.Utilities;

var cfg = ConfigLoader.Load<UI.Web.Utilities.ConfigSettings>();
var baseUrl = cfg.BaseUrl;
```

**UI/Mobile** (`UI/Mobile/config.json`) holds the Appium server URL, `platform` (`Android`/`iOS`), `appType` (`Web`/`Native`), `baseUrl`, plus `device`, `android`, `ios`, `behavior`, `logging`, and `testData` sections. See [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md) for every field.

> In the Selenium suite the browser comes from each test's `[TestCase("chrome")]`, `[TestCase("firefox")]`, `[TestCase("edge")]` argument. `BaseWebTest` reads `Headless`, `BaseUrl`, timeouts, and driver paths from config. The `Browser` value in config is not used for driver selection. Headless mode is forced on when `GITHUB_ACTIONS=true`.

### Playwright suite: `TestSettings`

`Automation.Core.Configuration.TestSettings` is an immutable record exposed through `ConfigurationProvider.Current`, a thread-safe lazy singleton:

| Setting            | Default                                  |
|--------------------|------------------------------------------|
| `BaseUrl`          | `https://demo.playwright.dev/todomvc`    |
| `Browser`          | `chromium` (also `firefox`, `webkit`)    |
| `Headless`         | `true`                                   |
| `DefaultTimeoutMs` | `30000`                                  |

> These values are currently compile-time defaults. No file or environment variable is read yet, so the `HEADLESS` variable set in `azure-pipelines.yml` has no effect.

### Test data

```csharp
// Loads <output>/test-data/CheckoutFlowTests/EndToEnd_Checkout_Success.json
var data = TestDataLoader.Instance.Load<CheckoutFlowTestData>(
    "CheckoutFlowTests/EndToEnd_Checkout_Success.json");
```

Convention: `test-data/<TestClassName>/<Scenario>.json`.

The Playwright suite generates data instead of loading files. It uses `TodoItemMother.StandardItem()` / `TodoItemBuilder`, which produce GUID-based titles that can't collide across tests.

---

## 🧾 Logging (Pluggable Providers)

The NUnit suites read logging settings from `logging-config.json`. `LoggingConfig.Load()` finds this file by walking up from the test run directory until it reaches a folder containing `logging-config.json`.

```json
{
  "Provider": "elastic",
  "Elastic": {
    "Url": "http://localhost:9200",
    "Username": "elastic",
    "Password": "<your-password>"
  },
  "Console": {
    "Enabled": true
  }
}
```

Provider selection, implemented by `LoggingServiceFactory` and `ResultsPublisherFactory`:

1. Use the provider named in `Provider` (`elastic` or `console`) if it can be created.
2. If it can't, fall back to `console`.
3. If console can't be created either, try any other discovered provider. As a last resort, use a built-in console logger.

The `elastic` provider probes connectivity first. If Elasticsearch is unreachable, it disables itself and prints a single warning, and the run continues.

The `ELASTIC_SERVER` environment variable selects the Elastic connection profile: `ON_CLOUD`, `ON_LOCALHOST_SECURE`, or `ON_LOCALHOST_INSECURE` (the default).

```csharp
var logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
logger.Info("message");
```

Index names in use: `ui-web-logs-{date}` (UI/Web) and `test-logs-{date}` (API).

> Don't commit real Elasticsearch credentials. Keep a placeholder in `logging-config.json` and use local overrides.

---

## 📣 Results Publishing

`BaseWebTest` and `BaseApiTest` publish one `LogMetadata` record per test in `[TearDown]` through `ResultsPublisherFactory.Create()`. The record contains project, class, method, status, duration, failure reason, start/end time, run name, user, and browser. A publish failure is logged and never fails the test.

- `Provider=elastic`: results are indexed into Elasticsearch
- `Provider=console`: results are printed to the console

---

## 📊 Allure Reporting

- `Directory.Build.props` points `AllureResultsDirectory` (and the `ALLURE_*` environment variables) at a root-level `allure-results/` folder for all projects.
- Both NUnit base classes add **Start Time**, **End Time**, and **Duration (s)** parameters to every test.
- `BaseWebTest` appends the browser to the Allure test name, e.g. `EndToEnd_Checkout_Success (chrome)`, and attaches a screenshot when a test fails. `BaseMobileTest` appends the platform and app type, e.g. `(Android Web)`.
- `allure-categories.json` groups results into **UI Tests** and **API Tests** by the `ui` / `api` tags.
- The Playwright (xUnit) suite doesn't integrate with Allure yet. It reports through TRX.

### Tags and categories in use

Each test class carries `[AllureTag(...)]` (for Allure) and NUnit `[Category(...)]` (for `dotnet test --filter "Category=..."`, which CI uses).

| Suite     | Allure tags | NUnit categories |
|-----------|-------------|------------------|
| API       | `api`, `health`/`get`/`post`/`put`/`patch`/`delete`, `smoke`, `regression` | `smoke` (health check only), `regression` (all) |
| UI/Web    | `ui`, `saucedemo`/`add-to-cart`/`cart`/`checkout-flow`/`login`/`negative`, `smoke`, `regression` | `smoke` (SauceDemo login only), `regression` (all) |
| UI/Mobile | `mobile`, `web`, `android`, `smoke` | none yet |

---

## 🧪 Running Tests via Scripts

Run the Windows scripts from the repository root. Each `scripts\windows\*.bat` script has a `scripts/unix/*.sh` equivalent for macOS/Linux, and the shell scripts can be run from any directory. They detect the target framework with `scripts/unix/detect_net_version.sh`.

| Script                                   | What it does |
|------------------------------------------|--------------|
| `scripts\windows\run_all_tests_gen_report.bat` | Kills drivers, cleans, builds, runs API + UI/Web, stamps `run.timestamp`, and merges results into `allure-results/` |
| `scripts\windows\run_api_tests_gen_report.bat` | Runs the API suite only and copies results to `allure-results/` |
| `scripts\windows\run_web_tests_gen_report.bat` | Runs the UI/Web suite only and copies results to `allure-results/` |
| `scripts\windows\run_ui_web_all_browsers.bat`  | Runs UI/Web once per browser (chrome, firefox, edge), keeps per-browser result sets, then merges them |
| `scripts\windows\run_mobile_android_web.bat`   | Runs the Android mobile-web smoke tests (needs Appium + emulator running) |
| `scripts\windows\kill_all_webdrivers.bat`      | Kills orphaned `chromedriver` / `geckodriver` / `msedgedriver` processes |
| `scripts\windows\appium_env_check.ps1` / `appium_env_fix.ps1` | Diagnose / repair the Windows Appium + Android environment (Java, Android SDK, PATH) |

---

## 🔁 CI/CD

### Azure DevOps: `azure-pipelines.yml`

Triggers on pushes to `main`, `release/*`, and `add-elastic-logging`, and on PRs into `main`.

| Job | Agent | Steps |
|-----|-------|-------|
| **WebAndApiAutomation** | `ubuntu-latest` | .NET 10 SDK → restore/build `TestAutomationPlatform.slnx` (Release) → install Playwright Chromium → run **API** (NUnit) + **Playwright** (xUnit) tests with TRX + XPlat code coverage → publish test results (fails on test failure) and Cobertura coverage |
| **DesktopAutomation** | `windows-latest` | .NET 10 SDK → restore/build `UI/Desktop/UI.Desktop.Tests.csproj` → publish any `desktop_results.trx` |

The Selenium `UI/Web` and Appium `UI/Mobile` suites aren't run in Azure DevOps. On Azure, web coverage comes from the Playwright suite.

### GitHub Actions: `.github/workflows/`

| Workflow                    | Trigger                    | Runs |
|-----------------------------|----------------------------|------|
| `pr-smoke.yml`              | PR to `main`               | API + UI smoke via `reusable-test-runner.yml` |
| `main-regression.yml`       | Push to `main`             | API + UI regression via `reusable-test-runner.yml` |
| `reusable-test-runner.yml`  | `workflow_call`            | Sets up .NET 10, builds the chosen layer, runs `dotnet test --filter "Category=<tag>"`, then uploads TRX |
| `api-tests.yml`             | Manual (`workflow_dispatch`) | Full API suite and uploads Allure + TRX |
| `ui-tests.yml`              | Manual (`workflow_dispatch`) | Full UI/Web suite and uploads Allure + TRX |

---

## 🛠️ Test Infrastructure

### `UI/Web/Tests/BaseWebTest.cs` (Selenium / NUnit)

- Creates a Chrome, Firefox, or Edge driver from the test-case argument (default: Firefox), with headless mode taken from config (forced on in GitHub Actions)
- Uses a `RemoteWebDriver` against `GridUrl` when one is set. Otherwise it uses a configured driver path, falling back to Selenium Manager.
- Bypasses system proxies for the local WebDriver connection
- Applies the page-load, script, and implicit-wait timeouts, maximizes the window, and navigates to `BaseUrl`
- Records timing to Allure, logs, publishes the result, and takes a screenshot on failure
- Always quits and disposes the driver

### `API/Tests/BaseApiTest.cs` (NUnit)

- Creates a fresh `ApiClient` per test and disposes it in teardown
- Records timing to Allure, logs, and publishes the result
- The assembly runs fixtures in parallel (`[assembly: Parallelizable(ParallelScope.Fixtures)]`)

### `UI/Mobile/Tests/BaseMobileTest.cs` (Appium / NUnit)

- Parses `platform` / `appType` from config, and `Drivers/Bootstrap.cs` builds the `AppiumOptions` and the Android or iOS driver
- For mobile web, navigates to `baseUrl`. For native apps, launches the configured app or package.
- Records timing, logging, and result publishing the same way as the other NUnit bases
- Pages inherit `MobileWebBasePage` or `AndroidBasePage`. `GestureHelper` and `WaitHelper` handle swipes, taps, and waits.

### `tests/Automation.Tests/Fixtures/PlaywrightFixture.cs` (Playwright / xUnit)

- Shared through `IClassFixture<PlaywrightFixture>`: launches **one browser per test class**
- `CreateCleanPageAsync()` gives each test a **new isolated `IBrowserContext`** with `BaseURL` and the default timeout applied
- Tests close their own context in `DisposeAsync`. The fixture closes the browser.

---

## ➕ Adding Tests

### Selenium (UI/Web)

1. Add or extend a page object in `UI/Web/Pages/` (inherit `BasePage`).
2. Add test data at `UI/Web/test-data/<TestClass>/<Scenario>.json` and a matching model in `UI/Web/Models/`.
3. Create a test class that inherits `BaseWebTest`, decorated with `[AllureNUnit]`, `[AllureSuite]`, and `[AllureTag("ui", ...)]`, with one `[TestCase("<browser>")]` per browser.

### API

1. Add models in `API/Models/` and data in `API/test-data/<TestClass>/`.
2. Inherit `BaseApiTest` and use `_client` (`ApiClient`) and `AuthHelper` for authenticated calls.

### Mobile

1. Add a page in `UI/Mobile/Pages/Web/` or `UI/Mobile/Pages/Native/<Platform>/`, inheriting the matching base page.
2. Create a test class that inherits `BaseMobileTest`, with `[AllureTag("mobile", ...)]` and, once CI runs mobile tests, a `[Category]`.

### Playwright

1. Add a page object in `src/Automation.Pages/<Feature>/` (inherit `Automation.Pages.Common.BasePage`) and expose `ILocator`s for assertions.
2. Add builders/mothers for test data in `src/Automation.Core/TestData/`.
3. Add an xUnit class in `tests/Automation.Tests/<Feature>/` that implements `IClassFixture<PlaywrightFixture>, IAsyncLifetime`, and use `Assertions.Expect(...)` for auto-retrying assertions.

See also [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) for code-style rules.

---

## ⚠️ Known Issues / TODO

- **Release vs Debug artifact paths.** The manual GitHub workflows (`api-tests.yml`, `ui-tests.yml`) build in Release but upload Allure results from `bin/Debug/net10.0/...`.
- **Mobile config is machine-specific.** `UI/Mobile/config.json` has a hard-coded Windows `chromedriverExecutable` path and points to Appium on port `4725`. Adjust both for your machine.
- **Mobile tests have no NUnit `[Category]`**, so the CI smoke/regression filters skip them. No CI job runs mobile tests yet.
- **Playwright settings are hard-coded.** `TestSettings` doesn't read config files or environment variables yet.
- **The Desktop job has no test step.** `azure-pipelines.yml` builds `UI.Desktop.Tests` but never runs `dotnet test`, and the project contains no tests yet.
- **`LICENSE`** still has the `[Year] [Your Name]` placeholders.
- **`Core.csproj`** excludes `Utilities\SerilogLoggingService.cs`, which no longer exists. This is harmless but can be removed.

---

## 📖 Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the layer diagram, the test-run flow, and the design decisions behind the UI stacks. The mobile layer has its own design doc: [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md). Release notes are in [CHANGELOG.md](CHANGELOG.md).

---

## 🤝 Contributing

Contributions and suggestions are welcome. Open an issue or a PR. Before submitting, run the relevant suite locally (`dotnet test …` or the scripts in `scripts/`) and follow `.editorconfig` and [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md).

---

## 📄 License

MIT. See [LICENSE](LICENSE).
