# Modern C# Test Automation Framework: Architecture

## Overview

This framework is a modular C# / .NET 10 test automation platform for REST API, web UI and Android mobile-web testing. **Playwright** drives both the web UI and the API tests; **Appium** drives the mobile tests. Every suite runs on **xUnit v2 with Allure (`Allure.Xunit`)**. Source lives on GitHub, and CI/CD runs on Azure DevOps Pipelines. Results go to Allure, TRX, and an optional Elasticsearch sink.

| Area | Location | Driver/Client | In CI |
|------|----------|---------------|-------|
| API | `Tests/Api` | Playwright `APIRequestContext` | Yes |
| Web UI | `Tests/UI/Web` (SauceDemo, TodoMVC) | Playwright (Chromium, Chrome, Edge, Firefox, WebKit) | Yes (Chrome) |
| Mobile web | `Tests/UI/Mobile` | Appium 2 (Android; iOS/native in the bootstrap) | No (needs an emulator) |

Why xUnit **v2**: `Allure.Xunit` doesn't support xUnit v3 yet. `xunit.runner.visualstudio` is pinned to 2.8.2 because adapters from 3.0 on are built on the v3 runner and can't load Allure's reporter.

---

## 1. Goals

- **Transportable:** moves to new applications and environments with minimal setup.
- **Maintainable:** clear layering (Core → Pages → Tests) and small, single-purpose classes.
- **One web/API tool:** Playwright for browsers and HTTP, so UI and API tests share fixtures, configuration and reporting.
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
│                   + PlaywrightFixture           Configuration/ Driver/   │
│     testsettings.json                           Pages/<App>/ <App>/      │
│                                                 mobilesettings.json      │
├──────────────────────────────────────────────────────────────────────────┤
│ Pages/  (Pages.csproj, Playwright)                                       │
│   Common/BasePage · SauceDemo/LoginPage, InventoryPage, CartPage,        │
│   Checkout* · Todo/TodoPage                                              │
├──────────────────────────────────────────────────────────────────────────┤
│ Core/   (Core.csproj, shared by everything)                              │
│   Api/            ApiClient, ApiException · RestfulBooker/AuthClient     │
│   Configuration/  TestSettings (+ sections) + ConfigurationProvider      │
│   Driver/         PlaywrightDriverFactory                                │
│   Logging/        factories, LogMetadata, LoggingConfig + Provider       │
│                   Console/ · Elastic/ providers                          │
│   Models/<App>/   data shapes (RestfulBooker, SauceDemo, Todo)           │
│   TestData/       TestDataLoader · <App>/ Builders + Object Mothers      │
│   Testing/        TestRunner + TestRunInfo                               │
└──────────────────────────────────────────────────────────────────────────┘
```

Dependency rules:

- `Tests` → `Pages` → `Core`. `Tests/UI/Mobile` → `Core`. Nothing depends on `Tests`.
- `Core` has no dependency on a test framework. It references Playwright, `Allure.Net.Commons` (the framework-neutral Allure core) and the Elasticsearch client.
- `Tests/UI/Mobile` and `Tests/Unit` (framework unit tests: builders, settings loading, `TestRunner`, …) are separate projects inside the `Tests/` folder; `Tests.csproj` excludes them (`DefaultItemExcludes`).
- Selenium remains a package only because Appium's .NET client is built on it.
- The mobile suite has its own `Pages/` because the shared `Pages` project is built on Playwright types.

Conventions (the same in every project):

- **`<Layer>/<App>/`**: app-specific code sits in an app folder under its layer (`Pages/SauceDemo`, `Core/Models/RestfulBooker`, `Core/TestData/Todo`, `Core/Api/RestfulBooker`, `Tests/UI/Web/Todo`, `Tests/UI/Mobile/SauceDemo`). Generic code sits at the layer root (`Core/Api/ApiClient`).
- **Namespace = folder path**, assembly name = root namespace (`Core`, `Pages`, `Tests`, `Tests.Unit`, `Tests.UI.Mobile`). File-scoped namespaces, one type per file.
- **Settings = record + cached provider**, all loaded by `SettingsLoader` (JSON → prefixed env vars → aliases): `TestSettings`/`ConfigurationProvider` (`TESTS_`), `LoggingConfig`/`LoggingConfigProvider` (`LOGGING_`), `MobileSettings`/`MobileSettingsProvider` (`MOBILE_`).
- **One base class per app** on top of the suite base (`SauceDemoTestBase`, `TodoTestBase`), and the same Allure labels on every class: `ParentSuite` = layer, `Suite` = app, `SubSuite` = feature.

---

## 3. Core

### 3.1 Configuration

- **One mechanism:** `SettingsLoader.Load<T>(dir, file, prefix, aliases)` (Microsoft.Extensions.Configuration) binds a record from the JSON file, then environment variables with the prefix (`__` = nesting), then aliases.
- **Playwright tests:** `ConfigurationProvider.Current`: `Tests/testsettings.json` from the output directory, `TESTS_*`, aliases `BROWSER` / `HEADLESS`. `TestSettings` holds `Browser`, `Headless`, `DefaultTimeoutMs` and one section per app (`TodoMvc`, `SauceDemo`, `RestfulBooker` incl. credentials).
- **Logging:** `LoggingConfigProvider.Current`: `logging-config.json` (found by walking up from the run directory), `LOGGING_*`, aliases `ELASTIC_URL` / `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` / `ELASTIC_CERT_FINGERPRINT`. Credentials only come from the environment.
- **Mobile:** `MobileSettingsProvider.Current` (in the mobile project): `Tests/UI/Mobile/mobilesettings.json`, `MOBILE_*`, aliases `APPIUM_URL` / `DEVICE_NAME`.

### 3.2 Browsers

`PlaywrightDriverFactory.CreateBrowserAsync(playwright, browser, headless)` maps `chromium` (bundled), `chrome` / `msedge` (installed browsers via Playwright channels, no download), `firefox` and `webkit`. The caller owns and disposes the `IPlaywright` instance.

### 3.3 API client

`ApiClient` wraps a Playwright `IAPIRequestContext` (`BaseURL`, `Accept: application/json`). `GetAsync/PostAsync/PutAsync/PatchAsync` (de)serialize JSON with System.Text.Json (one set of options for requests and responses; `null` properties are left out) and throw `ApiException(StatusCode)` on non-2xx; `DeleteAsync` returns the status; `SendRawAsync` returns the raw response for status assertions. Every method takes optional per-request `headers`; `ApiClient` has no knowledge of any API's auth scheme. Restful Booker's `AuthClient.GetAuthHeadersAsync` turns a token from `POST /auth` into the `token` cookie header.

### 3.4 Test data

- **Models** (all data shapes) are in `Core/Models/<App>/`: PascalCase properties mapped with `[JsonPropertyName]`, `sealed record`s.
- **Builders + Object Mothers, for every app** in `Core/TestData/<App>/`: `<Entity>Builder` returns a valid object by default (with generated unique values where collisions matter) and `With…` methods for variations; `<Entity>Mother` names the common cases (`BookingMother.Standard()`, `UserMother.LockedOut()`, `ProductMother.Backpack()`, `TodoItemMother.Completed()`). Tests get their entities from these.
- **`[Theory]` cases** come from a `[MemberData]` source built with the Builders/Mothers (e.g. `NegativeLoginTests.InvalidLogins()`), as plain values so xUnit v2 reports each case separately.
- **JSON is the exception**, used for large scenario tables, data owned outside the code, captured payloads or per-environment data. `TestDataLoader.Load<T>("<App>/<TestClass>/<Scenario>.json")` reads from `<output>/test-data/` (source: `Tests/test-data/`). There is none at the moment.

### 3.5 Logging & results publishing

```text
            logging-config.json  (found by walking up from the run dir)
                       │
        ┌──────────────┴───────────────┐
        ▼                              ▼
