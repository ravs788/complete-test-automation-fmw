# Modern C# Test Automation Framework: Architecture

## Overview

This framework is a modular C# / .NET 10 test automation platform for REST API, web UI and Android mobile-web testing. **Playwright** drives both the web UI and the API tests; **Appium** drives the mobile tests. Every suite runs on **xUnit v2 with Allure (`Allure.Xunit`)**. Source lives on GitHub, and CI/CD runs on Azure DevOps Pipelines. Results go to Allure, TRX, and an optional Elasticsearch sink.

**Diagrams:** rendered flow diagrams of the same material (project dependencies, each kind of test run, settings and secrets, CI) are in [DIAGRAMS.md](DIAGRAMS.md).

**Replaceable libraries:** the target design (contracts in `Core`, one adapter project per library, a composition root), the dependency inventory and the provider guide are in [PLUGIN_ARCHITECTURE.md](PLUGIN_ARCHITECTURE.md).

| Area | Location | Driver/Client | In CI |
|------|----------|---------------|-------|
| API | `Tests/Api` | `ApiClient` on `IHttpTransport`: Playwright `APIRequestContext` (default) or `HttpClient` | Yes |
| Web UI | `Tests/UI/Web` (SauceDemo, TodoMVC) | UI contract (`Core/Ui`) on the Playwright adapter (Chromium, Chrome, Edge, Firefox, WebKit) | Yes (Chrome) |
| Mobile web | `Tests/UI/Mobile` | Appium 2 (Android; iOS/native in the bootstrap) | Scheduled/manual Android smoke on hosted macOS; first CI run pending |

Why xUnit **v2**: `Allure.Xunit` doesn't support xUnit v3 yet. `xunit.runner.visualstudio` is pinned to 2.8.2 because adapters from 3.0 on are built on the v3 runner and can't load Allure's reporter.

---

## 1. Goals

- **Transportable:** moves to new applications and environments with minimal setup.
- **Maintainable:** clear layering (Core → Pages → Tests) and small, single-purpose classes.
- **One web/API tool by default:** Playwright for browsers and (through the `playwright` HTTP transport) API calls, so UI and API tests share configuration and reporting; API tests can switch to the built-in `HttpClient` transport by configuration (R41).
- **Observable:** Allure reports, TRX for CI dashboards, optional Elasticsearch for logs and trends.
- **CI/CD ready:** Azure DevOps Pipelines, triggered from GitHub.

---

