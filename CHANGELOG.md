# Changelog

## 2026-11-18: Adapter failure-path guarantees and review reconciliation (R49–R53)

- Selenium rejects ambiguous single-element actions, reads and scalar assertions, matching the Playwright contract; `First` is explicit and collection operations remain supported. Added shared driver regressions for hidden/disabled duplicates and unintended side effects.
- Playwright HTTP uses an owned context per request, honors pre-cancellation, aborts/drains cancelled requests, isolates concurrent requests and supports reuse after cancellation. Both HTTP transports run the same cancellation/disposal cases.
- RabbitMQ startup/disposal releases partially acquired resources and preserves the original error. Kafka handles duplicate headers with last-value-wins semantics, maps null header values to empty strings, exposes worker completion/failures and releases the consumer after worker/close errors.
- Added broker lifecycle regression tests using vendor-interface doubles; refreshed the review summary while preserving dated historical evidence. R16/R32/R33 remain blocked at the user's request; R36 still awaits real Key Vault verification.
- Validation: 191 framework tests passed, 2 live-broker cases skipped; 28 driver contract cases passed in headed Chrome. No instrumented coverage or new mobile/live-broker/vault integration run is claimed.

## 2026-11-17: xUnit as a thin host layer; label attributes (R48, R46, phase 7)

- New framework attributes **`[ReportSuite(layer, app, feature)]`** and **`[ReportTags(...)]`** in `Core.Reporting`; `TestRunner` passes them to the reporter (`ITestReporter.SetSuite`; the Allure adapter writes the parentSuite / suite / subSuite and tag labels). All 13 test classes use them instead of `[AllureParentSuite]` / `[AllureSuite]` / `[AllureSubSuite]` / `[AllureTag]`, so **scenario code imports no vendor library**. Verified: Allure results (labels included) identical to the baseline for the 15 Playwright tests and the Android smoke.
- `TestHostBoundaryTests` (metadata only; no test-project code runs): test classes use xUnit only through `[Fact]` / `[Theory]` / `[Trait]` and `Assert`, xUnit lifecycle interfaces only in fixtures and base classes, and `Tests`, `Tests.UI.Mobile` and `Domain` reference no vendor library besides xUnit. Checked that it fails when a test class gets an Allure attribute or an xUnit interface.
- R46 contract suites completed: reporting (console, Allure), logging and results publishing (console, Elastic on a closed port), secrets (environment, Key Vault with a fake client); the Key Vault fake moved to `Tests/Unit/Security/FakeKeyVault.cs`.
- Docs: how to label test classes (README, CONTRIBUTING, MOBILE_ARCHITECTURE), the host layer and migration path (PLUGIN_ARCHITECTURE §6).

## 2026-11-17: Selenium and Appium on the UI contract (R47, phase 6)

- New adapter project **`Core/Ui/Selenium`** (`SeleniumUiPlugin`, `"selenium"`): `WebDriverUiSession` / `WebDriverElement` for any W3C WebDriver, with polling waits, retries on stale or covered elements, a Playwright-like stability check before clicks, ARIA roles mapped to common tags, framework assertions polling for 5 s, and screenshots (no tracing). Browsers: installed Chrome, Edge or Firefox (each session is a new browser).
- **Appium is a UI driver** (`"appium"`, `AppiumUiPlugin` in `Core.Mobile`): its sessions reuse the WebDriver session and add the new `IMobileGestures` capability (swipe, scroll, scroll until visible). `providers.json` gains `"MobileUi"`. `Core.Mobile` → `Core.Ui.Selenium` is the one deliberate adapter-to-adapter reference.
- **The mobile suite uses the shared page objects and `Ui.Expect`**: `MobileTestBase` opens an `IUiSession` (`RunAsync`, `Session`), and the smoke test uses `Pages.SauceDemo.LoginPage` (its `Username` element is now public). Removed: `Tests/UI/Mobile/Pages` (`MobileBasePage`, mobile `LoginPage`), `Core.Mobile.Interactions.WaitHelper`, `ResilienceSettings.UiAction` and `MobileSettings.Resilience` (element waits and transient retries are built into the UI drivers), and the `Selenium.Support` / `DotNetSeleniumExtras.WaitHelpers` packages. New `IUiSession.TitleAsync()`.
- Verified: UI contract suite 18/18 (9 checks × Playwright and Selenium); Android smoke passes through the Appium UI driver with the shared `LoginPage` (Allure result identical to the baseline); web suite with `PROVIDERS_Ui=selenium BROWSER=chrome`: 8 of 8 pass (5 consecutive runs). During development SauceDemo's cart link briefly ignored WebDriver's native clicks (3 runs, reproduced with plain Selenium); it later navigated normally with the same markup, so it was the site, not the adapter. New unit check: every contract has at least two registered implementations.

