# Framework Review

Findings from the 2026-11-14 review, done alongside the consistency pass (see CHANGELOG). Each item names the affected files, what's wrong and a suggested fix. Priority: **P1** fix soon · **P2** should do · **P3** good practice · **P4** nice to have.

**How this list is maintained**

- IDs (`R1`, `R2`, …) never change, so re-sorting doesn't renumber anything. New findings get the next free ID.
- When an item is fixed, set **Status** to `Closed` and fill in **Closed in** with the date and the commit (or PR) that fixed it.
- **Sort order:** open items first, highest priority on top. Then closed items, most recently closed first. Items closed in the same commit: highest priority first. Ties within a priority: by ID.
- The table below is the sorted view. The detail sections after it stay grouped by priority, so links don't move.

| ID | Priority | Status | Closed in | Item | Area |
|---|---|---|---|---|---|
| R12 | P3 | Open | – | [Logging index names differ by suite](#r12) | Logging |
| R13 | P3 | Open | – | [CI only runs Chrome](#r13) | CI |
| R14 | P3 | Open | – | [Allure published as raw results only](#r14) | CI / reporting |
| R16 | P3 | Open | – | [Mobile suite not in CI](#r16) | CI |
| R18 | P4 | Open | – | [No unit tests for the framework itself](#r18) | Quality |
| R10 | P3 | Closed | 2026-11-14 · `0419662` | [Mixed test method naming](#r10) | Conventions |
| R8 | P3 | Closed | 2026-11-14 · `ad1a51e` | [Page methods don't return the next page](#r8) | Page objects |
| R9 | P3 | Closed | 2026-11-14 · `beadaa5` | [CSS-class locators](#r9) | Page objects |
| R20 | P3 | Closed | 2026-11-14 · `95440c4` | [No retry for transient navigation errors](#r20) | Reliability |
| R4 | P2 | Closed | 2026-11-14 · `e053af8` | [No Playwright trace on failure](#r4) | Debugging |
| R5 | P2 | Closed | 2026-11-14 · `c523e7b` | [API tests leave bookings behind](#r5) | Test hygiene |
| R11 | P3 | Closed | 2026-11-14 · `064182c` | [Restful Booker auth inside the generic `ApiClient`](#r11) | API layer |
| R6 | P2 | Closed | 2026-11-14 · `9ec39c5` | [Three hand-written config providers, few env overrides](#r6) | Configuration |
| R7 | P2 | Closed | 2026-11-14 · `4ab0cef` | [No `Directory.Build.props`; project settings have drifted](#r7) | Build |
| R19 | P4 | Closed | 2026-11-14 · `aa1079f` | [`ITestDataLoader` has one implementation](#r19) | Simplification |
| R17 | P4 | Closed | 2026-11-14 · `e4691ba` | [`Tests/UI/Mobile/.npmrc` points at a private registry](#r17) | Tooling |
| R15 | P3 | Closed | 2026-11-14 · `7cd6f22` | [`Tests/Desktop` is an empty project](#r15) | Structure |
| R1 | P1 | Closed | 2026-11-14 · `676a882` | [Elastic password committed in `logging-config.json`](#r1) | Security |
| R2 | P1 | Closed | 2026-11-14 · `676a882` | [Local logging stack (OpenSearch) vs Elasticsearch client](#r2) | Logging |
| R3 | P1 | Closed | 2026-11-14 · `676a882` | [Elastic probed several times per test](#r3) | Logging / speed |

---

## P1: Fix soon

<a id="r1"></a>
### R1. Elastic password committed in `logging-config.json`

- **Status:** Closed, 2026-11-14 · `676a882`. Credentials are now read from `ELASTIC_USERNAME` / `ELASTIC_PASSWORD` only, and the HTTPS profile uses its own `CertificateFingerprint` setting (it used to pass the password as the fingerprint). **Still to do by the owner:** the old password is in the public git history since `ec92ad9`, so rotate it on whatever cluster used it.
- **Where:** `logging-config.json` (`Elastic.Password`).
- **Problem:** the file contains what looks like a real Elasticsearch password, and the README itself says not to commit credentials. It stays in the git history even after it's removed.
- **Fix:** if the password was ever real, rotate it. Replace it with a placeholder and read the real value from an environment variable (e.g. `ELASTIC_PASSWORD`; see R6). Optionally add a secret scanner (e.g. gitleaks) to CI.

<a id="r2"></a>
### R2. Local logging stack (OpenSearch) vs Elasticsearch client

- **Status:** Closed, 2026-11-14 · `676a882`. Verified: OpenSearch 2.17 answers every write from the 8.12 client with HTTP 406 (`Content-Type … compatible-with=8 is not supported`) while the probe reports it as up. `docker-compose.yml` now runs Elasticsearch 8.15.3 + Kibana; a full run indexed 15 result documents and 2 log entries per test.
- **Where:** `docker-compose.yml` (OpenSearch images), `Directory.Packages.props` (`Elastic.Clients.Elasticsearch` 8.12.0), `Core/Logging/Elastic/*`.
- **Problem:** `docker compose up` starts **OpenSearch**, but `Core` uses Elastic's 8.x .NET client. That client checks that the server identifies itself as Elasticsearch, so it will very likely reject OpenSearch, and the logs silently go to the console fallback instead.
- **Fix:** pick one product. Either switch `docker-compose.yml` to the Elasticsearch + Kibana images, or switch `Core/Logging/Elastic` to `OpenSearch.Client`. Then check end to end that a test run's logs and results show up in the cluster.

<a id="r3"></a>
### R3. Elastic probed several times per test

- **Status:** Closed, 2026-11-14 · `676a882`. The probe is cached per URL, with one logger per index and one publisher per run. API suite with an unreachable Elastic URL: 7.7 s → ~4 s wall-clock.
- **Where:** `Core/Logging/LoggingServiceFactory.cs`, `Core/Logging/ResultsPublisherFactory.cs`, `Core/Logging/Elastic/ElasticLoggingService.cs`, `Core/Logging/Elastic/ElasticConnectivity.cs`; called from `UiTestBase`/`ApiTestBase`/`MobileTestBase` (constructor) and `TestRunner` (after each test).
- **Problem:** with `Provider: "elastic"`, every test instance probes the server three times: in `CreateLogger`, again in `ElasticLoggingService.Configure`, and again in `ResultsPublisherFactory.Create()`. Each probe has a 750 ms timeout, so an unreachable remote URL costs up to about 2 s per test.
- **Fix:** probe once per test run and cache the chosen logger/publisher per index (a `Lazy<>`, as the config providers do). Consider buffering result documents and sending them in bulk at the end of the run.

---

## P2: Should do

<a id="r4"></a>
### R4. No Playwright trace on failure

- **Status:** Closed, 2026-11-14 · `e053af8`. New Trace setting (off | on-failure | on; alias TRACE). Checked with a temporarily failing test: screenshot + 76 KB trace attached as application/vnd.allure.playwright-trace; passing tests have no trace; TRACE=on attaches one to every test.
- **Where:** `Tests/Infrastructure/UiTestBase.cs`, `Tests/Fixtures/PlaywrightFixture.cs`.
- **Problem:** a failing UI test only attaches a screenshot. CI failures that don't reproduce locally are hard to diagnose from that alone.
- **Fix:** start `Context.Tracing` for each test (screenshots, snapshots, sources). On failure, stop it into a `trace.zip` and attach that to Allure; on success, discard it. Optionally make this configurable (`Trace: off | on-failure | on`). Already on the ARCHITECTURE roadmap.

<a id="r5"></a>
### R5. API tests leave bookings behind

- **Status:** Closed, 2026-11-14 · `c523e7b`. ApiTestBase.CreateBookingAsync records each booking and DisposeAsync deletes them. Checked: a run created 5 bookings; 4 cleaned up with 201, 1 already deleted by the delete test (405).
- **Where:** `Tests/Api/BookingApi{Post,Get,Patch,Update}Tests.cs`, `Tests/Infrastructure/ApiTestBase.cs`.
- **Problem:** each of these tests creates a booking on the shared Restful Booker demo and never deletes it.
- **Fix:** give `ApiTestBase` a helper (e.g. `CreateBookingAsync`) that records the new ID, and delete the recorded bookings in `DisposeAsync`. Cleanup failures should be logged, never fail the test.

<a id="r6"></a>
### R6. Three hand-written config providers, few env overrides

- **Status:** Closed, 2026-11-14 · `9ec39c5`. SettingsLoader (Microsoft.Extensions.Configuration): JSON, then TESTS_/LOGGING_/MOBILE_ environment variables (__ for nesting), then aliases (BROWSER, HEADLESS, TRACE, ELASTIC_*, APPIUM_URL, DEVICE_NAME). Checked: TESTS_RestfulBooker__Password=wrong breaks exactly the 3 auth tests; TESTS_SauceDemo__BaseUrl redirects the UI tests.
- **Where:** `Core/Configuration/ConfigurationProvider.cs`, `Core/Logging/LoggingConfigProvider.cs`, `Tests/UI/Mobile/Configuration/MobileSettingsProvider.cs`.
- **Problem:** the same load-JSON-then-cache code is written three times, and only `BROWSER`/`HEADLESS` can be overridden from the environment. Credentials and machine-specific values (Appium URL, device name) can only be changed by editing committed files.
- **Fix:** use `Microsoft.Extensions.Configuration` (JSON + environment variables + user-secrets) and bind it to the existing records. Any value then becomes overridable, e.g. `RestfulBooker__Password`, `Elastic__Password`, `Server__Url`. Keep `BROWSER`/`HEADLESS` as aliases. This also helps with R1 and R3.

<a id="r7"></a>
### R7. No `Directory.Build.props`; project settings have drifted

- **Status:** Closed, 2026-11-14 · `4ab0cef`. Directory.Build.props holds the shared settings; EnforceCodeStyleInBuild reports .editorconfig violations in the build; TreatWarningsAsErrors when TF_BUILD/CI is set. Fixed the remaining warnings (MobileDriverFactory nullable flow, PlaywrightDriverFactory param docs). Checked: a file with a block namespace and an unused using fails a CI-mode build.
- **Where:** `Core/Core.csproj`, `Pages/Pages.csproj`, `Tests/Tests.csproj`, `Tests/UI/Mobile/Tests.UI.Mobile.csproj`, `Tests/Desktop/Tests.Desktop.csproj`.
- **Problem:** each project repeats `TargetFramework`/`Nullable`/`ImplicitUsings`, and they've drifted apart: only Core and Mobile set `LangVersion`, and Pages has no `IsPackable`. The `.editorconfig` rules are only checked when someone runs `dotnet format`.
- **Fix:** move the shared properties into a root `Directory.Build.props` and add `EnforceCodeStyleInBuild=true`. Set `TreatWarningsAsErrors=true` for CI builds. First fix the existing nullable warnings in `Tests/UI/Mobile/Driver/MobileDriverFactory.cs` (lines 36, 40, 45; the two in `ElasticClientFactory.cs` were fixed with R1).

---

## P3: Good practice

<a id="r8"></a>
### R8. Page methods don't return the next page

- **Status:** Closed, 2026-11-14 · `ad1a51e`. Navigation methods return the next page (LoginAsync → InventoryPage, OpenCartAsync → CartPage, CheckoutAsync → CheckoutInformationPage, FillAndContinueAsync → CheckoutOverviewPage, FinishAsync → CheckoutCompletePage); a rejected login uses LoginExpectingErrorAsync. Tests only create entry pages.
- **Where:** `Pages/SauceDemo/*`, `Pages/Todo/TodoPage.cs`, and the tests that use them.
- **Problem:** tests write `await inventory.OpenCartAsync(); var cart = new CartPage(Page);`. Tests have to know which page comes next, and nothing stops them creating a page the browser isn't on.
- **Fix:** have navigation methods return the next page object, e.g. `Task<CartPage> OpenCartAsync()`, `Task<CheckoutInformationPage> CheckoutAsync()`, `Task<InventoryPage> LoginAsync(...)`.

<a id="r9"></a>
### R9. CSS-class locators

- **Status:** Closed, 2026-11-14 · `beadaa5`. BasePage.TestId(id) with a per-app attribute (SauceDemoPage: data-test; default data-testid), plus GetByRole / GetByPlaceholder. No CSS class or #id selectors remain in Pages/. All 8 UI tests pass.
- **Where:** `Pages/SauceDemo/*` (`.title`, `.inventory_item`, `.cart_item`, …), `Pages/Todo/TodoPage.cs`.
- **Problem:** styling classes change more often than behaviour, so these locators break easily. SauceDemo exposes `data-test` attributes.
- **Fix:** prefer `GetByRole` / `GetByLabel` / `GetByTestId`, with `data-test` configured as the test-id attribute (`Playwright.Selectors.SetTestIdAttribute("data-test")`). Fall back to CSS only when there's no alternative.

<a id="r10"></a>
### R10. Mixed test method naming

- **Status:** Closed, 2026-11-14 · `0419662`. All 16 tests follow Subject_Should_Outcome[_When_Condition] (e.g. Cart_Should_ContainProduct_When_ProductAdded); the rule is in CONTRIBUTING. Allure history restarts once for these tests.
- **Where:** all test classes, e.g. `CanAddBackpackToCart`, `Create_New_Booking_Should_Succeed`, `UserCanCompleteTodoItem`, `Saucedemo_Login_ShouldShowInventory`.
- **Problem:** three naming styles across the suites.
- **Fix:** pick one, e.g. `Subject_Should_Outcome[_When_Condition]`, rename every test, and add the rule to `docs/CONTRIBUTING.md`. Note: renaming resets each test's Allure history (its ID is derived from the full name), so do it in a single change.

<a id="r11"></a>
### R11. Restful Booker auth inside the generic `ApiClient`

- **Status:** Closed, 2026-11-14 · `064182c`. ApiClient methods take optional per-request headers; AuthClient.Headers / GetAuthHeadersAsync build Restful Booker's token cookie; ApiTestBase exposes GetAuthHeadersAsync().
- **Where:** `Core/Api/ApiClient.cs` (`SendRawAsync` sets `Cookie: token=…`), `Core/Api/RestfulBooker/AuthClient.cs`.
- **Problem:** the generic client knows Restful Booker's token-cookie scheme, so a second API that authenticates differently (e.g. a Bearer header) would need changes to the shared client.
- **Fix:** have `ApiClient` accept per-request headers (or an auth strategy). Let the Restful Booker code turn its token into the cookie header, e.g. a `RestfulBookerAuth.Cookie(token)` helper next to `AuthClient`.

<a id="r12"></a>
### R12. Logging index names differ by suite

- **Status:** Open
- **Where:** `Tests/Infrastructure/ApiTestBase.cs` (`test-logs-…`), `Tests/Infrastructure/UiTestBase.cs` (`ui-web-logs-…`), `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs` (`ui-mobile-logs-…`).
- **Problem:** the API index name doesn't follow the `<project>-logs-` pattern the others use.
- **Fix:** rename it to `api-logs-…`. It was left unchanged on purpose, because existing Elastic dashboards and index patterns use the current names. Update those at the same time.

<a id="r13"></a>
### R13. CI only runs Chrome

- **Status:** Open
- **Where:** `azure-pipelines.yml`.
- **Problem:** the framework supports Chromium, Firefox and WebKit, but CI only ever runs `BROWSER=chrome`, so browser-specific breakage goes unnoticed.
- **Fix:** add a scheduled nightly pipeline (or stage) that runs `scripts/unix/run_ui_web_all_browsers.sh` with Playwright's browsers installed. Keep PR runs on Chrome only, so they stay fast.

<a id="r14"></a>
### R14. Allure published as raw results only

- **Status:** Open
- **Where:** `azure-pipelines.yml` ("Collect Allure Results" / "Upload Allure Results").
- **Problem:** to see the report you have to download the artifact and run `allure serve`, and there's no history to show trends.
- **Fix:** generate the HTML report in the pipeline (`allure generate`) and publish it, e.g. as an artifact or with an Allure report extension. Carry the `history/` folder over from the previous run to get trend charts.

<a id="r15"></a>
### R15. `Tests/Desktop` is an empty project

- **Status:** Closed, 2026-11-14 · `7cd6f22`. Removed the empty project; the docs describe how a FlaUI suite would be added later (Windows only).
- **Where:** `Tests/Desktop/Tests.Desktop.csproj`, `TestAutomationPlatform.slnx`.
- **Problem:** the project is in the solution but has no code, which suggests a capability the framework doesn't have.
- **Fix:** remove it until there's a plan, or add a first FlaUI (UI Automation) test with the same structure as the other suites (Configuration/, Driver/, Pages/<App>/, Infrastructure/DesktopTestBase, <App>/ tests). Note that desktop testing is Windows-only.

<a id="r16"></a>
### R16. Mobile suite not in CI

- **Status:** Open
- **Where:** `azure-pipelines.yml`, `Tests/UI/Mobile`.
- **Problem:** hosted agents have no Android emulator, so the Appium suite never runs automatically and can quietly break.
- **Fix:** add a self-hosted agent with an emulator, or use a device cloud (BrowserStack / Sauce Labs) by pointing `Server.Url` at the provider. In the meantime, at least build `Tests.UI.Mobile.csproj` in CI (add it to `ci.slnf`) so compile errors are caught.

<a id="r20"></a>
### R20. No retry for transient navigation errors

- **Status:** Closed, 2026-11-14 · `95440c4`. BasePage.GotoAsync retries net::ERR_* navigation failures up to twice. Checked with Playwright routing: 2 simulated connection resets, then the page loads; 3 resets, then it fails with net::ERR_CONNECTION_RESET.
- **Where:** `Pages/SauceDemo/LoginPage.cs`, `Pages/Todo/TodoPage.cs` (`OpenAsync`), `Tests/Infrastructure/UiTestBase.cs`.
- **Problem:** on 2026-11-14 two UI tests failed once with `net::ERR_CONNECTION_RESET` while opening saucedemo.com and passed on the rerun. A momentary network blip at the demo site fails the whole test. (The failure reasons were read back from Elasticsearch, which shows the R2 fix working.)
- **Fix:** retry the initial navigation only (not the assertions), e.g. up to 2 extra attempts on network errors (`net::ERR_*`) with a short backoff, in a shared `BasePage.GotoAsync` helper. Avoid blanket test reruns, which hide real failures.

---

## P4: Nice to have

<a id="r17"></a>
### R17. `Tests/UI/Mobile/.npmrc` points at a private registry

- **Status:** Closed, 2026-11-14 · `e4691ba`. Removed. Configure a registry in your own ~/.npmrc if you need one.
- **Where:** `Tests/UI/Mobile/.npmrc`.
- **Problem:** it sets a company-internal npm registry with `strict-ssl=false`. Outside that network `npm install` fails, and turning off SSL checking is risky in any case.
- **Fix:** remove it from the repo, and configure the registry in the user's own `~/.npmrc` where it's needed.

<a id="r18"></a>
### R18. No unit tests for the framework itself

- **Status:** Open
- **Where:** `Core/`.
- **Problem:** framework logic (`BookingPatch.ApplyTo`, the builders, config providers, `ApiClient` error handling, `TestRunner`) is only tested indirectly, through the end-to-end suites.
- **Fix:** add a small `Core.Tests` project with fast unit tests (no browser, no network) and run it first in CI.

<a id="r19"></a>
### R19. `ITestDataLoader` has one implementation

- **Status:** Closed, 2026-11-14 · `aa1079f`. TestDataLoader is now a static class; the interface and singleton are gone.
- **Where:** `Core/TestData/ITestDataLoader.cs`, `Core/TestData/TestDataLoader.cs`.
- **Problem:** the interface has a single implementation, and the class is only used through its static `Instance` singleton. Since JSON test data is now the exception, nothing currently calls it.
- **Fix:** drop the interface (and possibly the singleton). Bring back an abstraction if a second loader (e.g. CSV, or loading from a database) is ever needed.