## 2. Layered View

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ Tests/                                                                   │
│   Tests.csproj (Playwright)                   UI/Mobile (Tests.UI.Mobile)│
│     Api/        → ApiTestBase + ApiFixture      Infrastructure/          │
│     UI/Web/<App>/ → <App>TestBase : UiTestBase    MobileTestBase         │
│                   + BrowserFixture              (Core.Mobile: config,    │
│                                                  driver, interactions)   │
│     testsettings.json                           Pages/<App>/ <App>/      │
│                                                 mobilesettings.json      │
├──────────────────────────────────────────────────────────────────────────┤
│ Pages/  (Pages.csproj, Playwright)                                       │
│   Common/BasePage · SauceDemo/LoginPage, InventoryPage, CartPage,        │
│   Checkout* · Todo/TodoPage                                              │
├──────────────────────────────────────────────────────────────────────────┤
│ Domain/  (Domain.csproj, app-specific)                                   │
│   Api/RestfulBooker · Models/<App> · TestData/<App> (Builders/Mothers)   │
├──────────────────────────────────────────────────────────────────────────┤
│ Core/   (Core.csproj, shared by everything)                              │
│   Api/            ApiClient (JSON, status, retries), ApiException        │
│   Http/           IHttpTransport · HttpClient (built in) · Playwright/   │
│   Composition/    Framework.Configure · IFrameworkPlugin · registry      │
│   Configuration/  TestSettings (+ sections) + ConfigurationProvider      │
│   Ui/             UI contract · Ui.Expect · capabilities · Playwright/   │
│   Logging/        factories, LogMetadata, LoggingConfig + Provider       │
│                   Console/ (built in) · Elastic/ (adapter project)       │
│   TestData/       TestDataLoader (JSON)                                  │
│   Resilience/     RetryPolicies on IRetryPolicy · Poll · Polly/ adapter  │
│   Security/       ISecretProvider (secret://NAME) · env-var provider     │
│                   AzureKeyVault/ (add-on project)                        │
│   Data/           DbConnectionFactory · DbTestScope (rollback by default)│
│   Messaging/      MessageCollector<T> · Kafka/, RabbitMq/ (add-ons)      │
│   Reporting/      ITestReporter · Report.Step · console (Allure/ adapter)│
│   Testing/        TestRunner + TestRunInfo · CleanupTracker              │
│   Mobile/         Appium bootstrapping (add-on project Core.Mobile)      │
└──────────────────────────────────────────────────────────────────────────┘
```

Dependency rules:

- `Tests` → `Pages` → `Core` and `Tests` → `Domain` → `Core`. **Core is domain-agnostic**: no app models, Builders/Mothers or app auth (those live in `Domain`), no page objects, no tests. `Tests/Unit/Architecture/CoreBoundaryTests` enforces it. `Tests/UI/Mobile` → `Core.Mobile` → `Core` (Appium bootstrapping is kept out of Core, R38). `Core.Http.Playwright`, `Core.Logging.Elastic`, `Core.Reporting.Allure`, `Core.Resilience.Polly`, `Core.Security.AzureKeyVault`, `Core.Ui.Playwright`, `Core.Messaging.RabbitMq` and `Core.Messaging.Kafka` → `Core` are optional adapters (R36, R37, R39–R42, R45). `Pages` uses only the UI contract (no Playwright reference, checked by `AdapterBoundaryTests`). Nothing depends on `Tests`.
- `Core` has no dependency on a test framework. It references only Microsoft.Extensions.Configuration (an accepted platform dependency, kept out of its public API); Elasticsearch (R40), Allure (R39), the Playwright HTTP transport (R41), Polly (R45) and the Playwright UI driver (R42) live in adapter projects (`Core.Logging.Elastic`, `Core.Reporting.Allure`, `Core.Http.Playwright`, `Core.Resilience.Polly`, `Core.Ui.Playwright`), and `AdapterBoundaryTests` keeps them out of `Core`.
- `Tests/UI/Mobile`, `Tests/Unit` (framework unit tests: builders, settings loading, `TestRunner`, …) and `Tests/Contract` (UI driver contract tests, need a browser) are separate projects inside the `Tests/` folder; `Tests.csproj` excludes them (`DefaultItemExcludes`). `Core/` works the same way: its add-ons (`Core/Mobile`, `Core/Http/Playwright`, `Core/Logging/Elastic`, `Core/Reporting/Allure`, `Core/Resilience/Polly`, `Core/Ui/Playwright`, `Core/Messaging/Kafka`, `Core/Messaging/RabbitMq`, `Core/Security/AzureKeyVault`) sit next to the area they extend but are separate projects that `Core.csproj` excludes, so Appium, the broker clients and the Azure SDK reach only the suites that reference them. In the solution they are grouped under one `/Core/` solution folder.
- Selenium remains a package only because Appium's .NET client is built on it.
- The mobile suite still has its own `Pages/` (Appium/Selenium types); moving it onto the shared UI contract is R47.

Conventions (the same in every project):

- **`<Layer>/<App>/`**: app-specific code sits in an app folder under its layer (`Pages/SauceDemo`, `Domain/Models/RestfulBooker`, `Domain/TestData/Todo`, `Domain/Api/RestfulBooker`, `Tests/UI/Web/Todo`, `Tests/UI/Mobile/SauceDemo`). Generic code sits at the layer root (`Core/Api/ApiClient`).
- **Namespace = folder path**, assembly name = root namespace (`Core`, `Core.Http.Playwright`, `Core.Logging.Elastic`, `Core.Reporting.Allure`, `Core.Resilience.Polly`, `Core.Ui.Playwright`, `Core.Messaging.Kafka`, `Core.Messaging.RabbitMq`, `Core.Mobile`, `Core.Security.AzureKeyVault`, `Domain`, `Pages`, `Tests`, `Tests.Unit`, `Tests.UI.Mobile`). File-scoped namespaces, one type per file.
- **Settings = record + cached provider**, all loaded by `SettingsLoader` (JSON → prefixed env vars → aliases): `TestSettings`/`ConfigurationProvider` (`TESTS_`), `LoggingConfig`/`LoggingConfigProvider` (`LOGGING_`), `MobileSettings`/`MobileSettingsProvider` (`MOBILE_`).
- **One base class per app** on top of the suite base (`SauceDemoTestBase`, `TodoTestBase`), and the same Allure labels on every class: `ParentSuite` = layer, `Suite` = app, `SubSuite` = feature.

---

## 3. Core

### 3.1 Configuration

- **One mechanism:** `SettingsLoader.Load<T>(dir, file, prefix, aliases)` (Microsoft.Extensions.Configuration) binds a record from the JSON file, then environment variables with the prefix (`__` = nesting), then aliases. Values of the form `secret://NAME` are then resolved by `SettingsLoader.SecretProvider` (`Core.Security.ISecretProvider`; chosen by name in `providers.json` `Secrets`, tried in order: `environment` by default, plus `azure-keyvault` from the `Core.Security.AzureKeyVault` adapter in `Core/Security/AzureKeyVault/`).
- **Playwright tests:** `ConfigurationProvider.Current`: `Tests/testsettings.json` from the output directory, `TESTS_*`, aliases `BROWSER` / `HEADLESS`. `TestSettings` holds `Browser`, `Headless`, `DefaultTimeoutMs` and one section per app (`TodoMvc`, `SauceDemo`, `RestfulBooker` incl. credentials).
- **Providers (R40):** `ProvidersConfigProvider.Current`: `providers.json` (found by walking up from the run directory), `PROVIDERS_*`. It names the provider for `Logging`, `ResultsPublisher` (default: same as `Logging`) and `Secrets` (ordered list). Each test project's `TestRunSetup` module initializer is the composition root: `Framework.Configure(new ElasticLoggingPlugin(), new AzureKeyVaultPlugin())` registers the adapters it references (plus the built-in console and environment providers) in a `ProviderRegistry`, wires the selected secret providers into `SettingsLoader`, and prints the selection once. Selecting a secret provider that isn't registered fails at startup with the registered names. See [PLUGIN_ARCHITECTURE.md](PLUGIN_ARCHITECTURE.md).
- **Logging:** `LoggingConfigProvider.Current`: `logging-config.json` (found by walking up from the run directory), `LOGGING_*`, aliases `ELASTIC_URL` / `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` / `ELASTIC_CERT_FINGERPRINT`. Credentials only come from the environment.
- **Mobile:** `Core.Mobile.Configuration.MobileSettingsProvider.Current`: `Tests/UI/Mobile/mobilesettings.json`, `MOBILE_*`, aliases `APPIUM_URL` / `DEVICE_NAME`.

### 3.2 Browsers

Web UI code works on the UI contract in `Core/Ui` (R42): an `IUiDriverFactory` (selected with `"Ui"` in `providers.json`) launches an `IUiDriver` per test class; each test gets an `IUiSession` (isolated cookies and storage, one page) with lazy, auto-waiting `IElement`s found by `Locate` (test id, role, placeholder, CSS), web-first assertions (`Ui.Expect(element).ToHaveTextAsync(...)`) and optional capabilities (`IScreenshots`, `ITracing`) checked with `TryGet<T>`. Driver failures surface as `UiException` / `UiAssertionException`. The `playwright` adapter (`Core.Ui.Playwright`) maps `chromium` (bundled), `chrome` / `msedge` (installed browsers via Playwright channels, no download), `firefox` and `webkit`, and implements elements on Playwright locators and assertions on its `Expect`. `Tests/Contract` runs the same contract tests against every UI driver adapter.

### 3.3 API client

`ApiClient` sends through an `IHttpTransport` (R41) created per client by the `IHttpTransportFactory` selected with `"Http"` in `providers.json`: `playwright` (`Core.Http.Playwright`, a Playwright `APIRequestContext`; one Playwright instance shared by the run) or `httpclient` (built in, .NET `HttpClient`). Transports only send: they return every HTTP response as `HttpResponseData` and throw `HttpRequestException` only when no response arrived, so `ApiClient` owns JSON, status handling and retries for all of them (`BaseURL`, `Accept: application/json`). `GetAsync/PostAsync/PutAsync/PatchAsync` (de)serialize JSON with System.Text.Json (one set of options for requests and responses; `null` properties are left out) and throw `ApiException(StatusCode)` on non-2xx; `DeleteAsync` returns the status; `SendRawAsync` returns the raw `HttpResponseData` (status, status text, headers, body) for status assertions. Every method takes optional per-request `headers`; `ApiClient` has no knowledge of any API's auth scheme. Restful Booker's `AuthClient.GetAuthHeadersAsync` turns a token from `POST /auth` into the `token` cookie header.

### 3.4 Test data

- **Models** (all data shapes) are in `Domain/Models/<App>/`: PascalCase properties mapped with `[JsonPropertyName]`, `sealed record`s.
- **Builders + Object Mothers, for every app** in `Domain/TestData/<App>/`: `<Entity>Builder` returns a valid object by default (with generated unique values where collisions matter) and `With…` methods for variations; `<Entity>Mother` names the common cases (`BookingMother.Standard()`, `UserMother.LockedOut()`, `ProductMother.Backpack()`, `TodoItemMother.Completed()`). Tests get their entities from these.
- **`[Theory]` cases** come from a `[MemberData]` source built with the Builders/Mothers (e.g. `NegativeLoginTests.InvalidLogins()`), as plain values so xUnit v2 reports each case separately.
- **JSON is the exception**, used for large scenario tables, data owned outside the code, captured payloads or per-environment data. `TestDataLoader.Load<T>("<App>/<TestClass>/<Scenario>.json")` reads from `<output>/test-data/` (source: `Tests/test-data/`). There is none at the moment.

### 3.5 Logging & results publishing

```text
     providers.json (Logging / ResultsPublisher)  +  logging-config.json (Elastic settings)
                       │
        ┌──────────────┴───────────────┐
        ▼                              ▼
LoggingServiceFactory          ResultsPublisherFactory
  │ Framework.Providers (registered by TestRunSetup): ILoggingProviderFactory / IResultsPublisherFactory
  │ 1. selected provider  → 2. "console"  → 3. console (direct)
  ▼                              ▼
ILoggingService               IResultsPublisher
 ├─ ConsoleLoggingService      ├─ ConsoleResultsPublisher
 └─ ElasticLoggingService      └─ ElasticResultsPublisher
        │ ElasticClientFactory(ELASTIC_SERVER profile)
        │ ElasticConnectivity probe (cached per connection configuration for the run) → console fallback if unreachable
```

- Loggers are cached per index format and the publisher once per run; the reachability probe is cached per URL, credentials, TLS profile/fingerprint, and timeout.
- To add a provider, implement both factory interfaces in an adapter project, register them in its `IFrameworkPlugin`, pass the plugin to `Framework.Configure` in the test projects' `TestRunSetup`, and select it by name in `providers.json` ([provider guide](PLUGIN_ARCHITECTURE.md#5-writing-a-provider)).
- Each test publishes one `LogMetadata` document (project, class, method, status, duration, reason, timing, browser, user) from `TestRunner`. A publish failure never fails the test.

---

## 4. Test Lifecycle

xUnit v2 creates a new test-class instance per test and has no teardown hook that knows the test's outcome, so each base class wraps the test body in `Core`'s `TestRunner`.

### 4.1 Web UI (`Tests/UI/Web`)

```text
Class start   BrowserFixture.InitializeAsync → Framework.UiDrivers.LaunchAsync → launch browser (Settings.Browser)
Per test      UiTestBase.InitializeAsync → driver.NewSessionAsync(BaseUrl, default timeout) → IUiSession
                                       → ITracing.StartAsync (Trace = on-failure | on)
              [Fact] public Task X() => RunAsync(async () => { ... })
                └─ TestRunner.RunAsync(logger, "ui-web", class, [CallerMemberName], browser)
                     ├─ log [Start]; report name "<test> (<browser>)" + "browser" parameter
                     ├─ body: page objects + Mothers/Builders + Report.Step + Ui.Expect
                     ├─ UiException → Assert.Fail (Allure shows "failed", not "broken")
                     ├─ on failure: IScreenshots + ITracing → report → rethrow
                     └─ finally: Allure Start/End/Duration → log [Finish] → publish LogMetadata
              UiTestBase.DisposeAsync → session.DisposeAsync()
Class end     BrowserFixture.DisposeAsync → driver.DisposeAsync() (closes the browser)
```

Each test gets fresh cookies and storage while the browser is launched once per class. Test classes run in parallel (xUnit's default).

### 4.2 API (`Tests/Api`)

`ApiFixture` holds the settings (no browser; the transport is created per test). `ApiTestBase` creates an `ApiClient` per test and disposes it afterwards, first running its `CleanupTracker` (which deletes every booking created through `CreateBookingAsync`, last first); tests use `RunAsync(async () => ...)` and `GetAuthHeadersAsync()` for PUT/PATCH/DELETE.

### 4.3 Mobile (`Tests/UI/Mobile`)

```text
mobilesettings.json → MobileSettingsProvider.Current (Platform, AppType, Server, Device, Android, IOS, Behavior)
   │  MobileOptionsBuilder.Parse → (Android|iOS, Web|Native) → Build → AppiumOptions
   ▼
MobileDriverFactory.CreateDriver → AndroidDriver / IOSDriver  (Appium 2 server)
   ▼
MobileTestBase.Run(body) → TestRunner: create driver → navigate (Web) / launch app (Native)
                → body → (failure: screenshot) → Allure timing → log → publish LogMetadata → quit
   ▼
Pages: Common/MobileBasePage · <App>/…Page
Helpers: Core/Mobile/Interactions/GestureHelper · WaitHelper  (settings + driver: Core.Mobile)
```

The full design, config reference, and local Appium/emulator setup are in [MOBILE_ARCHITECTURE.md](MOBILE_ARCHITECTURE.md).

Categories are xUnit traits (`[Trait("Category", "smoke")]`), so `dotnet test --filter "Category=smoke"` works.

---

## 5. CI/CD

```text
GitHub (source)  ──push / PR──►  Azure Pipelines (azure-pipelines.yml)  ──status checks──►  GitHub PR

Stage: BuildAndValidate
 ├─ Job Tests (ubuntu; web/API and framework checks)
      .NET 10 → build ci.slnf (Core + its add-on projects, Domain, Pages, Tests, Tests/Unit, Tests/UI/Mobile; warnings as errors)
      → dotnet test Tests/Unit (framework unit tests, ~1 s)
      → dotnet test Tests/Tests.csproj, BROWSER=chrome (preinstalled Chrome, no download)
          PR           → --filter "FullyQualifiedName!~.SauceDemo.|Category=smoke"
          main/release → all tests + coverage
      → nightly (schedule, main): install Playwright browsers → web UI tests on chromium, firefox, webkit
      → TRX (gate) + allure-results artifact
      → latest main allure-report/history + results → Allure CLI → allure-report artifact
        (HTML with trends + single-file allure-report.html)
 ├─ Job MobileSmoke (macOS-15; schedule/manual only)
      .NET 10 + pinned Node/Appium/UiAutomator2 → Android 14 emulator + Chrome
      → bounded boot/Appium readiness checks → mobile smoke tests (empty runs fail)
      → TRX + raw Allure + diagnostics in mobile-results → owned-process cleanup
 └─ Job WindowsHelperVerification (windows-2025; schedule/manual only)
      → isolated batch helper build-failure check → JSON/log artifact
```

Locally, `scripts/{windows,unix}/run_*_gen_report` run all / API / web tests through a shared `_run_playwright_tests` helper and collect the Allure results into `./allure-results`. `run_ui_web_all_browsers` runs the web tests once per browser and merges the results; `run_mobile_android_web` runs the mobile smoke tests. `docker-compose.yml` starts a local Elasticsearch 8 + Kibana (no auth) for the Elastic logging provider.

---

## 6. Extending the Framework

- **New web app or feature:** page objects in `Pages/<App>/`, models in `Domain/Models/<App>/`, a Builder + Mother per entity in `Domain/TestData/<App>/`, the app's URL in `testsettings.json` / `TestSettings`, an `<App>TestBase : UiTestBase` and tests in `Tests/UI/Web/<App>/`.
- **New API:** models in `Domain/Models/<Service>/`, a Builder + Mother per entity in `Domain/TestData/<Service>/`, service-specific helpers (e.g. auth) in `Domain/Api/<Service>/`, and tests in `Tests/Api/` inheriting `ApiTestBase`.
- **New logging sink:** implement `ILoggingProviderFactory` and `IResultsPublisherFactory` in `Core`, then select the sink with `Provider` in `logging-config.json`.
- **New mobile screen:** add a page under `Tests/UI/Mobile/Pages/<App>/` (inherit `MobileBasePage`) and tests in `Tests/UI/Mobile/<App>/` inheriting `MobileTestBase`.
- **Desktop (later):** add a `Tests/Desktop` project with the same layout as Mobile (Configuration/, Driver/, Pages/<App>/, Infrastructure/DesktopTestBase, <App>/ tests) using FlaUI, and a Windows job (`windows-latest`) to run it.

Update this document, and the matching diagram in [DIAGRAMS.md](DIAGRAMS.md), whenever a layer, a dependency direction, a test-run step or a CI flow changes.

### Roadmap

- Move to xUnit v3 (and a current `xunit.runner.visualstudio`) once `Allure.Xunit` supports it.
- Verify the first hosted macOS mobile smoke run and close R16 once results are published.
- Add native-app mobile samples (Android, then iOS).
- Desktop UI suite (FlaUI / UIA).

---

## 7. Revision History

| Date       | Notes |
|------------|-------|
| 2025-09-03 | Initial draft; baseline definition |
| 2025-09-03 | Added Mobile UI layer; Appium integration planned |
| 2026-11-13 | Selenium and RestSharp removed: web UI and API tests rewritten on Playwright (`Page` / `APIRequestContext`). New layout: `Core/` (merged shared library incl. API client, settings, browser factory), `Pages/`, `Tests/` (`Api/`, `UI/Web/`, `UI/Mobile/` Appium, `Desktop/`). `testsettings.json` + `BROWSER`/`HEADLESS` replace hard-coded Playwright settings. CI runs one test project with `BROWSER=chrome`. |
| 2026-11-13 | All suites moved to xUnit v2 + `Allure.Xunit` (NUnit removed). Shared `Core.Utilities.TestRunner` replaces SetUp/TearDown. Playwright now reports to Allure. CI publishes one merged `allure-results` artifact and filters Selenium with `DisplayName~chrome`. |
| 2026-11-13 | CI sped up: suites merged into one job, `ci.slnf` builds only the tested projects, Playwright uses the preinstalled Chrome (`PLAYWRIGHT_CHANNEL`), coverage only on main/release. The Playwright fixture now disposes `IPlaywright`. |
| 2026-11-13 | CI: Selenium runs Chrome cases only; Desktop job removed until the project has tests. |
| 2026-11-13 | CI/CD consolidated on Azure Pipelines sourced from GitHub; GitHub Actions retired; Selenium smoke/regression added to the pipeline. |
| 2026-11-13 | Phase 1 restructure: single `TestAutomationPlatform.slnx`, central package versions, `scripts/{windows,unix}`, docs consolidated under `docs/`, Desktop project flattened. |
| 2026-11-13 | Merged GitHub `main` (mobile layer, sh scripts, Grid support, OpenSearch compose). Rewritten to match the codebase: .NET 10, Playwright/xUnit stack, NUnit stack internals (config, test data, pluggable logging), Azure DevOps + GitHub Actions flows, roadmap |
| 2026-11-13 | Consistency pass: models in `Domain/Models/<App>/`, a Builder + Object Mother per entity for every app in `Domain/TestData/<App>/` (JSON test data removed; `TestDataLoader` kept for large tables); namespaces match folders (`Core.Utilities` removed); `LoggingConfigProvider`; `TodoTestBase`; uniform Allure labels; Mobile restructured (`Configuration/`, `Driver/`, `Pages/<App>/`, `Infrastructure/`, `mobilesettings.json`); projects renamed `Tests.UI.Mobile` / `Tests.Desktop`; LF line endings. |
| 2026-11-14 | Review P1 fixes: Elastic credentials only from environment variables (and the certificate fingerprint has its own setting); `docker-compose.yml` switched from OpenSearch to Elasticsearch 8 + Kibana; reachability probe cached per URL, loggers and publisher created once per run. |
| 2026-11-14 | Review R4–R20: traces on failure, navigation retry, API cleanup, one `SettingsLoader`, `Directory.Build.props` (style in build, warnings as errors in CI), pages return next page + test-id locators, test naming, `api-logs-*`, generic `ApiClient` headers, nightly cross-browser run, Allure HTML report artifact, `Tests/Unit`, mobile compiled in CI; Desktop project and `.npmrc` removed. |
| 2026-11-17 | Core-architecture feedback: app-specific code moved to `Domain` (R34) with `CoreBoundaryTests`; shared retry/polling in `Core/Resilience` with Polly (R35); mobile bootstrapping moved to `Core.Mobile` (R38). |
| 2026-11-17 | Core add-ons moved inside `Core/` next to the area they extend (`Core/Mobile`, `Core/Messaging/{Kafka,RabbitMq}`, `Core/Security/AzureKeyVault`), still separate projects excluded from `Core.csproj`; `Core.Secrets.AzureKeyVault` renamed `Core.Security.AzureKeyVault`; solution groups them under `/Core/`. |
| 2026-11-17 | Mermaid flow diagrams added in `docs/DIAGRAMS.md`: project dependencies, web UI / API / mobile test runs, settings and secrets, CI pipeline. |
| 2026-11-17 | `docs/PLUGIN_ARCHITECTURE.md` added (R43): plug-and-play target shape, contract list, dependency inventory, design rules, provider guide, accepted host/platform dependencies. |
| 2026-11-17 | Composition root (R40): `providers.json` + `Core/Composition` (`Framework.Configure`, `IFrameworkPlugin`, `ProviderRegistry`) replace the reflection scan and the per-project Key Vault wiring; Elasticsearch moved out of `Core` into the `Core.Logging.Elastic` adapter (R44); first adapter guardrail tests (R46). |
| 2026-11-17 | Reporting contract (R39): `Core/Reporting` (`ITestReporter`, `Report.Step`, built-in console reporter); Allure moved into the `Core.Reporting.Allure` adapter, selected with `"Reporting"` in `providers.json`; `TestRunner`, the base classes and all tests call the contract. |
| 2026-11-17 | HTTP transport contract (R41): `Core/Http` (`IHttpTransport`, `HttpRequestData`/`HttpResponseData`, built-in `HttpClient` transport) and the `Core.Http.Playwright` adapter, selected with `"Http"` in `providers.json`; `ApiClient` keeps JSON, status and retries; `ApiFixture` no longer owns Playwright; transport contract tests. |
| 2026-11-17 | Resilience contract (R45): `IRetryPolicy` / `IRetryPolicyFactory` with a built-in retry and the `Core.Resilience.Polly` adapter, selected with `"Resilience"` in `providers.json`; `RetryPolicies` keeps its named policies and no longer references Polly or Playwright; `SettingsLoader.ResolveSecrets` internal (no `IConfiguration` in Core's public API). |
| 2026-11-17 | UI contract (R42): `Core/Ui` (`IUiDriverFactory`/`IUiDriver`/`IUiSession`, `IElement`, `Locate`, `Ui.Expect`, `UiException`, capabilities `IScreenshots`/`ITracing`); Playwright moved into the `Core.Ui.Playwright` adapter, so `Core` has no vendor package left; page objects, `UiTestBase` and web tests use the contract; `PlaywrightFixture` → `BrowserFixture`; new `Tests/Contract` project with the UI driver contract suite, run in CI. |
