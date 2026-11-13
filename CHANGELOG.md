# Changelog

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
