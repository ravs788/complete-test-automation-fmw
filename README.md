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
  - **REST API**: [Restful Booker](https://restful-booker.herokuapp.com/) through `ApiClient` on a pluggable HTTP transport: Playwright's `APIRequestContext` (default) or .NET `HttpClient` (`"Http"` in `providers.json`; no RestSharp)
- **Mobile tests** (`Tests/UI/Mobile`) with Appium 2: Android mobile web against saucedemo.com, with iOS and native-app support in the driver bootstrap
- **xUnit + Allure everywhere** (`Allure.Xunit`): suites, tags, steps, timing parameters, a full-page screenshot on failure, and the browser in every UI test name
- **Four top-level folders**: `Core` (shared framework code, with optional add-on projects such as `Core/Mobile` inside it), `Domain` (app-specific models, test data, API helpers), `Pages` (page objects), `Tests` (all tests: `Api/`, `UI/Web/`, `UI/Mobile/`), with one layout rule everywhere: `<Layer>/<App>/`, namespace = folder, one type per file
- **One configuration mechanism** (`Microsoft.Extensions.Configuration`) for every settings file: JSON, then environment variables, so any value (URLs, credentials, Appium server) can be overridden without editing committed files
- **Typed test data**: models in `Domain/Models/<App>/`, a Builder + Object Mother per entity in `Domain/TestData/<App>/` for every app, and `TestDataLoader` (JSON) ready for large data-driven scenario tables
- **Pluggable providers** (plug-and-play, [docs/PLUGIN_ARCHITECTURE.md](docs/PLUGIN_ARCHITECTURE.md)): each test project registers its adapters' plugins in one composition root, and `providers.json` picks the implementation per contract (logging, results publishing, reporting, HTTP transport, retries, web UI driver, secrets), with automatic fallback to console for logging and reporting
- **CI/CD on Azure DevOps** (`azure-pipelines.yml`), triggered from GitHub, in a single job. PRs run API, TodoMVC and the SauceDemo smoke test; `main` runs everything.
- **Run scripts for Windows (`scripts/windows/`) and macOS/Linux (`scripts/unix/`)**: build → test → collect Allure results, including a run-every-browser script
- **Local Elasticsearch + Kibana stack** (`docker-compose.yml`) for the Elastic logging provider

---

## 📂 Project Structure

```plaintext
TestAutomationProj/
│
├── Core/                              # Domain-agnostic framework engine (Core.csproj; used by every project)
│   ├── Api/                           # ApiClient (JSON, status, retries on IHttpTransport; auth-agnostic), ApiException
│   ├── Composition/                   # Framework.Configure (composition root), IFrameworkPlugin, ProviderRegistry, providers.json settings
│   ├── Http/                          # IHttpTransport + factory, HttpRequestData/HttpResponseData, HttpClientTransport (built in)
│   │   └── Playwright/                # Adapter project Core.Http.Playwright ("playwright" in providers.json)
│   ├── Configuration/                 # TestSettings (+ WebAppSettings, ApiSettings) + ConfigurationProvider, SettingsLoader
│   ├── Messaging/                     # MessageCollector<T> + ReceivedMessage (broker-neutral; fed by the add-ons)
│   │   ├── Kafka/                     # Add-on project Core.Messaging.Kafka: KafkaListener (manual partition assignment, no commits)
│   │   └── RabbitMq/                  # Add-on project Core.Messaging.RabbitMq: RabbitMqListener (exclusive auto-delete queue)
│   ├── Data/                          # DbConnectionFactory (any ADO.NET provider) + DbTestScope (rolls back by default)
│   ├── Security/                      # ISecretProvider, EnvironmentSecretProvider, CompositeSecretProvider (secret://NAME placeholders)
│   │   └── AzureKeyVault/             # Adapter project Core.Security.AzureKeyVault ("azure-keyvault" in providers.json Secrets)
│   ├── Resilience/                    # RetryPolicies (transient HTTP, navigation, UI action) on IRetryPolicy, built-in retries, Poll.UntilAsync
│   │   └── Polly/                     # Adapter project Core.Resilience.Polly ("polly" in providers.json)
│   ├── Logging/                       # ILoggingService, LoggingServiceFactory, ResultsPublisherFactory, LogMetadata,
│   │   │                              # LoggingConfig (+ sections) + LoggingConfigProvider (logging-config.json)
│   │   ├── Console/                   # console provider (built in)
│   │   └── Elastic/                   # Adapter project Core.Logging.Elastic: Elasticsearch provider ("elastic" in providers.json)
│   ├── TestData/                      # TestDataLoader (JSON) for data-driven tables
│   ├── Reporting/                     # ITestReporter, Report.Step (what tests call), ConsoleTestReporter (built in)
│   │   └── Allure/                    # Adapter project Core.Reporting.Allure ("allure" in providers.json)
│   ├── Ui/                            # UI contract: IUiDriverFactory/IUiDriver/IUiSession, IElement, Locate, Ui.Expect, capabilities
│   │   ├── Playwright/                # Adapter project Core.Ui.Playwright ("playwright" in providers.json; browsers chromium/chrome/msedge/firefox/webkit)
│   │   └── Selenium/                  # Adapter project Core.Ui.Selenium ("selenium"; WebDriver session reused by the Appium adapter)
│   ├── Testing/                       # TestRunner, TestRunInfo (logging, report timing, failure hook, publishing), CleanupTracker
│   └── Mobile/                        # Adapter project Core.Mobile: Appium ("appium" UI driver for mobile suites; builds on Core.Ui.Selenium)
│       ├── Configuration/             # MobileSettings (+ sections) + MobileSettingsProvider
│       ├── Driver/                    # MobileDriverFactory, MobileOptionsBuilder, MobilePlatform, MobileAppType
│       ├── Interactions/              # GestureHelper (behind the IMobileGestures capability)
│       └── Ui/                        # AppiumUiDriverFactory, AppiumUiSession, AppiumUiPlugin
│
├── Domain/                            # App-specific test code (Domain.csproj): never in Core (R34)
│   ├── Api/RestfulBooker/             # AuthClient (POST /auth → token → Cookie header)
│   ├── Models/                        # Data shapes per app: RestfulBooker, SauceDemo, Todo
│   └── TestData/                      # Builders + Object Mothers per app: RestfulBooker, SauceDemo, Todo
│
├── Pages/                             # Playwright page objects (Pages.csproj)
│   ├── Common/BasePage.cs
│   ├── SauceDemo/                     # SauceDemoPage (data-test ids) + LoginPage, InventoryPage, CartPage, Checkout* pages
│   └── Todo/TodoPage.cs
│
├── Tests/                             # All tests
│   ├── Tests.csproj                   # Playwright tests: Api/ + UI/Web/ (one project, run in CI)
│   ├── Api/                           # BookingApi{Get,Post,Update,Patch,Delete,HealthCheck}Tests
│   ├── UI/
│   │   ├── Web/                       # Playwright web UI tests, one folder per app with a <App>TestBase
│   │   │   ├── SauceDemo/             # SauceDemoTestBase + SauceDemo, AddToCart, RemoveFromCart, CheckoutFlow, NegativeLogin
│   │   │   └── Todo/                  # TodoTestBase + TodoTests
│   │   └── Mobile/                    # Appium suite (own project: Tests.UI.Mobile.csproj; shared Pages/ on the UI contract, refs Core.Mobile)
│   │       ├── Infrastructure/        # MobileTestBase
│   │       ├── SauceDemo/             # SauceDemoTests
│   │       ├── mobilesettings.json    # Appium server, platform, device, android/ios, behavior
│   │       └── package.json           # npm scripts: appium, appium-doctor, driver install
│   ├── Unit/                          # Framework unit tests (own project: Tests.Unit.csproj; no browser or external services; TLS checks use loopback)
│   ├── Contract/                      # UI driver contract tests (own project: Tests.Contract.csproj; runs per driver adapter, no Allure)
│   ├── Fixtures/                      # BrowserFixture (browser per class, from the selected UI driver), ApiFixture
│   ├── Infrastructure/                # UiTestBase, ApiTestBase
│   └── testsettings.json              # Browser, headless, timeouts, app URLs, API credentials
│
├── scripts/
│   ├── windows/                       # .bat run scripts, Appium env check/fix (.ps1)
│   └── unix/                          # .sh equivalents for macOS/Linux
├── docs/                              # ARCHITECTURE, DIAGRAMS (Mermaid flows), PLUGIN_ARCHITECTURE, MOBILE_ARCHITECTURE, GETTING_STARTED, CONTRIBUTING, REVIEW
├── azure-pipelines.yml                # Azure DevOps pipeline (the only CI)
├── ci.slnf                            # Solution filter CI builds: Core (+ its add-ons), Domain, Pages, Tests, Tests/Unit, Tests/UI/Mobile
├── .editorconfig                      # C# code style (file-scoped namespaces, usings, LF)
├── .gitattributes                     # Line endings: LF, CRLF for .bat
├── Directory.Build.props              # Shared project settings (net10.0, nullable, code style in build, warnings as errors in CI)
├── Directory.Packages.props           # Central NuGet versions for every project
├── allure-categories.json             # Allure categories (UI vs API by tag)
├── providers.json                     # Which provider implements each contract (Logging, ResultsPublisher, Reporting, Http, Resilience, Ui, MobileUi, Secrets)
├── logging-config.json                # Logging provider settings (Elasticsearch URL, TLS fingerprint)
├── docker-compose.yml                 # Local Elasticsearch 8 + Kibana (ports 9200 / 5601, no auth)
├── CHANGELOG.md
├── LICENSE                            # MIT
└── TestAutomationPlatform.slnx        # The solution; Core and its add-ons under one /Core/ solution folder
```

Dependencies: `Tests` → `Pages` → `Core` and `Tests` → `Domain` → `Core`; `Tests/UI/Mobile` → `Pages`, `Core.Mobile` → `Core.Ui.Selenium` → `Core`. `Tests.csproj` excludes the `Tests/UI/Mobile`, `Tests/Unit` and `Tests/Contract` folders, which are separate projects; in the same way `Core.csproj` excludes its add-on projects (`Core/Mobile`, `Core/Messaging/Kafka`, `Core/Messaging/RabbitMq`, `Core/Security/AzureKeyVault`), which live inside `Core/` but are built separately so only the suites that need Appium, a broker client or the Azure SDK take those packages. Every project's assembly name equals its root namespace, which matches its folder (`Core`, `Core.Messaging.Kafka`, `Core.Messaging.RabbitMq`, `Core.Mobile`, `Core.Security.AzureKeyVault`, `Domain`, `Pages`, `Tests`, `Tests.Unit`, `Tests.UI.Mobile`). The mobile suite keeps its own `Pages/` because the top-level `Pages` project is built on Playwright; its reusable bootstrapping is in `Core.Mobile`, so `Core` has no Appium dependency.

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
- For the Mobile suite: Node.js, Appium (`npm i -g appium`), the UiAutomator2 driver matching your Appium version (`appium driver install uiautomator2`; see GETTING_STARTED for older Appium versions), Android SDK and a running emulator or device. On Windows, `scripts/windows/appium_env_check.ps1` diagnoses the setup and `scripts/windows/appium_env_fix.ps1` repairs common problems.
- Optional: Elasticsearch 8 for the `elastic` logging provider. `docker compose up -d` starts a local single-node Elasticsearch on `http://localhost:9200` (security off, no password) and Kibana on `http://localhost:5601`. OpenSearch doesn't work: Elastic's 8.x client gets HTTP 406 on every write.

Step-by-step setup for **Windows, macOS and Linux**, with what each platform can and can't run, is in [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md).

---

## ⚡ Quick Start

```bash
# Restore & build everything
dotnet build TestAutomationPlatform.slnx

# Framework unit tests (no browser or external services; TLS checks use loopback)
dotnet test Tests/Unit/Tests.Unit.csproj

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

All four settings files load the same way (`Core.Configuration.SettingsLoader`, built on `Microsoft.Extensions.Configuration`), once per run, with later sources winning:

1. the JSON file (comments allowed),
2. environment variables with the file's prefix, using `__` between nested keys,
3. a few short aliases for the most common settings.

| File | Loaded by | Prefix | Example | Aliases |
|---|---|---|---|---|
| `Tests/testsettings.json` | `ConfigurationProvider.Current` | `TESTS_` | `TESTS_RestfulBooker__Password=…` | `BROWSER`, `HEADLESS` |
| `providers.json` | `ProvidersConfigProvider.Current` | `PROVIDERS_` | `PROVIDERS_Logging=console`, `PROVIDERS_Secrets__1=azure-keyvault` | – |
| `logging-config.json` | `LoggingConfigProvider.Current` | `LOGGING_` | `LOGGING_Elastic__Url=…` | `ELASTIC_URL`, `ELASTIC_USERNAME`, `ELASTIC_PASSWORD`, `ELASTIC_CERT_FINGERPRINT` |
| `Tests/UI/Mobile/mobilesettings.json` | `MobileSettingsProvider.Current` | `MOBILE_` | `MOBILE_Device__Udid=…` | `APPIUM_URL`, `DEVICE_NAME` |

Keys are case-insensitive. Secrets (passwords, tokens) should come only from the environment or a CI secret variable.

**Secret placeholders.** Any setting can hold `secret://NAME` instead of a value, e.g. `"Password": "secret://booker-password"` or `TESTS_RestfulBooker__Password=secret://booker-password`. `SettingsLoader` resolves it through `SettingsLoader.SecretProvider` (`Core.Security.ISecretProvider`). A missing secret fails the run with the setting and secret named.

- **Default:** environment variables: `booker-password`, then `BOOKER_PASSWORD`.
- **Azure Key Vault:** add `"azure-keyvault"` to `Secrets` in `providers.json` (e.g. `"Secrets": ["environment", "azure-keyvault"]`, or `PROVIDERS_Secrets__1=azure-keyvault`) and set `KEYVAULT_URI=https://<vault>.vault.azure.net/`. Providers are tried in the listed order, so environment variables still win (`Core.Security.AzureKeyVault` in `Core/Security/AzureKeyVault/`, signed in with `DefaultAzureCredential`: `az login` locally, a service connection or managed identity in CI). The identity needs *Get* permission on secrets (the "Key Vault Secrets User" role). Key Vault secret names allow letters, digits and dashes, so use names like `booker-password`.
- **Another vault:** implement `ISecretProvider` in an adapter project with an `IFrameworkPlugin` that registers it by name, pass the plugin to `Framework.Configure` in the test project's `TestRunSetup`, and list the name in `Secrets` ([provider guide](docs/PLUGIN_ARCHITECTURE.md#5-writing-a-provider)). Selecting a provider that isn't registered, or Key Vault without `KEYVAULT_URI`, stops the run at startup with the reason.

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
| `Trace` | `off`, `on-failure` (default: attach a Playwright trace to Allure when a test fails), `on` (every test) | `TRACE` |
| `Resilience.Http` / `.Navigation` | `MaxRetries` (default 2), `BaseDelayMs` (500/500): retries for API 502/503/504 and network errors (idempotent methods only), navigation `net::ERR_*`, and mobile UI glitches | `TESTS_Resilience__Http__MaxRetries`, … |
| `<App>.BaseUrl`, `RestfulBooker.Username/Password` | | `TESTS_SauceDemo__BaseUrl`, `TESTS_RestfulBooker__Password`, … |

Base URLs end with `/`, and entry pages open `./` (the context's `BaseURL`), so an app can live under a sub-path such as `/todomvc/`.

### Mobile: `Tests/UI/Mobile/mobilesettings.json`

Same pattern: `MobileSettingsProvider.Current` loads `mobilesettings.json` from the output directory once, then `MOBILE_*` variables and the `APPIUM_URL` / `DEVICE_NAME` aliases. It holds `Platform` (`Android`/`iOS`), `AppType` (`Web`/`Native`), `BaseUrl`, and `Server`, `Device`, `Android`, `IOS` and `Behavior` sections. See [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md) for every field.

### Test data

Every app uses the same pieces:

| What | Where | Use it for | Example |
|---|---|---|---|
| **Model** | `Domain/Models/<App>/` | The shape (API contract or test-data record) | `Booking`, `User`, `TodoItem` |
| **Builder** | `Domain/TestData/<App>/<Entity>Builder.cs` | A valid object by default; `With…` to vary one field | `new BookingBuilder().WithDepositPaid(false).Build()` |
| **Object Mother** | `Domain/TestData/<App>/<Entity>Mother.cs` | Named, ready-made objects built on the builder | `BookingMother.Standard()`, `UserMother.LockedOut()`, `ProductMother.Backpack()`, `TodoItemMother.Completed()` |
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

Then put it at `Tests/test-data/<App>/<TestClass>/<Scenario>.json` with a matching record in `Domain/Models/<App>/`, and load it with `TestDataLoader.Load<T>("<App>/<TestClass>/<Scenario>.json")`. Secrets never go in either. They belong in settings or environment variables.

---

## 🧾 Logging (Pluggable Providers)

Which provider is used is chosen in **`providers.json`** at the repo root (`"Logging": "elastic"` or `"console"`; `ResultsPublisher` follows it unless set; `"Reporting": "allure"` or `"console"`; `"Http": "playwright"` or `"httpclient"` for API tests; `"Resilience": "polly"` or `"builtin"` for retries; `"Ui": "playwright"` or `"selenium"` for web UI; `"MobileUi": "appium"`), overridable with `PROVIDERS_Logging=console`. Each test project registers the providers it can use in its `TestRunSetup` (`Framework.Configure(new ElasticLoggingPlugin(), …)`); console is built in. At startup the run prints the selection, e.g. `[Providers] Logging=elastic, ResultsPublisher=elastic, Secrets=environment`.

The Playwright tests (`Tests/`) and the Mobile suite read the provider settings from `logging-config.json`. `LoggingConfigProvider.Current` loads it once, finding the file by walking up from the test run directory until it reaches a folder containing `logging-config.json`, and then applies environment overrides.

```json
{
  "Elastic": {
    "Url": "http://localhost:9200",
    "Username": "",
    "Password": "",
    "CertificateFingerprint": ""
  }
}
```

| Setting | Environment override | Notes |
|---|---|---|
| `Elastic.Url` | `ELASTIC_URL` | |
| `Elastic.Username` / `Elastic.Password` | `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` | **Only set these in the environment** (or a CI secret variable); keep them empty in the file. The local `docker compose` stack needs none. |
| `Elastic.CertificateFingerprint` | `ELASTIC_CERT_FINGERPRINT` | CA fingerprint for `ON_LOCALHOST_SECURE`; when empty, a self-signed certificate is accepted only for loopback URLs in that profile |
| connection profile | `ELASTIC_SERVER` | `ON_CLOUD`, `ON_LOCALHOST_SECURE`, `ON_LOCALHOST_INSECURE` (default) |

Provider selection, implemented by `LoggingServiceFactory` and `ResultsPublisherFactory`:

1. Use the provider named in `providers.json` if it is registered and can be created.
2. If it isn't registered or can't be used (e.g. Elasticsearch unreachable), print one message and use `console`.
3. As a last resort, use a built-in console logger.

The `elastic` provider checks the server first, **once per URL per test run** (cached): `GET /` with the configured credentials must return 2xx from **Elasticsearch 8 or later** (not OpenSearch). If it doesn't (unreachable, wrong credentials, OpenSearch, too old), the run uses the console and prints **one** message with the reason, e.g. `[Logging] Provider 'elastic' not used: authentication failed (HTTP 401) …`. If the check passes but writes are rejected later (e.g. a user without write rights), the Elastic logger prints one message and continues on the console, and results publishing stops after its first failure. Loggers (one per index) and the results publisher are created once and reused. A user for an external cluster needs the `monitor` cluster privilege (for `GET /`) and write access to the `*-logs-*` and `search-*` indices.

```csharp
var logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
logger.Info("message");
```

Index names follow `<project>-logs-{date}`: `api-logs-…` (API tests), `ui-web-logs-…` (web UI tests) and `ui-mobile-logs-…` (Mobile). Results go to `search-<project>` (`search-api`, `search-ui-web`, `search-ui-mobile`). Until 2026-11-14 the API index was `test-logs-…`, so update any Kibana data view or dashboard that used it.

> Never commit Elasticsearch credentials. `logging-config.json` keeps them empty; use `ELASTIC_USERNAME` / `ELASTIC_PASSWORD`.

---

## 📣 Results Publishing

`UiTestBase`, `ApiTestBase` and `MobileTestBase` publish one `LogMetadata` record per test through `ResultsPublisherFactory.Create()`, from `Core`'s `TestRunner` (see Test Infrastructure below). The record contains project (`ui-web`, `api` or `ui-mobile`), class, method, status, duration, failure reason, start/end time, run name, user, and browser. A publish failure is logged and never fails the test.

- `Provider=elastic`: results are indexed into Elasticsearch
- `Provider=console`: results are printed to the console

---

## 📊 Allure Reporting

- Every suite uses **`Allure.Xunit`**, which turns on automatically when the tests run, and writes to `bin/<Configuration>/net10.0/allure-results/`. Each test run starts by clearing that folder (the Allure reporter's `BeginRun`, called by `Framework.Configure` in each test project's module initializer), so `allure serve` only shows the latest run. To keep several runs (e.g. one per browser), copy the folder out after each one, as the scripts and the nightly pipeline do. The run scripts collect the results (plus `categories.json`) into `./allure-results`, and CI publishes them as the `allure-results` artifact.
- **Reporting goes through a contract (R39).** Framework code and tests call `Core.Reporting` (`ITestReporter`; tests use `Report.Step("…", …)`), and the reporter is chosen with `"Reporting"` in `providers.json`: `allure` (the `Core.Reporting.Allure` adapter) or `console` (built in: prints steps, keeps no attachments). With `PROVIDERS_Reporting=console` no framework code touches Allure; `Allure.Xunit`, the xUnit integration, still writes a basic outcome file per test.
- Class labels still use Allure.Xunit's attributes from `Allure.Net.Commons.Attributes` (read when tests are discovered). Every test class has the same labels: `[AllureParentSuite("API" | "UI Web" | "UI Mobile")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`. Steps use `Report.Step(...)`.
- `TestRunner` adds **Start Time**, **End Time**, and **Duration (s)** parameters to every test.
- `UiTestBase` appends the browser to the test name, e.g. `Checkout_Should_CompleteOrder (chrome)`, records it as a `browser` parameter (so per-browser runs stay separate when merged), and attaches a full-page screenshot and a **Playwright trace** when a test fails (Allure opens the trace in its built-in trace viewer: a step-by-step replay with DOM snapshots, network and console). `MobileTestBase` appends the platform and app type, e.g. `(Android Web)`.
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

| Event | Tests run |
|-------|-----------|
| PR into `main` | Unit tests; API + TodoMVC (all), SauceDemo `smoke` on Chrome (`--filter "FullyQualifiedName!~.SauceDemo.\|Category=smoke"`) |
| Push to `main` or `release/*` | Unit tests; everything on Chrome, plus code coverage |
| Nightly, 02:00 UTC (`main`, only if it changed) | As a push to `main`, plus web UI on **Chromium, Firefox and WebKit**, and Android mobile smoke on macOS, plus Windows helper verification |
| Manual run | Linux unit/web/API tests, Android mobile smoke on macOS, and Windows helper verification |

### Jobs (`azure-pipelines.yml`)

The **Linux job** (`Tests`, `ubuntu-latest`) runs:

1. Shallow checkout, .NET 10 SDK
2. Build `ci.slnf` (Release): `Core`, `Pages`, `Tests`, `Tests/Unit` and `Tests/UI/Mobile` (compile check only), with warnings as errors.
3. `dotnet test Tests/Unit/Tests.Unit.csproj`: the framework unit tests, first, so a broken `Core` fails fast.
4. `dotnet test Tests/Tests.csproj` with `BROWSER=chrome`, so Playwright drives the Google Chrome already on the hosted image and nothing is downloaded.
5. Nightly only: install Playwright's browsers and run the web UI tests once per browser (Chromium, Firefox, WebKit).
6. Publish TRX test results (failures fail the build) and coverage (`main`/`release/*` only).
7. Allure: publish the raw `allure-results`, then build the **`allure-report`** artifact with the Allure CLI (version pinned in `allureVersion`). The report includes trends: the `history/` of the latest `main` build's report is fed into each new one (PRs and branches compare against `main`). Download the artifact and open **`allure-report.html`** (a single self-contained file), or serve the folder.

Web/API and framework checks share the Linux job to avoid repeating setup. The mobile job needs macOS for Android emulator acceleration. With one parallel slot, the jobs queue rather than run simultaneously.

The **mobile job** (`MobileSmoke`, `macOS-15`) runs on scheduled and manual builds. It installs Node.js 22.23.2, Appium 2.19.0 and UiAutomator2 4.2.9, creates an Android 14 Google APIs emulator with Chrome, waits for boot/Appium readiness, and runs `Tests/UI/Mobile/Tests.UI.Mobile.csproj --filter "Category=smoke"`. Empty discovery, skipped tests, setup errors, and test failures fail the job. Appium's matching Chromedriver download is enabled only on its loopback-bound server.

The `mobile-results` artifact contains TRX, raw Allure results, tool versions, and emulator/Appium/logcat diagnostics. Cleanup runs when the helper exits, including on failure. This artifact is separate from the Linux Allure HTML report. Pipeline configuration is implemented; R16 remains pending until an actual hosted mobile run passes and publishes its results.

A third job, **WindowsHelperVerification** (`windows-2025`), runs on scheduled/manual builds. It uses a temporary copy of the Windows helper and a controlled executable that returns build exit code 37. It verifies that old binaries cannot lead to a successful test run after build failure. JSON evidence and the helper log are published as `windows-helper-verification`. No developer Windows machine is needed; R25 awaits the first passing hosted Windows run.

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
public Task Cart_Should_ContainProduct_When_ProductAdded() => RunAsync(async () =>
{
    // Session, Settings, Logger, Report.Step(...), Ui.Expect(...)
});
```

### `Tests/Infrastructure/UiTestBase.cs` (web UI)

- Uses `BrowserFixture` (`IClassFixture`): **one browser per test class**, launched by the UI driver selected with `"Ui"` in `providers.json` (`playwright`) for the configured `Browser`
- Gives each test a **new isolated `Session`** (`IUiSession`: own cookies and storage, one page) with the app's `BaseUrl` (SauceDemo, TodoMVC) and the default timeout applied, and closes it after the test
- `RunAsync` adds the browser to the report's test name and parameters, attaches a full-page screenshot and a trace on failure (the driver's `IScreenshots` / `ITracing` capabilities), and turns a `UiException` (failed `Ui.Expect`, element never actionable) into an xUnit assertion failure
- Each app has its own base: `SauceDemoTestBase` (sets `BaseUrl`, adds `LoginAsync(username, password)`) and `TodoTestBase` (sets `BaseUrl`, adds `OpenWithItemAsync(item)`)

### `Tests/Infrastructure/ApiTestBase.cs` (API)

- Uses `ApiFixture` (the settings); no browser
- Gives each test its own `ApiClient` (on the HTTP transport selected with `"Http"` in `providers.json`: `playwright`, the default, shares one Playwright instance per run; `httpclient` needs no Playwright), with `BaseURL` and `Accept: application/json`, disposed afterwards
- `GetAuthHeadersAsync()` gets a Restful Booker token and returns it as request headers (the `token` cookie), which tests pass to PUT, PATCH and DELETE. `ApiClient` itself knows nothing about any API's auth scheme: it just sends the headers it's given.
- `CreateBookingAsync(booking)` POSTs a booking and registers its deletion on `Cleanup` (a `Core.Testing.CleanupTracker`); after the test, `DisposeAsync` runs every registered cleanup, last first (a failed cleanup is logged, never fails the test), so the shared demo API isn't left with test data. Tests can register their own cleanups with `Cleanup.Add(description, action)`
- `ApiClient` throws `ApiException` (with `StatusCode`) on non-2xx responses; `SendRawAsync` returns the raw response when a test wants to assert on the status itself (e.g. `404` after delete)

### `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs` (Appium)

- `Run(body)` reads `MobileSettingsProvider.Current`; `MobileOptionsBuilder` builds the `AppiumOptions` and `MobileDriverFactory` the Android or iOS driver
- For mobile web, navigates to `BaseUrl`. For native apps, launches the configured app or package.
- Records timing, logging, result publishing, and a failure screenshot through `TestRunner`
- Tests use the shared page objects in `Pages/` and `Ui.Expect` on the session from the `appium` UI driver (R47). Waits are built into the UI contract; swipes and scrolls use the `IMobileGestures` capability (`Session.TryGet<IMobileGestures>(...)`).

---

## ➕ Adding Tests

### Web UI

1. Add a page object in `Pages/<App>/` (inherit `Pages.Common.BasePage`; it takes the test's `IUiSession`), exposing `IElement`s for assertions and async intent methods for actions. Locate elements with `TestId("…")` or `Session.Find(Locate.Role(...) / Locate.Placeholder(...))`, and filter with `WithChild(...)`, not CSS classes. Methods that navigate return the next page object (`var cart = await inventory.OpenCartAsync();`). Entry pages open with `BasePage.GotoAsync("./")`, which retries a navigation that fails with a network error (`net::ERR_*`) up to twice.
2. Add a model in `Domain/Models/<App>/` and a Builder + Mother in `Domain/TestData/<App>/`. Put `[Theory]` cases in a `[MemberData]` source built from them. Use JSON (`Tests/test-data/<App>/<TestClass>/<Scenario>.json` via `TestDataLoader`) only for large tables or data owned outside the code (see below) cases.
3. For a new app, add its URL to `testsettings.json` / `TestSettings` and a `Tests/UI/Web/<App>/<App>TestBase.cs` that inherits `UiTestBase` and overrides `BaseUrl`.
4. Add a test class in `Tests/UI/Web/<App>/` that inherits the app's test base (takes `BrowserFixture`) and carries `[AllureParentSuite("UI Web")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("ui", "<app>", "<feature>", ...)]` and `[Trait("Category", "regression")]` (add `smoke` if it belongs in PR runs). Write each test as `[Fact] public Task X() => RunAsync(async () => { ... });` and assert with `Ui.Expect(...)` (from `Core.Ui`).

### API

1. Add models in `Domain/Models/<Service>/`, a Builder + Mother per entity in `Domain/TestData/<Service>/`, and any service-specific helpers (auth, …) in `Domain/Api/<Service>/`.
2. Add a test class in `Tests/Api/` that inherits `ApiTestBase` (takes `ApiFixture`), with `[AllureParentSuite("API")]`, `[AllureSuite("<Service>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("api", "<service>", ...)]` and `[Trait("Category", ...)]`. Use `Client.GetAsync/PostAsync/PutAsync/PatchAsync/DeleteAsync` and `GetAuthHeadersAsync()` (for another API, put its auth helper in `Domain/Api/<Service>/` and return headers the same way).

### Database checks (when a system under test has a database)

1. Reference the ADO.NET provider package in the test project (e.g. `Npgsql`, `Microsoft.Data.SqlClient`) and put the connection string in settings, with the password as `secret://NAME`.
2. Create `new DbConnectionFactory(NpgsqlFactory.Instance, connectionString)` and, per test, `await using var db = await factory.BeginScopeAsync();`. Seed or check rows with `db.ExecuteAsync` / `db.ScalarAsync<T>` (or Dapper on `db.Connection`). The scope **rolls back** when the test ends, unless you call `CommitAsync()`. For anything committed or created outside the transaction, register a cleanup with `Cleanup.Add(...)`.

### Message checks (when a system under test publishes to a broker)

Reference the add-on for your broker from the test project and start a listener **before** triggering the behaviour:

```csharp
await using var events = await RabbitMqListener.StartAsync(rabbitUri, exchange: "bookings", routingKey: "booking.*");
// or: await using var events = await KafkaListener.StartAsync("broker:9092", topic: "bookings");
await Client.PostAsync<CreatedBooking>("booking", booking);
var created = await events.WaitForAsync(m => m.Key == "booking.created", TimeSpan.FromSeconds(10));
Assert.Equal(booking.LastName, created.BodyAs<Booking>()!.LastName);
```

The RabbitMQ listener binds an exclusive, auto-delete queue; the Kafka listener reads from the end offsets with no consumer-group commits. Either way, nothing is left on the broker after the test. The broker tests in `Tests/Unit` run only when `RABBITMQ_URI` / `KAFKA_BOOTSTRAP` are set (see GETTING_STARTED).

### Mobile

1. Reuse the page object in `Pages/<App>/`, or add one there (inherit `BasePage`; it works on mobile and web).
2. Add a test class in `Tests/UI/Mobile/<App>/` that inherits `MobileTestBase`, with `[AllureParentSuite("UI Mobile")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`, `[AllureTag("ui", "mobile", "<app>", ...)]` and `[Trait("Category", ...)]`. Write each test as `[Fact] public Task X() => RunAsync(async () => { ... });`.

See also [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) for code-style rules.

---

## ⚠️ Known Issues / TODO

- **Mobile tests are only compiled in CI.** Hosted agents have no Android emulator. Running them would need a self-hosted agent or a device cloud (point `APPIUM_URL` at it).
- **Playwright Firefox may not start on some Macs** ("Could not find profile folder"). Reinstall it with `playwright.ps1 install --force firefox` or use another browser; CI uses Chrome.
- **`LICENSE`** still has the `[Year] [Your Name]` placeholders.

---

## 📖 Architecture

See [docs/DIAGRAMS.md](docs/DIAGRAMS.md) for flow diagrams (project dependencies, a web/API/mobile test run step by step, settings and secrets, the CI pipeline), and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the text reference and the design decisions behind the UI stacks. The mobile layer has its own design doc: [docs/MOBILE_ARCHITECTURE.md](docs/MOBILE_ARCHITECTURE.md). The plan to make every external library replaceable (contracts, adapters, dependency inventory, how to write a provider) is in [docs/PLUGIN_ARCHITECTURE.md](docs/PLUGIN_ARCHITECTURE.md). Release notes are in [CHANGELOG.md](CHANGELOG.md). Open review findings (not yet fixed) are tracked in [docs/REVIEW.md](docs/REVIEW.md).

---

## 🤝 Contributing

Contributions and suggestions are welcome. Open an issue or a PR. Before submitting, run the relevant suite locally (`dotnet test …` or the scripts in `scripts/`) and follow `.editorconfig` and [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md).

---

## 📄 License

MIT. See [LICENSE](LICENSE).
