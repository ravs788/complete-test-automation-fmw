# Modern C# Test Automation Framework: Architecture

## Overview

This framework is a modular C# / .NET 10 test automation platform that covers REST API, Web UI, and Android mobile-web testing. A Desktop project exists as a placeholder. Source lives on GitHub, and CI/CD runs on Azure DevOps Pipelines. It reports through Allure, TRX, and an optional Elasticsearch sink.

Every suite runs on **xUnit v2 with Allure (`Allure.Xunit`)**. The repository contains **two stacks** that share no code:

| Stack | Projects | Runner | Driver/Client | Reporting | Status |
|-------|----------|--------|---------------|-----------|--------|
| **Core stack** | `Core`, `API`, `UI/Web`, `UI/Mobile` | xUnit 2 | RestSharp, Selenium 4 (local or Grid), Appium 2 | Allure + TRX + Console/Elastic results | Mature: full SauceDemo and Restful Booker suites; Android mobile-web samples |
| **Playwright stack** | `src/Automation.Core`, `src/Automation.Pages`, `tests/Automation.Tests` | xUnit 2 | Microsoft.Playwright | Allure + TRX + Cobertura coverage | Newer: TodoMVC sample; the Azure DevOps web suite |

Why xUnit **v2**: `Allure.Xunit` doesn't support xUnit v3 yet. `xunit.runner.visualstudio` is pinned to 2.8.2 because adapters from 3.0 on are built on the v3 runner and can't load Allure's reporter.

The Playwright stack is the forward-looking web stack: it is async-first, isolates each test in its own browser context, and uses web-first assertions. The Selenium suite also runs in Azure Pipelines (smoke on PRs, regression on `main`). The Appium suite runs locally through `scripts/`.

---

## 1. Goals

- **Transportable:** moves to new applications and environments with minimal setup.
- **Maintainable:** clear layering (config → driver → pages → tests) and small, single-purpose classes.
- **Comprehensive:** API, browser, and mobile automation today; a desktop slot is reserved.
- **Observable:** Allure reports, TRX for CI dashboards, optional Elasticsearch for logs and trends.
- **CI/CD ready:** Azure DevOps Pipelines, triggered from GitHub.

---

## 2. Layered View

```text
┌──────────────────────────────────────────────────────────────────────────┐
│                               Test Layer                                 │
│  API/Tests      UI/Web/Tests     UI/Mobile/Tests   tests/Automation.Tests│
│  BaseApiTest    BaseWebTest      BaseMobileTest    PlaywrightFixture     │
├──────────────────────────────────────────────────────────────────────────┤
│                      Page Objects / Clients                              │
│  API/Utilities  UI/Web/Pages     UI/Mobile/Pages   src/Automation.Pages  │
│  ApiClient,Auth (Selenium POM)   + Drivers/Bootstrap  BasePage, TodoPage │
├──────────────────────────────────────────────────────────────────────────┤
│                    Shared Infrastructure                                 │
│  Core (Core stack)                      src/Automation.Core (Playwright) │
│   • ConfigLoader / TestDataLoader        • TestSettings + Provider       │
│   • Logging + Results (Console/Elastic)  • PlaywrightDriverFactory       │
│   • IScreenshotHelper · TestRunner       • TodoItem Builder / Mother     │
├──────────────────────────────────────────────────────────────────────────┤
│  Configuration & Data:  config.json (per project) · logging-config.json  │
│                         test-data/<TestClass>/*.json · Directory.Build.props
└──────────────────────────────────────────────────────────────────────────┘
```

Dependency rules:

- `API`, `UI/Web`, and `UI/Mobile` depend on `Core`. `Core` has no dependency on a test framework or Selenium. It references only `Allure.Net.Commons`, the framework-neutral Allure core, for `TestRunner`. The only UI-related type in Core is the `IScreenshotHelper` interface plus a stub, and `UI/Web` supplies the real implementation.
- `Automation.Pages` depends on `Automation.Core`. `Automation.Tests` depends on both.
- The two stacks share no code.

---

## 3. Core Stack (API, UI/Web, UI/Mobile)

### 3.1 Configuration

- Each test project owns a `config.json` and a typed `ConfigSettings` class, and copies `config.json` to the output directory on build.
- `Core.Utilities.ConfigLoader.Load<T>()` deserializes `AppContext.BaseDirectory/config.json`.
- `Allure.Xunit` writes each project's results to `bin/<Configuration>/net10.0/allure-results/`. The `run_all_tests_gen_report` scripts and CI merge them into one folder.

### 3.2 Test data

- `TestDataLoader.Instance.Load<T>("<TestClass>/<Scenario>.json")` reads from `<output>/test-data/`.
- The data files live next to the project, under `test-data/<TestClass>/`, and are copied on build.

### 3.3 Logging & results publishing

