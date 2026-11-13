# Framework Review: Open Items

Findings from the 2026-11-14 review, done alongside the consistency pass (see CHANGELOG). **None of these are fixed yet.** Each item names the affected files, what's wrong and a suggested fix. Priority: **P1** fix soon · **P2** should do · **P3** good practice · **P4** nice to have.

| # | Priority | Item | Area |
|---|---|---|---|
| 1 | P1 | Elastic password committed in `logging-config.json` | Security |
| 2 | P1 | Local logging stack (OpenSearch) vs Elasticsearch client | Logging |
| 3 | P1 | Elastic probed several times per test | Logging / speed |
| 4 | P2 | No Playwright trace on failure | Debugging |
| 5 | P2 | API tests leave bookings behind | Test hygiene |
| 6 | P2 | Three hand-written config providers, few env overrides | Configuration |
| 7 | P2 | No `Directory.Build.props`; project settings have drifted | Build |
| 8 | P3 | Page methods don't return the next page | Page objects |
| 9 | P3 | CSS-class locators | Page objects |
| 10 | P3 | Mixed test method naming | Conventions |
| 11 | P3 | Restful Booker auth inside the generic `ApiClient` | API layer |
| 12 | P3 | Logging index names differ by suite | Logging |
| 13 | P3 | CI only runs Chrome | CI |
| 14 | P3 | Allure published as raw results only | CI / reporting |
| 15 | P3 | `Tests/Desktop` is an empty project | Structure |
| 16 | P3 | Mobile suite not in CI | CI |
| 17 | P4 | `Tests/UI/Mobile/.npmrc` points at a private registry | Tooling |
| 18 | P4 | No unit tests for the framework itself | Quality |
| 19 | P4 | `ITestDataLoader` has one implementation | Simplification |

---

## P1: Fix soon

### 1. Elastic password committed in `logging-config.json`

- **Where:** `logging-config.json` (`Elastic.Password`).
- **Problem:** the file contains what looks like a real Elasticsearch password, and the README itself says not to commit credentials. It stays in the git history even after it's removed.
- **Fix:** if the password was ever real, rotate it. Replace it with a placeholder and read the real value from an environment variable (e.g. `ELASTIC_PASSWORD`; see item 6). Optionally add a secret scanner (e.g. gitleaks) to CI.

### 2. Local logging stack (OpenSearch) vs Elasticsearch client

- **Where:** `docker-compose.yml` (OpenSearch images), `Directory.Packages.props` (`Elastic.Clients.Elasticsearch` 8.12.0), `Core/Logging/Elastic/*`.
- **Problem:** `docker compose up` starts **OpenSearch**, but `Core` uses Elastic's 8.x .NET client. That client checks that the server identifies itself as Elasticsearch, so it will very likely reject OpenSearch, and the logs silently go to the console fallback instead.
- **Fix:** pick one product. Either switch `docker-compose.yml` to the Elasticsearch + Kibana images, or switch `Core/Logging/Elastic` to `OpenSearch.Client`. Then check end to end that a test run's logs and results show up in the cluster.

### 3. Elastic probed several times per test

- **Where:** `Core/Logging/LoggingServiceFactory.cs`, `Core/Logging/ResultsPublisherFactory.cs`, `Core/Logging/Elastic/ElasticLoggingService.cs`, `Core/Logging/Elastic/ElasticConnectivity.cs`; called from `UiTestBase`/`ApiTestBase`/`MobileTestBase` (constructor) and `TestRunner` (after each test).
- **Problem:** with `Provider: "elastic"`, every test instance probes the server three times: in `CreateLogger`, again in `ElasticLoggingService.Configure`, and again in `ResultsPublisherFactory.Create()`. Each probe has a 750 ms timeout, so an unreachable remote URL costs up to about 2 s per test.
- **Fix:** probe once per test run and cache the chosen logger/publisher per index (a `Lazy<>`, as the config providers do). Consider buffering result documents and sending them in bulk at the end of the run.

---

## P2: Should do

### 4. No Playwright trace on failure