LoggingServiceFactory          ResultsPublisherFactory
  │ reflection: ILoggingProviderFactory / IResultsPublisherFactory
  │ 1. configured Provider  → 2. "console"  → 3. any other  → 4. console (direct)
  ▼                              ▼
ILoggingService               IResultsPublisher
 ├─ ConsoleLoggingService      ├─ ConsoleResultsPublisher
 └─ ElasticLoggingService      └─ ElasticResultsPublisher
        │ ElasticClientFactory(ELASTIC_SERVER profile)
        │ ElasticConnectivity probe (cached per URL for the run) → console fallback if unreachable
```

- Loggers are cached per index format and the publisher once per run; the reachability probe is cached per URL.
- To add a provider, implement both factory interfaces in `Core`. The factories discover providers by reflection, so no registration code is needed.
- Each test publishes one `LogMetadata` document (project, class, method, status, duration, reason, timing, browser, user) from `TestRunner`. A publish failure never fails the test.

---

## 4. Test Lifecycle

xUnit v2 creates a new test-class instance per test and has no teardown hook that knows the test's outcome, so each base class wraps the test body in `Core`'s `TestRunner`.

### 4.1 Web UI (`Tests/UI/Web`)

```text
Class start   PlaywrightFixture.InitializeAsync → Playwright.CreateAsync() → launch browser (Settings.Browser)
Per test      UiTestBase.InitializeAsync → new BrowserContext(BaseURL = app's BaseUrl) → new Page(default timeout)
                                       → start tracing (Trace = on-failure | on)
              [Fact] public Task X() => RunAsync(async () => { ... })
                └─ TestRunner.RunAsync(logger, "ui-web", class, [CallerMemberName], browser)
                     ├─ log [Start]; Allure name "<test> (<browser>)" + "browser" parameter
                     ├─ body: page objects + Mothers/Builders + AllureApi.Step + Assertions.Expect
                     ├─ PlaywrightException → Assert.Fail (Allure shows "failed", not "broken")
                     ├─ on failure: full-page screenshot + Playwright trace → Allure → rethrow
                     └─ finally: Allure Start/End/Duration → log [Finish] → publish LogMetadata
              UiTestBase.DisposeAsync → context.CloseAsync()
Class end     PlaywrightFixture.DisposeAsync → browser.CloseAsync() → playwright.Dispose()
```

Each test gets fresh cookies and storage while the browser is launched once per class. Test classes run in parallel (xUnit's default).

### 4.2 API (`Tests/Api`)

`ApiFixture` holds one Playwright instance per class (no browser). `ApiTestBase` creates an `ApiClient` per test and disposes it afterwards, first deleting any booking created through `CreateBookingAsync`; tests use `RunAsync(async () => ...)` and `GetAuthHeadersAsync()` for PUT/PATCH/DELETE.

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
Helpers: Interactions/GestureHelper · Interactions/WaitHelper
```

The full design, config reference, and local Appium/emulator setup are in [MOBILE_ARCHITECTURE.md](MOBILE_ARCHITECTURE.md).

Categories are xUnit traits (`[Trait("Category", "smoke")]`), so `dotnet test --filter "Category=smoke"` works.

---

## 5. CI/CD

```text
GitHub (source)  ──push / PR──►  Azure Pipelines (azure-pipelines.yml)  ──status checks──►  GitHub PR

Stage: BuildAndValidate
 └─ Job Tests (ubuntu; single job, since the org has one parallel job)
      .NET 10 → build ci.slnf (Core, Pages, Tests, Tests/Unit, Tests/UI/Mobile; warnings as errors)
      → dotnet test Tests/Unit (framework unit tests, ~1 s)
      → dotnet test Tests/Tests.csproj, BROWSER=chrome (preinstalled Chrome, no download)
          PR           → --filter "FullyQualifiedName!~.SauceDemo.|Category=smoke"
          main/release → all tests + coverage
      → TRX (gate) + allure-results artifact (+ categories.json)
```

Locally, `scripts/{windows,unix}/run_*_gen_report` run all / API / web tests through a shared `_run_playwright_tests` helper and collect the Allure results into `./allure-results`. `run_ui_web_all_browsers` runs the web tests once per browser and merges the results; `run_mobile_android_web` runs the mobile smoke tests. `docker-compose.yml` starts a local Elasticsearch 8 + Kibana (no auth) for the Elastic logging provider.

---

## 6. Extending the Framework

- **New web app or feature:** page objects in `Pages/<App>/`, models in `Core/Models/<App>/`, a Builder + Mother per entity in `Core/TestData/<App>/`, the app's URL in `testsettings.json` / `TestSettings`, an `<App>TestBase : UiTestBase` and tests in `Tests/UI/Web/<App>/`.
- **New API:** models in `Core/Models/<Service>/`, a Builder + Mother per entity in `Core/TestData/<Service>/`, service-specific helpers (e.g. auth) in `Core/Api/<Service>/`, and tests in `Tests/Api/` inheriting `ApiTestBase`.
- **New logging sink:** implement `ILoggingProviderFactory` and `IResultsPublisherFactory` in `Core`, then select the sink with `Provider` in `logging-config.json`.
- **New mobile screen:** add a page under `Tests/UI/Mobile/Pages/<App>/` (inherit `MobileBasePage`) and tests in `Tests/UI/Mobile/<App>/` inheriting `MobileTestBase`.
- **Desktop (later):** add a `Tests/Desktop` project with the same layout as Mobile (Configuration/, Driver/, Pages/<App>/, Infrastructure/DesktopTestBase, <App>/ tests) using FlaUI, and a Windows job (`windows-latest`) to run it.

Update this document whenever a layer, a dependency direction, or a CI flow changes.

### Roadmap

- Move to xUnit v3 (and a current `xunit.runner.visualstudio`) once `Allure.Xunit` supports it.
- Add mobile tests to CI (emulator job or device cloud).
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
| 2026-11-13 | Consistency pass: models in `Core/Models/<App>/`, a Builder + Object Mother per entity for every app in `Core/TestData/<App>/` (JSON test data removed; `TestDataLoader` kept for large tables); namespaces match folders (`Core.Utilities` removed); `LoggingConfigProvider`; `TodoTestBase`; uniform Allure labels; Mobile restructured (`Configuration/`, `Driver/`, `Pages/<App>/`, `Infrastructure/`, `mobilesettings.json`); projects renamed `Tests.UI.Mobile` / `Tests.Desktop`; LF line endings. |
| 2026-11-14 | Review P1 fixes: Elastic credentials only from environment variables (and the certificate fingerprint has its own setting); `docker-compose.yml` switched from OpenSearch to Elasticsearch 8 + Kibana; reachability probe cached per URL, loggers and publisher created once per run. |