## 2026-11-17: UI contract; Core has no vendor package left (R42, phase 5)

- New `Core/Ui`: `IUiDriverFactory` / `IUiDriver` / `IUiSession` (one isolated session per test), lazy auto-waiting `IElement` found by `Locate` (test id, role, placeholder, CSS; `WithChild` filter, `First`), web-first assertions `Ui.Expect(element).ToBeVisibleAsync / ToHaveTextAsync / ToContainTextAsync / ToHaveCountAsync / ToHaveClassAsync`, `UiException` / `UiAssertionException`, and optional capabilities `IScreenshots` / `ITracing` (`TryGet<T>`), with `Native<T>()` as a visible escape hatch. Selected with `"Ui"` in `providers.json`.
- **Playwright moved out of `Core`** into the adapter project `Core/Ui/Playwright` (`PlaywrightUiPlugin`; `PlaywrightDriverFactory` moved there). `Core` now references no vendor package (only Microsoft.Extensions.Configuration). Playwright timeouts and errors become `UiException`, so an element that never appears is now reported as **failed** rather than broken.
- Page objects (`BasePage(IUiSession)`, `IElement` instead of `ILocator`), `UiTestBase` (`Session` instead of `Page`/`Context`) and web tests (`Ui.Expect` instead of `Assertions.Expect`) use only the contract. **`PlaywrightFixture` is now `BrowserFixture`.** `Tests.csproj` no longer references Playwright directly (its install script and driver still arrive through the adapter).
- New test project **`Tests/Contract`**: UI driver contract tests (8 checks on a local `data:` page, run per driver adapter, no Allure), run in CI after the Playwright tests (`contract_results.trx`).
- Verified: 15/15 Playwright tests with Allure results identical to the baseline; contract 8/8; `AdapterBoundaryTests`: `Core` doesn't reference Playwright and `Pages` uses only the contract.

## 2026-11-17: Resilience contract (R45, phase 4)

- New contract in `Core/Resilience`: `IRetryPolicy` (sync/async, with or without a result), `IRetryPolicy<T>` (also retries on a transient result) and `IRetryPolicyFactory`, plus a built-in implementation (**`builtin`**: exponential backoff with jitter). New adapter project **`Core/Resilience/Polly`** (`PollyResiliencePlugin`, `"polly"`). Selected with `"Resilience"` in `providers.json` (`polly` committed). Both log the same `[Retry] attempt N of M after D ms: reason` line.
- `RetryPolicies.TransientHttp / Navigation / Transient` now return `IRetryPolicy` / `IRetryPolicy<T>` (were Polly `ResiliencePipeline`s) and take an optional `factory`. Network errors are recognised by `HttpRequestException` or by message, so `Core/Resilience` no longer references Playwright. `ApiClient`, `BasePage.GotoAsync` and `MobileBasePage` use the contract.
- `Core` no longer references `Polly.Core`. `SettingsLoader.ResolveSecrets` is internal, so `IConfiguration` is out of Core's public API; Microsoft.Extensions.Configuration stays as an accepted platform dependency (no `ISettingsSource` wrapper).
- Tests: the retry tests are now a contract suite run for both implementations (20 tests); Playwright suite 15/15 with `polly` and with `builtin`; `AdapterBoundaryTests` keeps Polly out of `Core`.
- Docs fix: the project-reference table in `docs/DIAGRAMS.md` listed `Core.Http.Playwright` under the mobile test project (a phase 3 slip); the rows now match the `.csproj` files.

