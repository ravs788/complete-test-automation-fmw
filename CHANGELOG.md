# Changelog

## 2026-11-13

### CI/CD
- **Azure Pipelines is the only CI/CD.** The source lives on GitHub, and the pipeline (`ravs788.complete-test-automation-fmw`) connects through the Azure Pipelines GitHub App, so results appear as checks on GitHub PRs.
- Triggers: PRs into `main` run API + Playwright (full) and Selenium smoke. Pushes to `main` / `release/*` run API + Playwright (full) and Selenium regression.
- New **SeleniumWeb** job. It runs **Chrome test cases only** (`Name~chrome`) because Edge crashes at startup on the hosted Linux agent. Firefox and Edge cases still run locally.
- Allure results are published as pipeline artifacts (`allure-results-api`, `allure-results-ui-web`).
- Removed: the GitHub Actions workflows, the Desktop build-only job (the project has no tests yet), and the old Azure Repos–based pipeline (disabled).

### Structure (Phase 1)
- One solution, `TestAutomationPlatform.slnx`, now holds all 8 projects. `modern-test-framework.sln` is removed.
- Central NuGet versions in `Directory.Packages.props`. Diverging versions are unified, and unused `FluentAssertions` is removed.
- `bat/` → `scripts/windows/`, `sh/` → `scripts/unix/`, `UI/Desktop/UI.Desktop.Tests/` → `UI/Desktop/`.
- Docs consolidated under `docs/` (`MOBILE_ARCHITECTURE.md`, `CONTRIBUTING.md`). Removed the placeholder `Framework.Examples/`.

### Code
- `LoggingConfig` finds `logging-config.json` directly instead of searching for a `*.sln`, which doesn't match `.slnx` on macOS/Linux.
- `BaseWebTest` forces headless mode on any CI agent (`TF_BUILD`, `GITHUB_ACTIONS`, or `CI`), not only GitHub Actions.
- `CheckoutCompletePage` waits for the confirmation header. This fixes a race with `PageLoadStrategy.Eager` that failed `EndToEnd_Checkout_Success` in CI.

### Merged from GitHub `main`
- Appium mobile layer (Android mobile web), `.sh` scripts, Selenium Grid and timeout settings, OpenSearch `docker-compose.yml`, `NegativeLoginTests` and `RemoveFromCartTests`, and NUnit `[Category]` attributes.

### Docs
- README, `docs/ARCHITECTURE.md` and `docs/GETTING_STARTED.md` rewritten to match the codebase: both web stacks, the mobile layer, configuration, logging, and CI/CD.

## Earlier: elastic logging

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