```text
            logging-config.json  (found by walking up from the run dir)
                       │
        ┌──────────────┴───────────────┐
        ▼                              ▼
LoggingServiceFactory          ResultsPublisherFactory
  │ reflection: ILoggingProviderFactory / IResultsPublisherFactory
  │ 1. configured Provider  → 2. "console"  → 3. any other  → 4. inline console
  ▼                              ▼
ILoggingService               IResultsPublisher
 ├─ ConsoleLoggingService      ├─ ConsoleResultsPublisher
 └─ ElasticLoggingService      └─ ElasticResultsPublisher
        │ ElasticClientFactory(ELASTIC_SERVER profile)
        │ ElasticConnectivity probe → disables itself if unreachable
```

- To add a provider, implement both factory interfaces in `Core`. The factories discover providers by reflection, so no registration code is needed.
- Each test publishes one `LogMetadata` document (project, class, method, status, duration, reason, timing, browser, user) from `TestRunner` (below). A publish failure never fails the test.

### 3.4 Test lifecycle

xUnit v2 creates a new test-class instance per test. It has no setup/teardown hook that knows the test's name or outcome, so each base class wraps the test body in `Core.Utilities.TestRunner`:

```text
[Theory] [InlineData("chrome")]                 ← browser is the theory argument
public void X(string browser) => Run(browser, () => { ... });

BaseWebTest.Run(browser, body)
  └─ TestRunner.Run(info = logger, project, class, [CallerMemberName], browser)
       ├─ log [Start]
       ├─ StartDriver(browser)
       │    ├─ ConfigLoader.Load<ConfigSettings>()
       │    ├─ create WebDriver (headless from config) → maximize → GoTo(BaseUrl)
       │    └─ rename Allure test "<name> (<browser>)"
       ├─ body: page objects + TestDataLoader + AllureApi.Step + Assert.*
       ├─ on exception: log [Failed] → screenshot → Allure → rethrow
       └─ finally: Allure params (Start/End/Duration) → log [Finish] + outcome
                   → publish LogMetadata → QuitDriver()
Dispose()  safety net: QuitDriver()
```

Driver creation: when `GridUrl` is set, `BaseWebTest` creates a `RemoteWebDriver`. Otherwise it uses a local driver from a configured path, or Selenium Manager when no path is set. It applies the configured timeouts, bypasses system proxies for the local WebDriver connection, and forces headless mode on CI agents (`TF_BUILD`, `GITHUB_ACTIONS`, or `CI` = `true`). Selenium test classes run sequentially (`[assembly: CollectionBehavior(DisableTestParallelization = true)]`).

