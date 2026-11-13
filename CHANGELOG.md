# Changelog

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
