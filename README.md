# Modern Test Framework

<!-- Repo Metadata -->
<!-- Azure Pipelines badge: Pipelines → (pipeline) → ⋯ → Status badge, then paste the Markdown here -->
[![Issues](https://img.shields.io/github/issues/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/issues)
[![Last Commit](https://img.shields.io/github/last-commit/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/commits/main)

A modular C# / .NET 10 test automation framework. **Playwright** drives both the web UI and the REST API tests, **Appium** drives the Android mobile-web tests, and everything runs on **xUnit + Allure**, with pluggable logging (Console / Elasticsearch). Source lives on GitHub and CI/CD runs on Azure DevOps Pipelines.

---

## 🚀 Features

- **.NET 10** across all projects (`net10.0`, nullable enabled)
- **Playwright for UI and API**
  - **Web UI**: [saucedemo.com](https://www.saucedemo.com/) and the [Playwright TodoMVC demo](https://demo.playwright.dev/todomvc), with async page objects and auto-retrying `Expect(...)` assertions, on Chromium, Chrome, Edge, Firefox or WebKit
  - **REST API**: [Restful Booker](https://restful-booker.herokuapp.com/) through Playwright's `APIRequestContext` (no RestSharp)
- **Mobile tests** (`Tests/UI/Mobile`) with Appium 2: Android mobile web against saucedemo.com, with iOS and native-app support in the driver bootstrap
- **xUnit + Allure everywhere** (`Allure.Xunit`): suites, tags, steps, timing parameters, a full-page screenshot on failure, and the browser in every UI test name
- **Three top-level folders**: `Core` (shared framework code), `Pages` (page objects), `Tests` (all tests: `Api/`, `UI/Web/`, `UI/Mobile/`), with one layout rule everywhere: `<Layer>/<App>/`, namespace = folder, one type per file
- **One configuration mechanism** (`Microsoft.Extensions.Configuration`) for every settings file: JSON, then environment variables, so any value (URLs, credentials, Appium server) can be overridden without editing committed files
- **Typed test data**: models in `Core/Models/<App>/`, a Builder + Object Mother per entity in `Core/TestData/<App>/` for every app, and `TestDataLoader` (JSON) ready for large data-driven scenario tables
- **Pluggable logging and result publishing**: providers (`console`, `elastic`) are discovered by reflection and picked in `logging-config.json`, with automatic fallback to console
- **CI/CD on Azure DevOps** (`azure-pipelines.yml`), triggered from GitHub, in a single job. PRs run API, TodoMVC and the SauceDemo smoke test; `main` runs everything.
- **Run scripts for Windows (`scripts/windows/`) and macOS/Linux (`scripts/unix/`)**: build → test → collect Allure results, including a run-every-browser script
- **Local Elasticsearch + Kibana stack** (`docker-compose.yml`) for the Elastic logging provider

---

## 📂 Project Structure

```plaintext
TestAutomationProj/
│
├── Core/                              # Shared framework code (Core.csproj; also used by Tests/UI/Mobile)
│   ├── Api/                           # ApiClient (Playwright APIRequestContext), ApiException
│   │   └── RestfulBooker/             # AuthClient (POST /auth → token → Cookie header)
│   ├── Configuration/                 # TestSettings (+ WebAppSettings, ApiSettings) + ConfigurationProvider
│   ├── Driver/                        # PlaywrightDriverFactory (chromium/chrome/msedge/firefox/webkit)
│   ├── Logging/                       # ILoggingService, LoggingServiceFactory, ResultsPublisherFactory, LogMetadata,
│   │   │                              # LoggingConfig (+ sections) + LoggingConfigProvider (logging-config.json)
│   │   ├── Console/                   # console provider
│   │   └── Elastic/                   # Elasticsearch provider
│   ├── Models/                        # Data shapes, one folder per app
│   │   ├── RestfulBooker/             # Booking, BookingDates, BookingPatch, CreatedBooking, BookingId, Auth*
│   │   ├── SauceDemo/                 # User, Product, CheckoutInfo
│   │   └── Todo/                      # TodoItem
│   ├── TestData/                      # TestDataLoader (JSON) + Builders / Object Mothers, one folder per app
│   │   ├── RestfulBooker/             # BookingBuilder, BookingMother
│   │   ├── SauceDemo/                 # User/Product/CheckoutInfo Builder + Mother
│   │   └── Todo/                      # TodoItemBuilder, TodoItemMother
│   └── Testing/                       # TestRunner, TestRunInfo: logging, Allure timing, failure hook, publishing
│
├── Pages/                             # Playwright page objects (Pages.csproj)
│   ├── Common/BasePage.cs
│   ├── SauceDemo/                     # LoginPage, InventoryPage, CartPage, Checkout* pages
│   └── Todo/TodoPage.cs
│
├── Tests/                             # All tests
│   ├── Tests.csproj                   # Playwright tests: Api/ + UI/Web/ (one project, run in CI)
│   ├── Api/                           # BookingApi{Get,Post,Update,Patch,Delete,HealthCheck}Tests
│   ├── UI/
│   │   ├── Web/                       # Playwright web UI tests, one folder per app with a <App>TestBase
│   │   │   ├── SauceDemo/             # SauceDemoTestBase + SauceDemo, AddToCart, RemoveFromCart, CheckoutFlow, NegativeLogin
│   │   │   └── Todo/                  # TodoTestBase + TodoTests
│   │   └── Mobile/                    # Appium suite (own project: Tests.UI.Mobile.csproj, refs Core)
│   │       ├── Configuration/         # MobileSettings (+ sections) + MobileSettingsProvider
│   │       ├── Driver/                # MobileDriverFactory, MobileOptionsBuilder, MobilePlatform, MobileAppType
│   │       ├── Interactions/          # WaitHelper, GestureHelper
│   │       ├── Pages/                 # Common/MobileBasePage, SauceDemo/LoginPage
│   │       ├── Infrastructure/        # MobileTestBase
│   │       ├── SauceDemo/             # SauceDemoTests
│   │       ├── mobilesettings.json    # Appium server, platform, device, android/ios, behavior
│   │       └── package.json           # npm scripts: appium, appium-doctor, driver install
│   ├── Fixtures/                      # PlaywrightFixture (browser per class), ApiFixture
│   ├── Infrastructure/                # UiTestBase, ApiTestBase
│   └── testsettings.json              # Browser, headless, timeouts, app URLs, API credentials
│
├── scripts/
│   ├── windows/                       # .bat run scripts, Appium env check/fix (.ps1)
│   └── unix/                          # .sh equivalents for macOS/Linux
├── docs/                              # ARCHITECTURE, MOBILE_ARCHITECTURE, GETTING_STARTED, CONTRIBUTING, REVIEW
├── azure-pipelines.yml                # Azure DevOps pipeline (the only CI)
├── ci.slnf                            # Solution filter CI builds: Core, Pages, Tests
├── .editorconfig                      # C# code style (file-scoped namespaces, usings, LF)
├── .gitattributes                     # Line endings: LF, CRLF for .bat
├── Directory.Build.props              # Shared project settings (net10.0, nullable, code style in build, warnings as errors in CI)
├── Directory.Packages.props           # Central NuGet versions for every project
├── allure-categories.json             # Allure categories (UI vs API by tag)
├── logging-config.json                # Global logging provider configuration
├── docker-compose.yml                 # Local Elasticsearch 8 + Kibana (ports 9200 / 5601, no auth)
├── CHANGELOG.md
├── LICENSE                            # MIT
└── TestAutomationPlatform.slnx        # The solution: Core, Pages, Tests, Tests/UI/Mobile
```

Dependencies: `Tests` → `Pages` → `Core`; `Tests/UI/Mobile` → `Core`. `Tests.csproj` excludes the `Tests/UI/Mobile` folder, which is a separate project. Every project's assembly name equals its root namespace (`Core`, `Pages`, `Tests`, `Tests.UI.Mobile`). The mobile suite keeps its own `Pages/` because the top-level `Pages` project is built on Playwright.

NuGet versions are managed centrally in `Directory.Packages.props`. Projects reference packages without a `Version`, so to upgrade a package, change it there. Selenium packages are only there because Appium's .NET client (`Tests/UI/Mobile`) is built on Selenium WebDriver.

Code style for `.cs` files comes from `.editorconfig`: file-scoped namespaces that match the folder, `using`s above the namespace (`System` first, no ones already covered by implicit usings), LF line endings. `dotnet format` applies it, and the build enforces it (`EnforceCodeStyleInBuild` in `Directory.Build.props`). In CI (`TF_BUILD`/`CI` set) every warning, including style ones, fails the build.

---

## 🧰 Prerequisites

- [.NET SDK 10.x](https://dotnet.microsoft.com/download)
- Browsers for the Playwright tests. Either install Playwright's own builds after the first build:

  ```bash
  dotnet build Tests/Tests.csproj
  pwsh Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium   # or: install (all browsers)
  ```

  or use a browser you already have, with `BROWSER=chrome` or `BROWSER=msedge`. CI uses `chrome`.
- Optional: the [Allure CLI](https://allurereport.org/docs/install/) for viewing reports
- For the Mobile suite: Node.js, Appium 2 (`npm i -g appium`), the UiAutomator2 driver (`appium driver install uiautomator2`), Android SDK and a running emulator or device. On Windows, `scripts/windows/appium_env_check.ps1` diagnoses the setup and `scripts/windows/appium_env_fix.ps1` repairs common problems.
- Optional: Elasticsearch 8 for the `elastic` logging provider. `docker compose up -d` starts a local single-node Elasticsearch on `http://localhost:9200` (security off, no password) and Kibana on `http://localhost:5601`. OpenSearch doesn't work: Elastic's 8.x client gets HTTP 406 on every write.

Step-by-step setup for **Windows, macOS and Linux**, with what each platform can and can't run, is in [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md).

---

## ⚡ Quick Start

```bash
# Restore & build everything
dotnet build TestAutomationPlatform.slnx

# All Playwright tests (web UI + API)
dotnet test Tests/Tests.csproj

# Only the API tests / only the web UI tests
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tests.Api."
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName!~Tests.Api."

# Another browser, with the window visible
BROWSER=webkit HEADLESS=false dotnet test Tests/Tests.csproj

# Smoke tests only
dotnet test Tests/Tests.csproj --filter "Category=smoke"

# Mobile web smoke tests (Appium server + Android emulator must be running)
dotnet test Tests/UI/Mobile/Tests.UI.Mobile.csproj

# View Allure results after a run (or use the scripts below, which collect them into ./allure-results)
allure serve Tests/bin/Debug/net10.0/allure-results
```

---

## ⚙️ Configuration

All three settings files load the same way (`Core.Configuration.SettingsLoader`, built on `Microsoft.Extensions.Configuration`), once per run, with later sources winning:

1. the JSON file (comments allowed),
2. environment variables with the file's prefix, using `__` between nested keys,
3. a few short aliases for the most common settings.

| File | Loaded by | Prefix | Example | Aliases |
|---|---|---|---|---|
| `Tests/testsettings.json` | `ConfigurationProvider.Current` | `TESTS_` | `TESTS_RestfulBooker__Password=…` | `BROWSER`, `HEADLESS` |
| `logging-config.json` | `LoggingConfigProvider.Current` | `LOGGING_` | `LOGGING_PROVIDER=console`, `LOGGING_Elastic__Url=…` | `ELASTIC_URL`, `ELASTIC_USERNAME`, `ELASTIC_PASSWORD`, `ELASTIC_CERT_FINGERPRINT` |
| `Tests/UI/Mobile/mobilesettings.json` | `MobileSettingsProvider.Current` | `MOBILE_` | `MOBILE_Device__Udid=…` | `APPIUM_URL`, `DEVICE_NAME` |

Keys are case-insensitive. Secrets (passwords, tokens) should come only from the environment or a CI secret variable.

### Playwright tests: `Tests/testsettings.json`

```json
{
  "Browser": "chromium",
  "Headless": true,
  "DefaultTimeoutMs": 30000,
  "TodoMvc": { "BaseUrl": "https://demo.playwright.dev/todomvc/" },
  "SauceDemo": { "BaseUrl": "https://www.saucedemo.com/" },
  "RestfulBooker": {
    "BaseUrl": "https://restful-booker.herokuapp.com/",
    "Username": "admin",
    "Password": "password123"
  }
}
```

| Setting | Values | Environment override |
|---------|--------|----------------------|
| `Browser` | `chromium` (Playwright's bundled build), `chrome` / `msedge` (installed Google Chrome / Microsoft Edge, no download), `firefox`, `webkit` | `BROWSER` |
| `Headless` | `true` / `false` | `HEADLESS` |
| `DefaultTimeoutMs` | Playwright's default timeout per page | `TESTS_DefaultTimeoutMs` |
| `<App>.BaseUrl`, `RestfulBooker.Username/Password` | | `TESTS_SauceDemo__BaseUrl`, `TESTS_RestfulBooker__Password`, … |

Base URLs end with `/`, and entry pages open `./` (the context's `BaseURL`), so an app can live under a sub-path such as `/todomvc/`.

### Mobile: `Tests/UI/Mobile/mobilesettings.json`

Same pattern: `MobileSettingsProvider.Current` loads `mobilesettings.json` from the output directory once, then `MOBILE_*` variables and the `APPIUM_URL` / `DEVICE_NAME` aliases. It holds `Platform` (`Android`/`iOS`), `AppType` (`Web`/`Native`), `BaseUrl`, and `Server`, `Device`, `Android`, `IOS` and `Behavior` sections. See [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md) for every field.

### Test data

Every app uses the same pieces:

| What | Where | Use it for | Example |
|---|---|---|---|
| **Model** | `Core/Models/<App>/` | The shape (API contract or test-data record) | `Booking`, `User`, `TodoItem` |
| **Builder** | `Core/TestData/<App>/<Entity>Builder.cs` | A valid object by default; `With…` to vary one field | `new BookingBuilder().WithDepositPaid(false).Build()` |
| **Object Mother** | `Core/TestData/<App>/<Entity>Mother.cs` | Named, ready-made objects built on the builder | `BookingMother.Standard()`, `UserMother.LockedOut()`, `ProductMother.Backpack()`, `TodoItemMother.Completed()` |
| **`[MemberData]`** | the test class | `[Theory]` cases (input + expected result), built from Builders/Mothers | `NegativeLoginTests.InvalidLogins()` |

```csharp
var booking = BookingMother.Standard();                       // unique last name: no collisions on the shared demo API
var inventory = await LoginAsync(UserMother.Standard());
var blank = new UserBuilder().WithPassword("").Build();     // a variation for one [MemberData] row
```

Where uniqueness matters, builders generate it (`BookingBuilder`'s last name, `TodoItemBuilder`'s title), so tests running in parallel never collide. Models use PascalCase properties and map to JSON names with `[JsonPropertyName]`. `ApiClient` uses the same serializer options for requests and responses, and leaves out `null` properties, which is how `BookingPatch` sends only the fields that are set.

**When to use JSON instead.** The repo currently has no JSON test data. `TestDataLoader` (and the `Tests/test-data/**` copy rule in `Tests.csproj`) is there for when one of these applies:

- a `[Theory]` table grows large enough that adding a case should not need a code change,
- people who don't write C# maintain the scenarios,
- large or captured payloads (recorded API responses, contract fixtures),
- data that differs per environment.

Then put it at `Tests/test-data/<App>/<TestClass>/<Scenario>.json` with a matching record in `Core/Models/<App>/`, and load it with `TestDataLoader.Load<T>("<App>/<TestClass>/<Scenario>.json")`. Secrets never go in either. They belong in settings or environment variables.

---

## 🧾 Logging (Pluggable Providers)

The Playwright tests (`Tests/`) and the Mobile suite read logging settings from `logging-config.json`. `LoggingConfigProvider.Current` loads it once, finding the file by walking up from the test run directory until it reaches a folder containing `logging-config.json`, and then applies environment overrides.

```json
{
  "Provider": "elastic",
  "Elastic": {
    "Url": "http://localhost:9200",
    "Username": "",
    "Password": "",
    "CertificateFingerprint": ""
  },
  "Console": {
    "Enabled": true
  }
}
```

| Setting | Environment override | Notes |
|---|---|---|
| `Provider` | `LOGGING_PROVIDER` | `elastic` or `console` (any other key works with the `LOGGING_` prefix too, e.g. `LOGGING_Elastic__Url`) |
| `Elastic.Url` | `ELASTIC_URL` | |
| `Elastic.Username` / `Elastic.Password` | `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` | **Only set these in the environment** (or a CI secret variable); keep them empty in the file. The local `docker compose` stack needs none. |
| `Elastic.CertificateFingerprint` | `ELASTIC_CERT_FINGERPRINT` | CA fingerprint for `ON_LOCALHOST_SECURE`; when empty, the local self-signed certificate is accepted |
| connection profile | `ELASTIC_SERVER` | `ON_CLOUD`, `ON_LOCALHOST_SECURE`, `ON_LOCALHOST_INSECURE` (default) |

Provider selection, implemented by `LoggingServiceFactory` and `ResultsPublisherFactory`:

1. Use the provider named in `Provider` (`elastic` or `console`) if it can be created.
2. If it can't, fall back to `console`.
3. If console can't be created either, try any other discovered provider. As a last resort, use a built-in console logger.

The `elastic` provider probes connectivity first, **once per URL per test run** (cached). If Elasticsearch is unreachable, logging falls back to the console and the run continues without further delay. Loggers (one per index) and the results publisher are also created once and reused.

```csharp
var logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
logger.Info("message");
```

Index names in use: `ui-web-logs-{date}` (web UI tests), `test-logs-{date}` (API tests) and `ui-mobile-logs-{date}` (Mobile).

> Never commit Elasticsearch credentials. `logging-config.json` keeps them empty; use `ELASTIC_USERNAME` / `ELASTIC_PASSWORD`.

---

## 📣 Results Publishing

`UiTestBase`, `ApiTestBase` and `MobileTestBase` publish one `LogMetadata` record per test through `ResultsPublisherFactory.Create()`, from `Core`'s `TestRunner` (see Test Infrastructure below). The record contains project (`ui-web`, `api` or `ui-mobile`), class, method, status, duration, failure reason, start/end time, run name, user, and browser. A publish failure is logged and never fails the test.

- `Provider=elastic`: results are indexed into Elasticsearch
- `Provider=console`: results are printed to the console

---

## 📊 Allure Reporting

- Every suite uses **`Allure.Xunit`**, which turns on automatically when the tests run, and writes to `bin/<Configuration>/net10.0/allure-results/`. The run scripts collect the results (plus `categories.json`) into `./allure-results`, and CI publishes them as the `allure-results` artifact.
- Allure attributes come from `Allure.Net.Commons.Attributes`. Every test class has the same labels: `[AllureParentSuite("API" | "UI Web" | "UI Mobile")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`. Steps use `AllureApi.Step(...)`.
- `TestRunner` adds **Start Time**, **End Time**, and **Duration (s)** parameters to every test.
- `UiTestBase` appends the browser to the test name, e.g. `EndToEnd_Checkout_Success (chrome)`, records it as a `browser` parameter (so per-browser runs stay separate when merged), and attaches a full-page screenshot when a test fails. `MobileTestBase` appends the platform and app type, e.g. `(Android Web)`.
- A failed Playwright `Expect(...)` is reported as an xUnit assertion failure, so Allure shows it as **failed** rather than **broken**.
- `allure-categories.json` groups results into **UI Tests** and **API Tests** by the `ui` / `api` tags.

> **Version pin:** the solution stays on **xUnit v2** with **`xunit.runner.visualstudio` 2.8.2** (see `Directory.Packages.props`). `Allure.Xunit` doesn't support xUnit v3, and test adapters from 3.0 on can't load Allure's reporter, so you'd get no Allure results.

### Tags and categories in use

Each test class carries `[AllureTag(...)]` (for Allure) and xUnit `[Trait("Category", ...)]` (for `dotnet test --filter "Category=..."`). Tags always follow the same order: layer, (`mobile`), app, feature, categories.

| Suite | Allure tags | `Category` traits |
|-------|-------------|-------------------|
| API (`Tests/Api`) | `api`, `restful-booker`, `health`/`get`/`post`/`put`/`patch`/`delete`, `smoke`, `regression` | `smoke` (health check only), `regression` (all) |
| SauceDemo (`Tests/UI/Web/SauceDemo`) | `ui`, `saucedemo`, `login`/`negative`/`add-to-cart`/`remove-from-cart`/`checkout`, `smoke`, `regression` | `smoke` (login only), `regression` (all) |
| TodoMVC (`Tests/UI/Web/Todo`) | `ui`, `todomvc`, `todo-items`, `regression` | `regression` |
| Mobile (`Tests/UI/Mobile`) | `ui`, `mobile`, `saucedemo`, `login`, `smoke` | `smoke` |

---

## 🧪 Running Tests via Scripts

Each `scripts\windows\*.bat` script has a `scripts/unix/*.sh` equivalent for macOS/Linux; both can be run from any directory. The `run_*` scripts build `Tests/Tests.csproj`, run it, stamp `run.timestamp` and the browser into `environment.properties`, and collect the Allure results plus `categories.json` into `./allure-results`. They honour `BROWSER` and `HEADLESS`.

| Script | What it does |
|--------|--------------|
| `run_all_tests_gen_report` | All Playwright tests (web UI + API) |
| `run_api_tests_gen_report` | API tests only (`FullyQualifiedName~Tests.Api.`) |
| `run_web_tests_gen_report` | Web UI tests only (SauceDemo + TodoMVC) |
| `run_ui_web_all_browsers` | Web UI tests once per browser in `BROWSERS` (default `chromium firefox webkit`), merged into one result set with an `environment.<browser>.properties` per browser |
| `_run_playwright_tests` | Shared helper the scripts above call (`<label> [filter]`) |
| `run_mobile_android_web` | Runs the mobile `smoke` tests (`Tests.UI.Mobile.csproj`; needs Appium + emulator running) |
| `appium_env_check.ps1` / `appium_env_fix.ps1` | Windows only: diagnose / repair the Appium + Android environment (Java, Android SDK, PATH) |

---

## 🔁 CI/CD

Code lives on **GitHub**; **Azure DevOps Pipelines** is the only CI/CD. The pipeline is connected to the GitHub repo through the Azure Pipelines GitHub app, so pushes and PRs on GitHub trigger it, and its results show up as checks on the GitHub PR.

### Triggers

| Event | Tests run (Chrome) |
|-------|--------------------|
| PR into `main` | API + TodoMVC (all), SauceDemo `smoke` (`--filter "FullyQualifiedName!~.SauceDemo.\|Category=smoke"`) |
| Push to `main` or `release/*` | Everything, plus code coverage |

### Job (`azure-pipelines.yml`)

Everything runs in a **single job** (`Tests`, `ubuntu-latest`):

1. Shallow checkout, .NET 10 SDK
2. Build `ci.slnf` (Release): `Core`, `Pages` and `Tests`. Mobile is skipped.
3. `dotnet test Tests/Tests.csproj` with `BROWSER=chrome`, so Playwright drives the Google Chrome already on the hosted image and nothing is downloaded.
4. Publish TRX test results (failures fail the build), coverage (`main`/`release/*` only), and the `allure-results` artifact (with `categories.json`). To view it, download the artifact and run `allure serve <folder>`.

Why one job: the Azure DevOps org has a single parallel job, so separate jobs ran one after another, each repeating agent start-up, checkout, SDK setup and the build.

The Appium `Tests/UI/Mobile` suite isn't run in CI: it needs an Android emulator, which Microsoft-hosted Linux agents don't provide.

### Connecting the pipeline (one-time)

1. In Azure DevOps, go to **Pipelines → New pipeline → GitHub (YAML)**, then authorize or install the Azure Pipelines GitHub app for `ravs788/complete-test-automation-fmw`.
2. Choose **Existing Azure Pipelines YAML file** → `/azure-pipelines.yml`.
3. Optional: in GitHub **Settings → Branches**, protect `main` and require the Azure pipeline check before merging.

---

## 🛠️ Test Infrastructure

### `Core/Testing/TestRunner.cs`

xUnit v2 has no teardown hook that knows a test's name or whether it passed. So each base class wraps every test body in `TestRunner.Run`/`RunAsync`, which:

- logs the start, and on failure, the error
- runs a failure hook (UI: attach a screenshot to Allure) and then rethrows, so the test still fails
- always adds the Allure timing parameters, publishes the `LogMetadata` result, and runs cleanup

A test therefore looks like this:

```csharp
[Fact]
public Task CanAddBackpackToCart() => RunAsync(async () =>
{
    // Page, Settings, Logger, AllureApi.Step(...), Assertions.Expect(...)
});
```

### `Tests/Infrastructure/UiTestBase.cs` (web UI)

- Uses `PlaywrightFixture` (`IClassFixture`): **one browser per test class**, launched by `PlaywrightDriverFactory` for the configured `Browser`
- Gives each test a **new isolated `IBrowserContext` and `Page`** with `BaseURL` set to the app's `BaseUrl` (SauceDemo, TodoMVC) and the default timeout applied, and closes it after the test
- `RunAsync` adds the browser to the Allure name and parameters, attaches a full-page screenshot on failure, and turns a failed Playwright `Expect` into an xUnit assertion failure
- Each app has its own base: `SauceDemoTestBase` (sets `BaseUrl`, adds `LoginAsync(username, password)`) and `TodoTestBase` (sets `BaseUrl`, adds `OpenWithItemAsync(item)`)

### `Tests/Infrastructure/ApiTestBase.cs` (API)

- Uses `ApiFixture`: one Playwright instance per class, no browser
- Gives each test its own `ApiClient` (a Playwright `APIRequestContext` with `BaseURL` and `Accept: application/json`), disposed afterwards
- `GetAuthHeadersAsync()` gets a Restful Booker token and returns it as request headers (the `token` cookie), which tests pass to PUT, PATCH and DELETE. `ApiClient` itself knows nothing about any API's auth scheme: it just sends the headers it's given.
- `CreateBookingAsync(booking)` POSTs a booking and records its ID; after the test, `DisposeAsync` deletes every recorded booking (a failed cleanup is logged, never fails the test), so the shared demo API isn't left with test data
- `ApiClient` throws `ApiException` (with `StatusCode`) on non-2xx responses; `SendRawAsync` returns the raw response when a test wants to assert on the status itself (e.g. `404` after delete)

### `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs` (Appium)

- `Run(body)` reads `MobileSettingsProvider.Current`; `MobileOptionsBuilder` builds the `AppiumOptions` and `MobileDriverFactory` the Android or iOS driver
- For mobile web, navigates to `BaseUrl`. For native apps, launches the configured app or package.
- Records timing, logging, result publishing, and a failure screenshot through `TestRunner`
- Pages inherit `Pages/Common/MobileBasePage`. `GestureHelper` and `WaitHelper` (in `Interactions/`) handle swipes, taps, and waits.

---

## ➕ Adding Tests

### Web UI

1. Add a page object in `Pages/<App>/` (inherit `Pages.Common.BasePage`), exposing `ILocator`s for assertions and async intent methods for actions.
2. Add a model in `Core/Models/<App>/` and a Builder + Mother in `Core/TestData/<App>/`. Put `[Theory]` cases in a `[MemberData]` source built from them. Use JSON (`Tests/test-data/<App>/<TestClass>/<Scenario>.json` via `TestDataLoader`) only for large tables or data owned outside the code (see below) cases.
3. For a new app, add its URL to `testsettings.json` / `TestSettings` and a `Tests/UI/Web/<App>/<App>TestBase.cs` that inherits `UiTestBase` and overrides `BaseUrl`.
4. Add a test class in `Tests/UI/Web/<App>/` that inherits the app's test base (takes `PlaywrightFixture`) and carries `[AllureParentSuite("UI Web")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("ui", "<app>", "<feature>", ...)]` and `[Trait("Category", "regression")]` (add `smoke` if it belongs in PR runs). Write each test as `[Fact] public Task X() => RunAsync(async () => { ... });` and assert with `Assertions.Expect(...)`.

### API

1. Add models in `Core/Models/<Service>/`, a Builder + Mother per entity in `Core/TestData/<Service>/`, and any service-specific helpers (auth, …) in `Core/Api/<Service>/`.
2. Add a test class in `Tests/Api/` that inherits `ApiTestBase` (takes `ApiFixture`), with `[AllureParentSuite("API")]`, `[AllureSuite("<Service>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("api", "<service>", ...)]` and `[Trait("Category", ...)]`. Use `Client.GetAsync/PostAsync/PutAsync/PatchAsync/DeleteAsync` and `GetAuthHeadersAsync()` (for another API, put its auth helper in `Core/Api/<Service>/` and return headers the same way).

### Mobile

1. Add a page in `Tests/UI/Mobile/Pages/<App>/`, inheriting `MobileBasePage`.
2. Add a test class in `Tests/UI/Mobile/<App>/` that inherits `MobileTestBase`, with `[AllureParentSuite("UI Mobile")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("ui", "mobile", "<app>", ...)]` and `[Trait("Category", ...)]`. Write each test as `[Fact] public void X() => Run(() => { ... });`.

See also [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) for code-style rules.

---

## ⚠️ Known Issues / TODO

- **Mobile tests aren't in CI.** Hosted agents have no Android emulator. Running them would need a self-hosted agent or a device cloud.
- **Logging index names differ by suite** (`ui-web-logs-*`, `test-logs-*` for API, `ui-mobile-logs-*`). They were left unchanged so existing Elastic dashboards keep working.
- **Playwright Firefox may not start on some Macs** ("Could not find profile folder"). Reinstall it with `playwright.ps1 install --force firefox` or use another browser; CI uses Chrome.
- **`LICENSE`** still has the `[Year] [Your Name]` placeholders.

---

## 📖 Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the layer diagram, the test-run flow, and the design decisions behind the UI stacks. The mobile layer has its own design doc: [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md). Release notes are in [CHANGELOG.md](CHANGELOG.md). Open review findings (not yet fixed) are tracked in [docs/REVIEW.md](docs/REVIEW.md).

---

## 🤝 Contributing

Contributions and suggestions are welcome. Open an issue or a PR. Before submitting, run the relevant suite locally (`dotnet test …` or the scripts in `scripts/`) and follow `.editorconfig` and [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md).

---

## 📄 License

MIT. See [LICENSE](LICENSE).