- **Where:** `Tests/Infrastructure/UiTestBase.cs`, `Tests/Fixtures/PlaywrightFixture.cs`.
- **Problem:** a failing UI test only attaches a screenshot. CI failures that don't reproduce locally are hard to diagnose from that alone.
- **Fix:** start `Context.Tracing` for each test (screenshots, snapshots, sources). On failure, stop it into a `trace.zip` and attach that to Allure; on success, discard it. Optionally make this configurable (`Trace: off | on-failure | on`). Already on the ARCHITECTURE roadmap.

### 5. API tests leave bookings behind

- **Where:** `Tests/Api/BookingApi{Post,Get,Patch,Update}Tests.cs`, `Tests/Infrastructure/ApiTestBase.cs`.
- **Problem:** each of these tests creates a booking on the shared Restful Booker demo and never deletes it.
- **Fix:** give `ApiTestBase` a helper (e.g. `CreateBookingAsync`) that records the new ID, and delete the recorded bookings in `DisposeAsync`. Cleanup failures should be logged, never fail the test.

### 6. Three hand-written config providers, few env overrides

- **Where:** `Core/Configuration/ConfigurationProvider.cs`, `Core/Logging/LoggingConfigProvider.cs`, `Tests/UI/Mobile/Configuration/MobileSettingsProvider.cs`.
- **Problem:** the same load-JSON-then-cache code is written three times, and only `BROWSER`/`HEADLESS` can be overridden from the environment. Credentials and machine-specific values (Appium URL, device name) can only be changed by editing committed files.
- **Fix:** use `Microsoft.Extensions.Configuration` (JSON + environment variables + user-secrets) and bind it to the existing records. Any value then becomes overridable, e.g. `RestfulBooker__Password`, `Elastic__Password`, `Server__Url`. Keep `BROWSER`/`HEADLESS` as aliases. This also helps with items 1 and 3.

### 7. No `Directory.Build.props`; project settings have drifted

- **Where:** `Core/Core.csproj`, `Pages/Pages.csproj`, `Tests/Tests.csproj`, `Tests/UI/Mobile/Tests.UI.Mobile.csproj`, `Tests/Desktop/Tests.Desktop.csproj`.
- **Problem:** each project repeats `TargetFramework`/`Nullable`/`ImplicitUsings`, and they've drifted apart: only Core and Mobile set `LangVersion`, and Pages has no `IsPackable`. The `.editorconfig` rules are only checked when someone runs `dotnet format`.
- **Fix:** move the shared properties into a root `Directory.Build.props` and add `EnforceCodeStyleInBuild=true`. Set `TreatWarningsAsErrors=true` for CI builds. First fix the two existing nullable warnings in `Core/Logging/Elastic/ElasticClientFactory.cs` (lines 51 and 69).

---

## P3: Good practice

### 8. Page methods don't return the next page

- **Where:** `Pages/SauceDemo/*`, `Pages/Todo/TodoPage.cs`, and the tests that use them.
- **Problem:** tests write `await inventory.OpenCartAsync(); var cart = new CartPage(Page);`. Tests have to know which page comes next, and nothing stops them creating a page the browser isn't on.
- **Fix:** have navigation methods return the next page object, e.g. `Task<CartPage> OpenCartAsync()`, `Task<CheckoutInformationPage> CheckoutAsync()`, `Task<InventoryPage> LoginAsync(...)`.

### 9. CSS-class locators

- **Where:** `Pages/SauceDemo/*` (`.title`, `.inventory_item`, `.cart_item`, …), `Pages/Todo/TodoPage.cs`.
- **Problem:** styling classes change more often than behaviour, so these locators break easily. SauceDemo exposes `data-test` attributes.
- **Fix:** prefer `GetByRole` / `GetByLabel` / `GetByTestId`, with `data-test` configured as the test-id attribute (`Playwright.Selectors.SetTestIdAttribute("data-test")`). Fall back to CSS only when there's no alternative.

### 10. Mixed test method naming

- **Where:** all test classes, e.g. `CanAddBackpackToCart`, `Create_New_Booking_Should_Succeed`, `UserCanCompleteTodoItem`, `Saucedemo_Login_ShouldShowInventory`.
- **Problem:** three naming styles across the suites.
- **Fix:** pick one, e.g. `Subject_Should_Outcome[_When_Condition]`, rename every test, and add the rule to `docs/CONTRIBUTING.md`. Note: renaming resets each test's Allure history (its ID is derived from the full name), so do it in a single change.

### 11. Restful Booker auth inside the generic `ApiClient`

