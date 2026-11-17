# Changelog

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