`BaseApiTest` follows the same pattern without a driver: its constructor creates the `ApiClient`, `Dispose()` disposes it, and tests use `RunAsync(async () => ...)`. API test classes run in parallel (xUnit's default).

Categories are xUnit traits (`[Trait("Category", "smoke")]`), so `dotnet test --filter "Category=smoke"` still works. Select browsers with `DisplayName~chrome`.

### 3.5 Mobile layer (UI/Mobile)

```text
config.json (server, platform, appType, device, android, ios, behavior)
   │  MobileOptionsBuilder.Parse → (Android|iOS, Web|Native)
   ▼
Drivers/Bootstrap.cs → AppiumOptions → AndroidDriver / IOSDriver  (Appium 2 server)
   ▼
BaseMobileTest.Run(body) → TestRunner: create driver → navigate (Web) / launch app (Native)
                → body → (failure: screenshot) → Allure timing → log → publish LogMetadata → quit
   ▼
Pages: Web/MobileWebBasePage · Native/Android/AndroidBasePage
Helpers: Interaction/Gestures/GestureHelper · Interaction/Waits/WaitHelper
```

The full design, config reference, and local Appium/emulator setup are in [MOBILE_ARCHITECTURE.md](MOBILE_ARCHITECTURE.md).

---

## 4. Playwright Stack

### 4.1 Components

| Component | Responsibility |
|-----------|----------------|
| `TestSettings` (record) | Immutable settings: `BaseUrl`, `Browser`, `Headless`, `DefaultTimeoutMs` |
| `ConfigurationProvider` | Thread-safe `Lazy<TestSettings>` singleton (`Current`) |
| `PlaywrightDriverFactory` | `CreateBrowserAsync(playwright, type, headless)` for chromium/firefox/webkit (Chromium honours `PLAYWRIGHT_CHANNEL`, e.g. `chrome`); `CreateIsolatedContextAsync`. The caller owns and disposes the `IPlaywright` |
| `BasePage` | Primary-constructor base holding `IPage` |
| `TodoPage` | Intent methods (`AddTodoAsync`, `CompleteTodoAsync`, …) plus exposed `ILocator`s for assertions |
| `TodoItemBuilder` / `TodoItemMother` | GUID-backed test data that can't collide between tests |
| `PlaywrightFixture` | `IClassFixture` + `IAsyncLifetime`: owns the `IPlaywright` instance, one browser per class, `CreateCleanPageAsync()` per test |

### 4.2 Test lifecycle

```text
Class start   PlaywrightFixture.InitializeAsync → Playwright.CreateAsync() → launch browser (Settings)
Per test      TodoTests.InitializeAsync → new BrowserContext(BaseURL) → new Page(timeout)
              Arrange (Mother) → Act (page intent) → Assert (Assertions.Expect, auto-retry)
              TodoTests.DisposeAsync → context.CloseAsync()
Class end     PlaywrightFixture.DisposeAsync → browser.CloseAsync() → playwright.Dispose() (stops the driver process)
```

Isolating each test in its own context gives it fresh cookies and storage while keeping only one browser launch per class.

---

## 5. CI/CD

```text
GitHub (source)  ──push / PR──►  Azure Pipelines (azure-pipelines.yml)  ──status checks──►  GitHub PR

Stage: BuildAndValidate
 └─ Job Tests (ubuntu; single job, since the org has one parallel job)
      .NET 10 → build ci.slnf (CI projects only)
      → API → Playwright (system Chrome via PLAYWRIGHT_CHANNEL=chrome)
      → Selenium (DisplayName~chrome; PR → Category=smoke · main/release → Category=regression)
      → TRX (gate) + one merged allure-results artifact; Cobertura coverage on main/release only
```

Locally, the scripts in `scripts/windows/` and `scripts/unix/` run the clean → build → test → Allure-merge cycle for the API, Selenium and Playwright suites. `run_ui_web_all_browsers` produces one result set per browser and merges them, and `run_mobile_android_web` runs the mobile smoke tests. `docker-compose.yml` starts a local OpenSearch cluster with Dashboards for the Elastic logging provider.

---

## 6. Extending the Framework

- **New web feature (Playwright):** add a page in `src/Automation.Pages/<Feature>/`, builders/mothers in `src/Automation.Core/TestData/`, and tests in `tests/Automation.Tests/<Feature>/` using `PlaywrightFixture`.
- **New web feature (Selenium):** add a page in `UI/Web/Pages/`, data in `UI/Web/test-data/<TestClass>/`, and a test class inheriting `BaseWebTest`.
- **New API area:** add models and data under `API/`, and a test class inheriting `BaseApiTest`.
- **New logging sink:** implement `ILoggingProviderFactory` and `IResultsPublisherFactory` in `Core`, then select the sink with `Provider` in `logging-config.json`.
- **New mobile screen:** add a page under `UI/Mobile/Pages/Web/` or `Pages/Native/<Platform>/`, and a test inheriting `BaseMobileTest`.
- **Desktop:** fill in `UI/Desktop` with a base test class and page objects that follow the same pattern, and add a Windows job (`windows-latest`) to the pipeline to run it.

Update this document whenever a layer, a dependency direction, or a CI flow changes.

---

## 7. Roadmap

- Make `TestSettings` read from `appsettings`/environment variables (e.g. `HEADLESS`, `BROWSER`, `BASE_URL`).
- Phase 2 restructure: split reusable framework code (`src/Framework.*`) from test projects (`tests/*.Tests`) for every stack, keeping both web stacks.
- Move to xUnit v3 (and a current `xunit.runner.visualstudio`) once `Allure.Xunit` supports it.
- Attach screenshots on failure in the Playwright suite.
- Add mobile tests to CI, with `[Trait("Category", ...)]` attributes and an emulator job, and remove the machine-specific values from `UI/Mobile/config.json`.
- Add native-app mobile samples (Android, then iOS).
- Desktop UI suite (FlaUI / UIA).

---

## 8. Revision History

| Date       | Notes |
|------------|-------|
| 2025-09-03 | Initial draft; baseline definition |
| 2025-09-03 | Added Mobile UI layer; Appium integration planned |
| 2026-11-13 | All suites moved to xUnit v2 + `Allure.Xunit` (NUnit removed). Shared `Core.Utilities.TestRunner` replaces SetUp/TearDown. Playwright now reports to Allure. CI publishes one merged `allure-results` artifact and filters Selenium with `DisplayName~chrome`. |
| 2026-11-13 | CI sped up: suites merged into one job, `ci.slnf` builds only the tested projects, Playwright uses the preinstalled Chrome (`PLAYWRIGHT_CHANNEL`), coverage only on main/release. The Playwright fixture now disposes `IPlaywright`. |
| 2026-11-13 | CI: Selenium runs Chrome cases only; Desktop job removed until the project has tests. |
| 2026-11-13 | CI/CD consolidated on Azure Pipelines sourced from GitHub; GitHub Actions retired; Selenium smoke/regression added to the pipeline. |
| 2026-11-13 | Phase 1 restructure: single `TestAutomationPlatform.slnx`, central package versions, `scripts/{windows,unix}`, docs consolidated under `docs/`, Desktop project flattened. |
| 2026-11-13 | Merged GitHub `main` (mobile layer, sh scripts, Grid support, OpenSearch compose). Rewritten to match the codebase: .NET 10, Playwright/xUnit stack, NUnit stack internals (config, test data, pluggable logging), Azure DevOps + GitHub Actions flows, roadmap |