- **Where:** `Core/Api/ApiClient.cs` (`SendRawAsync` sets `Cookie: token=…`), `Core/Api/RestfulBooker/AuthClient.cs`.
- **Problem:** the generic client knows Restful Booker's token-cookie scheme, so a second API that authenticates differently (e.g. a Bearer header) would need changes to the shared client.
- **Fix:** have `ApiClient` accept per-request headers (or an auth strategy). Let the Restful Booker code turn its token into the cookie header, e.g. a `RestfulBookerAuth.Cookie(token)` helper next to `AuthClient`.

### 12. Logging index names differ by suite

- **Where:** `Tests/Infrastructure/ApiTestBase.cs` (`test-logs-…`), `Tests/Infrastructure/UiTestBase.cs` (`ui-web-logs-…`), `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs` (`ui-mobile-logs-…`).
- **Problem:** the API index name doesn't follow the `<project>-logs-` pattern the others use.
- **Fix:** rename it to `api-logs-…`. It was left unchanged on purpose, because existing Elastic dashboards and index patterns use the current names. Update those at the same time.

### 13. CI only runs Chrome

- **Where:** `azure-pipelines.yml`.
- **Problem:** the framework supports Chromium, Firefox and WebKit, but CI only ever runs `BROWSER=chrome`, so browser-specific breakage goes unnoticed.
- **Fix:** add a scheduled nightly pipeline (or stage) that runs `scripts/unix/run_ui_web_all_browsers.sh` with Playwright's browsers installed. Keep PR runs on Chrome only, so they stay fast.

### 14. Allure published as raw results only

- **Where:** `azure-pipelines.yml` ("Collect Allure Results" / "Upload Allure Results").
- **Problem:** to see the report you have to download the artifact and run `allure serve`, and there's no history to show trends.
- **Fix:** generate the HTML report in the pipeline (`allure generate`) and publish it, e.g. as an artifact or with an Allure report extension. Carry the `history/` folder over from the previous run to get trend charts.

### 15. `Tests/Desktop` is an empty project

- **Where:** `Tests/Desktop/Tests.Desktop.csproj`, `TestAutomationPlatform.slnx`.
- **Problem:** the project is in the solution but has no code, which suggests a capability the framework doesn't have.
- **Fix:** remove it until there's a plan, or add a first FlaUI (UI Automation) test with the same structure as the other suites (Configuration/, Driver/, Pages/<App>/, Infrastructure/DesktopTestBase, <App>/ tests). Note that desktop testing is Windows-only.

### 16. Mobile suite not in CI

- **Where:** `azure-pipelines.yml`, `Tests/UI/Mobile`.
- **Problem:** hosted agents have no Android emulator, so the Appium suite never runs automatically and can quietly break.
- **Fix:** add a self-hosted agent with an emulator, or use a device cloud (BrowserStack / Sauce Labs) by pointing `Server.Url` at the provider. In the meantime, at least build `Tests.UI.Mobile.csproj` in CI (add it to `ci.slnf`) so compile errors are caught.

---

## P4: Nice to have

### 17. `Tests/UI/Mobile/.npmrc` points at a private registry

- **Where:** `Tests/UI/Mobile/.npmrc`.
- **Problem:** it sets a company-internal npm registry with `strict-ssl=false`. Outside that network `npm install` fails, and turning off SSL checking is risky in any case.
- **Fix:** remove it from the repo, and configure the registry in the user's own `~/.npmrc` where it's needed.

### 18. No unit tests for the framework itself

- **Where:** `Core/`.
- **Problem:** framework logic (`BookingPatch.ApplyTo`, the builders, config providers, `ApiClient` error handling, `TestRunner`) is only tested indirectly, through the end-to-end suites.
- **Fix:** add a small `Core.Tests` project with fast unit tests (no browser, no network) and run it first in CI.

### 19. `ITestDataLoader` has one implementation

- **Where:** `Core/TestData/ITestDataLoader.cs`, `Core/TestData/TestDataLoader.cs`.
- **Problem:** the interface has a single implementation, and the class is only used through its static `Instance` singleton. Since JSON test data is now the exception, nothing currently calls it.
- **Fix:** drop the interface (and possibly the singleton). Bring back an abstraction if a second loader (e.g. CSV, or loading from a database) is ever needed.