## 2026-11-17: HTTP transport contract (R41, phase 3)

- New `Core/Http`: `IHttpTransport` / `IHttpTransportFactory`, `HttpRequestData`, `HttpResponseData` (status, status text, headers, body; fully read) and a built-in **`httpclient`** transport on .NET `HttpClient`. New adapter project **`Core/Http/Playwright`** (`PlaywrightHttpPlugin`, `"playwright"`; one Playwright instance shared by the run). Selected with `"Http"` in `providers.json` (`playwright` committed).
- `ApiClient.CreateAsync(baseUrl, retry, log, transports)` no longer takes an `IPlaywright`; it keeps JSON, `ApiException` and retries. **`SendRawAsync` now returns `HttpResponseData`** (was Playwright's `IAPIResponse`; `.Status` works as before). Transports report network failures as `HttpRequestException`, so retries don't depend on the library. `ApiFixture` no longer starts Playwright.
- Verified: the 7 API tests pass with `PROVIDERS_Http=playwright` and `=httpclient` (no code change); Allure results identical to the earlier baseline; new transport contract tests (5 checks, run against both transports on a loopback server) and `ApiClient` retry tests.

## 2026-11-17: Reporting contract (R39, phase 2)

- New `Core/Reporting`: `ITestReporter` (run start, test name, parameters, attachments, steps), `Report.Step(...)` for tests, and a built-in `console` reporter. Selected with `"Reporting"` in `providers.json` (`allure` committed; `PROVIDERS_Reporting=console` to run without it); an unregistered name falls back to console with one message.
- **Allure moved out of `Core`** into the adapter project `Core/Reporting/Allure` (`AllureReportingPlugin`); `Core` no longer references `Allure.Net.Commons`, and `Core.Testing.AllureResults` is replaced by the reporter's `BeginRun`.
- `TestRunner`, `UiTestBase`, `MobileTestBase` and all 13 test classes call the contract (`AllureApi.Step` → `Report.Step`). Class labels (`[AllureSuite]` …) stay as Allure.Xunit attributes.
- Verified: the Allure results of the 15 Playwright tests and the Android smoke are identical to before (names, status, labels, parameters, steps, attachments; ignoring ids and times); `TRACE=on` still attaches traces. 9 new unit tests; `AdapterBoundaryTests` now also keeps Allure out of `Core`.

## 2026-11-17: Composition root and plugin model (R40, phase 1)

- New `Core/Composition`: `Framework.Configure(params IFrameworkPlugin[])` is the composition root, called once from each test project's `TestRunSetup`; adapters register named providers through `IFrameworkPlugin` into a `ProviderRegistry`. The reflection scan in `LoggingServiceFactory` / `ResultsPublisherFactory` is gone.
- New **`providers.json`** (repo root, `PROVIDERS_*` overrides) chooses the provider per contract: `Logging`, `ResultsPublisher` (default: same as `Logging`), `Secrets` (ordered list). The run prints the selection once, e.g. `[Providers] Logging=elastic, ResultsPublisher=elastic, Secrets=environment`. Selecting a secret provider that isn't registered stops the run at startup with the registered names.
- **Elasticsearch moved out of `Core`** into the adapter project `Core/Logging/Elastic` (`Core.Logging.Elastic`, `ElasticLoggingPlugin`); `Core` no longer references `Elastic.Clients.Elasticsearch`. Console logging stays built in. Key Vault registers through `AzureKeyVaultPlugin`.
- **Behaviour changes:** the logging provider is no longer `Provider` in `logging-config.json` / `LOGGING_PROVIDER`; use `"Logging"` in `providers.json` or `PROVIDERS_Logging`. Key Vault is no longer switched on by `KEYVAULT_URI` alone; list `"azure-keyvault"` in `Secrets` (e.g. `PROVIDERS_Secrets__0=environment PROVIDERS_Secrets__1=azure-keyvault`) and set `KEYVAULT_URI`.
- Tests: 26 new unit tests (registry, `providers.json` loading, secret and logging selection with fallback, plugins) and `AdapterBoundaryTests` (Core doesn't reference Elastic, Azure, RabbitMQ or Kafka assemblies; each adapter depends on Core only among framework projects).
- Docs: README (configuration, logging, secrets, structure), ARCHITECTURE, GETTING_STARTED, DIAGRAMS, PLUGIN_ARCHITECTURE updated.

## 2026-11-17: Plug-and-play architecture doc (R43)

- New `docs/PLUGIN_ARCHITECTURE.md`: target shape (contracts in `Core`, one adapter project per library, a composition root selecting adapters from a `Providers` section), the contract list, a dependency inventory (role, kind, where each library's types leak today, realistic replacement, review item), design rules, a provider guide with the guarantees each kind of provider must keep, and the accepted host/platform dependencies (xUnit, Microsoft.Extensions, ADO.NET). Phase 0 of the roadmap in `docs/REVIEW.md`; no code changes.
- `docs/CONTRIBUTING.md`: "Adding or replacing a library" section; README and ARCHITECTURE link to the new doc.

## 2026-11-17: Flow diagrams (Mermaid)

- New `docs/DIAGRAMS.md` with six Mermaid diagrams that GitHub renders: projects and layers, a web UI test run, an API test run (retries and cleanup), a mobile test run, settings and secrets resolution, and the CI pipeline. Each shows one idea and only the main path (about eight boxes at most, one arrow per group); details are in tables under each diagram. Linked from README, ARCHITECTURE and CONTRIBUTING.

## 2026-11-17: Chromedriver keeps Chrome's data on Android (R33)

- `MobileOptionsBuilder` now sets `goog:chromeOptions.androidKeepAppDataDir` from `Behavior.NoReset` (default true). Chromedriver otherwise clears Chrome's data before every web session; in run 130 that undid the CI warm-up, and the cold Chrome start on the hosted emulator ended in `chrome not reachable`. Set `NoReset: false` to get a clean Chrome profile per session. Verified locally (Appium 3.6 + UiAutomator2 8.7.3, Android 14 arm64 emulator, Chrome 113): smoke test passes and the device log shows no data clear. The name `androidKeepAppData` (first tried in PR #35) is rejected by Chromedriver.
- Docs: local mobile setup notes which UiAutomator2 version matches which Appium (Appium 3 needs `uiautomator2@8.x`), and how to set up the Android SDK with Homebrew only.

## 2026-11-17: Core add-ons grouped under Core/

- The add-on projects now live inside `Core/`, next to the area they extend: `Core.Mobile/` → `Core/Mobile/`, `Core.Messaging.Kafka/` → `Core/Messaging/Kafka/`, `Core.Messaging.RabbitMq/` → `Core/Messaging/RabbitMq/`, `Core.Secrets.AzureKeyVault/` → `Core/Security/AzureKeyVault/`. They stay separate projects (`Core.csproj` excludes their folders), so Appium, the broker clients and the Azure SDK still reach only the suites that reference them.
- **Renamed:** `Core.Secrets.AzureKeyVault` → `Core.Security.AzureKeyVault` (project, assembly and namespace), so it matches its folder next to `ISecretProvider`. Update `using Core.Secrets.AzureKeyVault;` and any project reference to it.
- `TestAutomationPlatform.slnx` groups Core and its add-ons under one `/Core/` solution folder; `ci.slnf` uses the new paths.

## 2026-11-17: Longer Android boot budget in CI (R32)

- The `MobileSmoke` job now passes `BOOT_TIMEOUT_SECONDS=900` (was the script default, 600) and its job limit is 45 minutes (was 35). Run 127 failed only because the hosted emulator took longer than 600 s to report boot completed; earlier runs booted in 446–505 s.

## 2026-11-17: Message-broker listeners (R37)

- New `Core/Messaging`: `ReceivedMessage` (broker-neutral, `BodyAs<T>()`) and `MessageCollector<T>` (thread-safe buffer + `WaitForAsync(match, timeout)` on `Poll`).
- New optional add-ons: **`Core.Messaging.RabbitMq`** (`RabbitMqListener`: exclusive, auto-delete queue bound to an exchange; RabbitMQ.Client 7.2.2) and **`Core.Messaging.Kafka`** (`KafkaListener`: manual partition assignment at the end offsets, no commits, so no stored state; Confluent.Kafka 2.16.0).
- `RequiresEnvironmentFact`: broker integration tests in `Tests/Unit` run only when `RABBITMQ_URI` / `KAFKA_BOOTSTRAP` are set (otherwise reported as skipped). Verified against RabbitMQ 3.13 and Kafka 3.8 in Docker (3 runs in a row, nothing left on either broker).

## 2026-11-17: Azure Key Vault secrets (R36)

- New optional add-on **`Core.Secrets.AzureKeyVault`** (Azure.Security.KeyVault.Secrets 4.12.0, Azure.Identity 1.21.0): `AzureKeyVaultSecretProvider` resolves `secret://NAME` from the vault in `KEYVAULT_URI` with `DefaultAzureCredential`, caches each secret for the run, returns null for a missing secret and surfaces access errors.
- `Core.Security.CompositeSecretProvider` chains providers. When `KEYVAULT_URI` is set, the test projects' module initializers use environment variables first, then Key Vault. Core itself has no Azure dependency.

## 2026-11-17: Mobile CI session start (R32, R33)

- `run_mobile_ci.sh` now pre-downloads the Chromedriver matching the device's Chrome (legacy index below 115, Chrome for Testing from 115; falls back to Appium's own download if the lookup fails), pre-warms Chrome once with first-run screens disabled, and raises the .NET client's command timeout to 300 s. In run 118 these two steps happened inside the session and ran past the client's 120 s timeout.

## 2026-11-17: Cleanup tracking and database helpers (R37)

- `Core.Testing.CleanupTracker`: register how to remove what a test created; cleanups run last-first after the test, and failures are logged without failing the test. `ApiTestBase` now registers booking deletions on it (`Cleanup`), and tests can add their own.
- New `Core/Data`: `DbConnectionFactory` (any ADO.NET provider passed in by the test project, so Core has no database package) and `DbTestScope` (connection + transaction, **rolled back by default**, `ExecuteAsync` / `ScalarAsync<T>` / `CommitAsync`). Verified against a temporary SQLite database in `Tests/Unit` (Microsoft.Data.Sqlite, test-only).
- Not added: message-queue listeners (Kafka/RabbitMQ); there is no broker to use them with yet.

## 2026-11-17: Secret placeholders (R36)

- New `Core/Security`: `ISecretProvider` and `EnvironmentSecretProvider`. Any setting can be `secret://NAME`; `SettingsLoader` resolves it through `SettingsLoader.SecretProvider` (environment variables by default: `NAME`, then `NAME` upper-cased with `-`/`.` → `_`). A missing secret fails with the setting and secret named. A vault provider (e.g. Azure Key Vault) can be plugged in without changing Core; none is included yet.

## 2026-11-17: Reusable mobile bootstrapping in Core.Mobile (R38)

- New **`Core.Mobile`** library: `Configuration/` (`MobileSettings` + `MobileSettingsProvider`), `Driver/` (`MobileDriverFactory`, `MobileOptionsBuilder`, `MobilePlatform`, `MobileAppType`) and `Interactions/` (`WaitHelper`, `GestureHelper`), moved from `Tests/UI/Mobile` (namespaces `Tests.UI.Mobile.*` → `Core.Mobile.*`). Only mobile suites reference it, so `Core` keeps no Appium/Selenium dependency. Page objects, the test base and tests stay in `Tests/UI/Mobile`.
- `Tests/Unit` references `Core.Mobile` instead of the mobile test project. New boundary tests: `Core` doesn't depend on Appium, and `Core.Mobile` doesn't reference `Domain`, `Pages` or test projects.

## 2026-11-17: Shared retry and polling (R35)

- New `Core/Resilience` (Polly.Core 8.8.1): `RetryPolicies.TransientHttp` (502/503/504 and network errors; `ApiClient` applies it to idempotent methods only, never POST/PATCH), `RetryPolicies.Navigation` (`net::ERR_*`), `RetryPolicies.Transient` (sync/async, e.g. mobile UI glitches), and `Poll.UntilAsync` for eventually consistent backends. Exponential backoff with jitter; each retry is logged.
- The hand-written retry loops in `BasePage.GotoAsync` and `MobileBasePage.SafeClick/SafeType` now use these policies. Retry counts are settings (`Resilience.Http/Navigation/UiAction` in testsettings.json and mobilesettings.json).

## 2026-11-17: Core is domain-agnostic (R34)

- App-specific code moved out of Core into a new **`Domain`** project, keeping `<Layer>/<App>/`: `Domain/Api/RestfulBooker` (auth), `Domain/Models/{RestfulBooker,SauceDemo,Todo}`, `Domain/TestData/{RestfulBooker,SauceDemo,Todo}` (Builders / Object Mothers). Namespaces change from `Core.` to `Domain.`. Core keeps the generic `ApiClient`, `TestDataLoader`, configuration, logging, drivers and `TestRunner`.
- `Tests/Unit/Architecture/CoreBoundaryTests` fails the build if Core references `Domain`, `Pages` or test projects, or contains app-named namespaces.

## 2026-11-14: Review items R21–R23

- **R21:** Elastic is only used when it can really be written to: `GET /` with the configured credentials must return 2xx from Elasticsearch 8+ (not OpenSearch). Otherwise one message explains why, and logging uses the console. If writes fail later, Elastic logging switches to the console once, and results publishing stops after one failure.
- **R22:** removed the unused `Console.Enabled` setting.
- **R23:** each test run clears the previous Allure results first; the pipeline copies results out after every `dotnet test`.

## 2026-11-14: Review items R4–R20

- **Debugging:** a Playwright trace is attached to Allure when a UI test fails (`Trace`: off | on-failure | on; `TRACE`) (R4). The first page load is retried on `net::ERR_*` network errors (R20).
- **Test hygiene:** API tests delete the bookings they create (`ApiTestBase.CreateBookingAsync`) (R5).
- **Configuration:** one `SettingsLoader` (Microsoft.Extensions.Configuration) for all settings files: JSON, then `TESTS_` / `LOGGING_` / `MOBILE_` environment variables, then aliases (R6).
- **Build:** `Directory.Build.props` holds the shared settings; code style is enforced in the build; warnings are errors in CI (R7).
- **Page objects:** navigation methods return the next page (R8); locators use test ids and roles instead of CSS classes (`BasePage.TestId`, `SauceDemoPage` uses `data-test`) (R9).
- **Conventions:** all tests are named `Subject_Should_Outcome[_When_Condition]` (R10, resets Allure history once); the API log index is `api-logs-*` (R12, update Kibana views that used `test-logs-*`).
- **API layer:** `ApiClient` takes per-request headers; Restful Booker builds its own auth (`AuthClient.GetAuthHeadersAsync`) (R11).
- **CI:** nightly run of the web tests on Chromium, Firefox and WebKit (R13); an `allure-report` artifact with history and a single-file HTML (R14); framework unit tests run first (R18); the mobile project is compiled in CI (R16, partly done).
- **Cleanup:** removed the empty `Tests/Desktop` (R15), the private-registry `.npmrc` (R17) and `ITestDataLoader` (R19).

## 2026-11-14: Review P1 fixes (R1–R3)

- **R1, no credentials in the repo.** `logging-config.json` no longer contains an Elastic username or password. Credentials come from `ELASTIC_USERNAME` / `ELASTIC_PASSWORD`. New overrides: `LOGGING_PROVIDER`, `ELASTIC_URL`, `ELASTIC_CERT_FINGERPRINT`. The `ON_LOCALHOST_SECURE` profile used the *password* as the certificate fingerprint; it now has its own `Elastic.CertificateFingerprint` setting.
- **R2, local stack matches the client.** `docker-compose.yml` runs Elasticsearch 8.15 + Kibana (single node, security off) instead of OpenSearch, which rejected every write from Elastic's 8.x client with HTTP 406 even though the reachability probe reported it as up.
- **R3, one probe per run.** `ElasticConnectivity.IsReachable` caches its answer per URL; `LoggingServiceFactory` caches a logger per index and `ResultsPublisherFactory` one publisher. With an unreachable Elastic URL, the API suite went from 7.7 s to about 4 s wall-clock.
- `ElasticClientFactory` rewritten: shared profile/URL resolution (`CurrentProfile`, `ResolveUrl`), and its two nullable warnings are gone.

## 2026-11-13: Consistent structure and conventions

- **Models in one place, per app.** `Core/Models/RestfulBooker/`, `Core/Models/SauceDemo/`, `Core/Models/Todo/`, one type per file. Restful Booker models use PascalCase properties with `[JsonPropertyName]`, like SauceDemo. PATCH uses a `BookingPatch` model instead of a dictionary. The SauceDemo login data has the same `{ "user": … }` shape as the other SauceDemo files.
- **Test data the same way for every app.** Each entity has a Builder and an Object Mother in `Core/TestData/<App>/`: `BookingBuilder`/`BookingMother`, `UserBuilder`/`UserMother`, `ProductBuilder`/`ProductMother`, `CheckoutInfoBuilder`/`CheckoutInfoMother`, `TodoItemBuilder`/`TodoItemMother` (`StandardItem()` → `Standard()`). Tests get entities from these. Bookings get a unique last name and future dates by default, replacing the manual `Ticks` suffix. `NegativeLoginTests` gets its cases from a `[MemberData]` source built from the builders. All JSON test data and the wrapper records (`LoginTestData`, `CartTestData`, `CheckoutTestData`, `NegativeLoginTestData`) were removed. `TestDataLoader` stays for large scenario tables or data owned outside the code. `SauceDemoTestBase.LoginAsync` takes a `User`.
- **Namespaces match folders.** `Core.Utilities` is gone: `Core.Logging` (+ `.Console`, `.Elastic`), `Core.TestData`, `Core.Testing`, `Core.Configuration`. File-scoped namespaces and `using` placement are enforced in `.editorconfig`.
- **Logging config:** `LoggingConfig` is a plain record, loaded once by `LoggingConfigProvider` (it was re-read from disk on every call). The logging models moved from `Core/Models` to `Core/Logging`.
- **TodoMVC** got a `TodoTestBase` (like `SauceDemoTestBase`), and `TodoPage.OpenAsync()` (like `LoginPage.OpenAsync()`).
- **Allure labels** are the same on every class: `ParentSuite` (layer) / `Suite` (app) / `SubSuite` (feature), and tags in a fixed order.
- **Mobile** follows the same layout: `Configuration/` (`MobileSettings` + `MobileSettingsProvider`, `mobilesettings.json` replaces `config.json` and `Core`'s `ConfigLoader`), `Driver/`, `Interactions/`, `Pages/Common` + `Pages/<App>`, `Infrastructure/MobileTestBase`, tests in `<App>/`. The duplicated base pages and sample tests were merged. The machine-specific Chromedriver path was removed and the Appium port reset to the default 4723. The run scripts filter on `Category=smoke`.
- **Projects** `UI.Mobile.Tests` / `UI.Desktop.Tests` renamed to `Tests.UI.Mobile` / `Tests.Desktop`, so every assembly name equals its root namespace.
- **Line endings** normalized to LF (`.gitattributes`). `.bat` files keep CRLF.
- **Docs:** `GETTING_STARTED.md` has per-OS setup (Windows, macOS, Linux) and a support matrix. `MOBILE_ARCHITECTURE.md` describes the actual mobile suite instead of a plan.

## 2026-11-13: Playwright for UI and API; Core / Pages / Tests layout

- **Selenium and RestSharp removed.**
  - The SauceDemo web tests are rewritten on Playwright, using page objects in `Pages/SauceDemo/` and auto-retrying `Expect(...)` assertions.
  - The Restful Booker API tests run on Playwright's `APIRequestContext` (`Core/Api/ApiClient`).
- **New layout:**
  - `Core/` is the shared library: config, browser factory, API client, logging, `TestRunner` and test-data models.
  - `Pages/` holds the page objects.
  - `Tests/` holds `Api/` and `UI/Web/` (the Playwright tests, one project), plus `UI/Mobile/` (Appium, its own project) and `Desktop/` (placeholder).
  - `API/`, `UI/Web/`, `src/` and `tests/` are gone.
- **Mobile stays on Appium.** Selenium remains only as Appium's dependency.
- **Settings:** `Tests/testsettings.json` replaces the per-project `config.json` files and hard-coded Playwright settings, with `BROWSER` (chromium, chrome, msedge, firefox, webkit) and `HEADLESS` overrides. `PLAYWRIGHT_CHANNEL` is replaced by `BROWSER=chrome`.
- **Allure:**
  - UI tests carry the browser in their name and as a parameter.
  - A failed Playwright `Expect` is reported as **failed** (not broken).
  - A full-page screenshot is attached on failure, including for the TodoMVC tests.
- **CI:** one `dotnet test Tests/Tests.csproj` with `BROWSER=chrome`. PRs run API + TodoMVC + SauceDemo smoke (the same 10 tests as before); `main` runs everything.
- **Scripts:** the `run_*_gen_report` scripts share `_run_playwright_tests`. `run_ui_web_all_browsers` loops over `BROWSERS` (default `chromium firefox webkit`).
- **Removed as unused:** `Directory.Build.props`, root `package.json`/`package-lock.json`, `Core/Screenshots`, `kill_all_webdrivers`, `detect_net_version.sh`, `exclude_envprops.txt`, NUnit-era `AssertHelper`.

## 2026-11-13: xUnit + Allure for every suite

- **NUnit removed.** The API, Selenium (UI/Web) and Mobile suites now use xUnit v2, like Playwright and Desktop.
  - `[Test]`/`[TestCase]` → `[Fact]`/`[Theory]` + `[InlineData]`; `[Category]` → `[Trait("Category", ...)]`.
  - Asserts moved to xUnit `Assert.*`.
  - Selenium browser filter: `Name~chrome` → `DisplayName~chrome`.
- **Allure everywhere** via `Allure.Xunit` 2.15. The Playwright suite now reports to Allure too. Attributes come from `Allure.Net.Commons.Attributes`.
- **`Core.Utilities.TestRunner`** replaces the per-suite SetUp/TearDown code. It handles logging, Allure timing parameters, result publishing, the failure screenshot and driver cleanup. Tests wrap their body in `Run(...)` / `RunAsync(...)`.
- **Version pins:** xUnit stays on v2 and `xunit.runner.visualstudio` on 2.8.2. Adapters from 3.0 on can't load Allure's reporter.
- **CI:** one merged `allure-results` artifact (with `categories.json`) replaces `allure-results-api` and `allure-results-ui-web`.
- **Scripts:** `run_all_tests_gen_report` also runs Playwright. `run_ui_web_all_browsers` now really runs one browser per pass (`--filter DisplayName~<browser>`).

## 🆕 What's New Since Last Push to main

- Introduced pluggable logging:
  - Core/Utilities/Logging/Common with LoggingServiceFactory and ResultsPublisherFactory
  - Providers: Console and Elastic (with connectivity and client factories)
  - Global configuration via logging-config.json
- Per-project configuration:
  - Core.ConfigLoader replaces Core ConfigManager
  - Project-local ConfigSettings in API and UI/Web, reading their own config.json
  - Updated UI BaseWebTest and API utilities (ApiClient, AuthHelper) to use ConfigLoader
- Results publishing abstraction:
  - IResultsPublisher and factory; Console and Elastic implementations
  - BaseWebTest and BaseApiTest publish results via factory
- Refined structure around screenshots:
  - Core defines interface and stub; UI/Web provides working implementation
- Documentation updates:
  - Project structure section reflects new utilities
  - References corrected to BaseWebTest.cs
