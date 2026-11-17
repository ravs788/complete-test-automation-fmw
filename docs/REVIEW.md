# Framework Review

Findings from the 2026-11-14 consistency review (see CHANGELOG), external architecture feedback on the Core layer (2026-11-17, R34–R38) and on replaceable adapters plus the owner's plug-and-play goal (2026-11-17, R39–R48; see [Plug-and-play roadmap](#plug-and-play-roadmap)), CI investigations (R31–R33) and the 2026-11-16 project review using the code-review, test-review, coverage-review, and documentation-review skills. Each item names the affected files, what's wrong and a suggested fix. Priority: **P1** fix soon · **P2** should do · **P3** good practice · **P4** nice to have.

**How this list is maintained**

- IDs (`R1`, `R2`, …) never change, so re-sorting doesn't renumber anything. New findings get the next free ID.
- Use **Implemented (verification pending)** when the fix is present but runtime verification is outstanding. Once verified, set **Status** to `Closed` and fill in **Closed in** with the date and the commit (or PR) that fixed it. Do not invent a commit reference for working-tree changes.
- **Sort order:** unresolved items (Open or Implemented with verification pending) first, highest priority on top. Then closed items, most recently closed first. Items closed in the same commit: highest priority first. Ties within a priority: by ID.
- The table below is the sorted view. The detail sections after it stay grouped by priority, so links don't move.

| ID | Priority | Status | Closed in | Item | Area |
|---|---|---|---|---|---|
| R32 | P2 | Implemented (verification pending) | – | [Android emulator doesn't finish booting on the hosted Mac within 300 s](#r32) | CI / mobile |
| R33 | P2 | Implemented (verification pending) | – | [Appium's 20 s helper-app install timeout is too short for the hosted emulator](#r33) | CI / mobile |
| R16 | P3 | Implemented (CI verification pending) | – | [Mobile suite not in CI](#r16) | CI |
| R36 | P4 | Implemented (verification pending) | – | [No secret-provider abstraction (e.g. Key Vault) behind SettingsLoader](#r36) | Security / configuration |
| R46 | P3 | Closed | 2026-11-17 · `70afeff` | [No guardrail or contract tests that prove adapters are replaceable](#r46) | Architecture / quality |
| R48 | P4 | Closed | 2026-11-17 · `70afeff` | [xUnit-specific code isn't isolated in a thin host layer](#r48) | Architecture / test host |
| R47 | P4 | Closed | 2026-11-17 · `1d6fdec` | [Only one implementation exists for the UI contract (no second browser or device driver)](#r47) | Architecture / UI |
| R42 | P3 | Closed | 2026-11-17 · `8bd245b` | [The web UI layer is Playwright-specific end to end](#r42) | Architecture / web UI |
| R44 | P3 | Closed | 2026-11-17 · `8bd245b` | [Core references seven vendor packages; the target is none](#r44) | Architecture |
| R45 | P3 | Closed | 2026-11-17 · `c557c08` | [Polly types are part of Core's public resilience API](#r45) | Architecture / resilience |
| R41 | P3 | Closed | 2026-11-17 · `1de545d` | [Playwright types are part of the API client's public surface](#r41) | Architecture / API |
| R39 | P3 | Closed | 2026-11-17 · `be94933` | [Allure is called directly from the shared runner, the base classes and every test](#r39) | Architecture / reporting |
| R40 | P3 | Closed | 2026-11-17 · `659a228` | [Providers are chosen three different ways; there is no composition root or plugin model](#r40) | Architecture |
| R43 | P3 | Closed | 2026-11-17 · `0a3cd2a` | [No dependency inventory, contract list or provider guide](#r43) | Documentation / architecture |
| R37 | P4 | Closed | 2026-11-17 · `8a6dc6d` | [No data / infrastructure helpers (database, message queue) with cleanup tracking](#r37) | Test infrastructure |
| R38 | P4 | Closed | 2026-11-17 · `cf18aa0` | [Mobile driver bootstrapping can't be reused outside the mobile test project](#r38) | Architecture / mobile |
| R35 | P3 | Closed | 2026-11-17 · `1b8f9d3` | [No central retry / polling policy; retries are hand-written per class](#r35) | Resilience |
| R34 | P3 | Closed | 2026-11-17 · `d01fbf0` | [Core contains app-specific code, so it can't be shared as a domain-agnostic package](#r34) | Architecture |
| R31 | P2 | Closed | 2026-11-17 · `4304475` | [Allure history download never finds a report, so every build is "partially succeeded"](#r31) | CI / reporting |
| R25 | P2 | Closed | 2026-11-17 · `5cdb42c` | [Report scripts can succeed after a failed build](#r25) | Run scripts |
| R24 | P1 | Closed | 2026-11-16 · `5cdb42c` | [Elastic probe bypasses TLS verification](#r24) | Security |
| R26 | P2 | Closed | 2026-11-16 · `5cdb42c` | [Mobile setup disables proxies process-wide](#r26) | Mobile / networking |
| R27 | P2 | Closed | 2026-11-16 · `5cdb42c` | [Screenshot failure prevents trace capture](#r27) | Diagnostics |
| R28 | P2 | Closed | 2026-11-16 · `5cdb42c` | [API update tests do not verify persistence](#r28) | Tests / API |
| R29 | P2 | Closed | 2026-11-16 · `5cdb42c` | [Shared mobile settings pin a specific emulator](#r29) | Mobile / configuration |
| R30 | P3 | Closed | 2026-11-16 · `5cdb42c` | [Mobile troubleshooting omits explicit UDID selection](#r30) | Documentation |
| R23 | P4 | Closed | 2026-11-14 · `03cf510` | [Allure results pile up across local runs](#r23) | Reporting |
| R22 | P4 | Closed | 2026-11-14 · `e8ba96d` | [`Console.Enabled` in `logging-config.json` does nothing](#r22) | Logging / config |
| R21 | P3 | Closed | 2026-11-14 · `0f492d8` | [Elastic probe treats 4xx as "up", so a misconfigured cluster fails every write](#r21) | Logging |
| R13 | P3 | Closed | 2026-11-14 · `7a8e003` | [CI only runs Chrome](#r13) | CI |
| R14 | P3 | Closed | 2026-11-14 · `7a8e003` | [Allure published as raw results only](#r14) | CI / reporting |
| R18 | P4 | Closed | 2026-11-14 · `536694d` | [No unit tests for the framework itself](#r18) | Quality |
| R12 | P3 | Closed | 2026-11-14 · `5c0fa57` | [Logging index names differ by suite](#r12) | Logging |
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

## Current state — 2026-11-16

| State | Findings | Count |
|---|---|---|
| Implemented; Windows verification pending | R25 | 1 |
| Implemented; hosted CI verification pending | R16 | 1 |
| Closed after fixes and verification | R24, R26–R30 | 6 |
| Closed in earlier reviews | R1–R15, R17–R23 | 22 |

Fixes and hosted CI jobs are committed in `5cdb42c` on `fix/review-p1-p2`; no PR has been created. R25 passed on Unix; a scheduled/manual hosted Windows verification job is now configured and awaits its first execution. R16 now has a hosted macOS mobile job and awaits its first successful Azure execution; R30 is closed: the troubleshooting guide now explains explicit UDID selection and stale override cleanup.

**Agent-executed validation:** The user explicitly allowed shell and native .NET verification for this run. Framework unit tests passed: **37 passed, 0 failed, 0 skipped**, including the three TLS regression cases. Targeted checks passed for Unix build failure handling (R25), controlled local/remote Appium proxy routing without global mutations (R26), headed Chrome screenshot failure with valid trace attachment and original exception preservation (R27), and default/explicit UDID serialization (R29). The dedicated browser was closed in fixture cleanup. The earlier full build passed with 0 warnings/errors.

**Evidence:** [Verification summary](../artifacts/review-verification-2026-11-16/summary.json), [unit TRX](../artifacts/review-verification-2026-11-16/verification-unit.trx), and [R27 trace ZIP](../artifacts/review-verification-2026-11-16/r27-trace.zip). These are generated local artifacts in the ignored `artifacts/` directory, not committed files. The isolated harness and scenario JSON evidence are saved alongside them.

**Verification boundary:** Windows script execution was unavailable on this Mac. Proxy checks used controlled HTTP responders that implement Appium session/orientation/quit responses, not a live device cloud. UDID checks verified configuration and serialized capabilities, not two physical devices. API PUT/PATCH execution for R28 remains supported by the user's earlier all-tests-passed report; it was not repeated in this run. No measured coverage was collected.

**Harness setup:** The first native test attempt was blocked by sandbox socket restrictions; the authorized retry outside the sandbox passed. The first browser harness attempt had missing Allure metadata; after adding the required fullName, the rerun passed. Neither setup failure was a product assertion failure.

## Coverage state — 2026-11-16

A TRX report now records 37 passing framework tests, and targeted harness evidence is available above. No Cobertura, OpenCover, or LCOV data was collected, so measured line/branch coverage remains unavailable. Passing results apply only to the stated execution scope.

| Area | Current implementation / evidence | Remaining gap | Tracking |
|---|---|---|---|
| Mobile capability construction | Current configuration and actual Appium capabilities checked with empty and explicit UDID | Optional broader matrix for Android/iOS, web/native, and Firefox | R29 (closed) and follow-up test work |
| Failure artifacts | Headed Chrome check forced screenshot failure, validated the attached trace ZIP, and preserved the original error | No remaining verification for this finding | R27 (closed) |
| Elastic transport security | Probe now reuses client TLS policy; ElasticTlsTests adds three cases for cloud rejection, matching fingerprint, and mismatched fingerprint | Agent ran all three TLS regression cases as part of 37 passing unit tests; loopback bypass boundaries remain optional additional coverage | R24 (closed) |
| API updates | PUT/PATCH tests now GET and compare the persisted record after writing | User reports all tests passed; an echo-only mutation check remains optional additional coverage | R28 (closed) |
| Mobile CI | Hosted macOS Android smoke job configured for scheduled/manual runs; Linux compilation retained | First actual Azure execution, smoke result discovery and artifact publication | R16 (CI verification pending) |

**Original review checks:** Source/configuration tracing and 14 local documentation file-link checks across six Markdown files (none missing). External links and anchors in those documentation files were not checked. Per-finding verification steps below remain outstanding unless marked completed or supported by the user-reported test run above.

---

## Plug-and-play roadmap

Goal (owner, 2026-11-17): every external library replaceable, plug and play. Direction: **`Core` holds contracts and the engine with no vendor packages; each library sits in its own adapter project under `Core/<Area>/<Vendor>/`; one composition root picks adapters from a `Providers` configuration section; every contract has a second implementation and a shared contract test suite.** xUnit stays the host and `Microsoft.Extensions.*` are accepted platform dependencies (R43, R48). Each phase keeps all tests green and ships as its own PR.

| Phase | Work | Items | Proves |
|---|---|---|---|
| 0 | Dependency inventory, contract list, `docs/PLUGIN_ARCHITECTURE.md` | [R43](#r43) | the scope and rules |
| 1 | Composition root + plugin model; logging and secrets moved onto it; first guardrail tests | [R40](#r40), [R44](#r44), [R46](#r46) | console ↔ Elastic by config only |
| 2 | Reporting contract; Allure and Console reporter adapters | [R39](#r39) | report unchanged; runs without Allure |
| 3 | HTTP transport contract; Playwright and `HttpClient` adapters | [R41](#r41) | API tests without Playwright |
| 4 | Resilience contract (Polly adapter + built-in retry); settings source contract | [R45](#r45), [R44](#r44) | `Core` has no vendor packages |
| 5 | UI contract with capability interfaces and framework assertions; Playwright adapter; one slice, then all web tests | [R42](#r42) | `Pages` and web tests free of Playwright |
| 6 | Selenium web adapter; Appium on the same UI contract | [R47](#r47) | same tests on another driver |
| 7 | xUnit confined to a thin host layer | [R48](#r48) | a later runner move is contained |

---

## P1: Fix soon

<a id="r24"></a>
### R24. Elastic probe bypasses TLS verification

- **Status:** Closed, 2026-11-16, based on the user-reported passing test run. Fix committed in `5cdb42c` on `fix/review-p1-p2`.
- **Implementation:** The probe now uses ElasticClientFactory and the actual Elastic transport for TLS and authentication. Provider callers pass their profile and fingerprint, and the probe cache separates connection/security configurations. The no-fingerprint development bypass is restricted to loopback URLs. Added ElasticTlsTests for accepted and rejected certificates and whether credentials reach the HTTP server; compiled and passed in the subsequent agent-executed unit test run.
- **Where:** Core/Logging/Elastic/ElasticConnectivity.cs:85 (`Probe`), and the logging/results provider factories.
- **Problem:** The probe accepts any server certificate and sends Basic authentication credentials when configured. Unlike the actual Elastic client, the probe does not apply the selected profile or certificate fingerprint. For a remote HTTPS endpoint, an intercepted connection could expose credentials before the client validates the server certificate.
- **Fix:** Use the same certificate validation policy for the probe and client, including fingerprint validation when configured. Restrict any explicitly enabled development bypass to its intended profile.
- **Verification:** Agent ran `dotnet test Tests/Unit/Tests.Unit.csproj --no-restore --logger "trx;LogFileName=verification-unit.trx"`: 37 passed, 0 failed, 0 skipped, including all three ElasticTlsTests cases. See the saved TRX above.

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

<a id="r25"></a>
### R25. Report scripts can succeed after a failed build

- **Status:** Closed, 2026-11-17 · `5cdb42c`. Verified on the hosted `windows-2025` agent in run 108 (nightly 2026-11-17): `PASS R25: build exit 37 preserved; stale binary present; test command never invoked`.
- **Implementation:** Both shared helpers now return the failed build exit code before invoking dotnet test --no-build. Unix syntax and runtime failure-path checks passed. The WindowsHelperVerification job now runs the isolated PowerShell verification on a hosted windows-2025 agent for scheduled/manual builds; its first execution remains pending.
- **Where:** scripts/unix/_run_playwright_tests.sh:36 and scripts/windows/_run_playwright_tests.bat:30.
- **Problem:** Both helpers record the build exit code but continue with dotnet test --no-build and return the test exit code. If previously compiled tests are available, they can pass even when the current source fails to build, producing a misleading successful run.
- **Fix:** Exit with the build failure before starting tests. Preserve failure status for required result-copy steps as appropriate.
- **Verification:** Unix check passed in an isolated temporary repository with a stale Tests.dll and a controlled dotnet command returning build exit 37: helper returned 37 and never invoked the test command. Windows execution remains pending on the hosted agent. No developer-owned Windows machine is required; the pipeline publishes JSON evidence and helper output as windows-helper-verification.

<a id="r26"></a>
### R26. Mobile setup disables proxies process-wide

- **Status:** Closed, 2026-11-16, after agent-executed targeted verification. Fix committed in `5cdb42c` on `fix/review-p1-p2`.
- **Implementation:** Appium now uses a per-session HttpCommandExecutor. Only loopback sessions receive a direct proxy; remote sessions retain default proxy behavior. No proxy environment variables or process-wide HTTP defaults are modified. Executor is disposed if session creation fails. Compiled and verified with controlled local/remote Appium responders.
- **Where:** Tests/UI/Mobile/Driver/MobileDriverFactory.cs:49 (`DisableProxyForLocalAppium`).
- **Problem:** Every session clears proxy environment variables and replaces global proxy defaults, regardless of Server.Url. A remote Appium server or logging endpoint that requires a corporate proxy can become unreachable; unrelated HTTP clients in the process also inherit the change.
- **Fix:** Scope proxy bypass to the local Appium connection. Preserve proxy configuration for remote Appium URLs and other clients.
- **Verification:** Passed using the actual MobileDriverFactory and two controlled HTTP responders: local session/orientation/quit requests reached Appium directly; remote requests for appium.verification.invalid went through the configured proxy. All proxy environment variables and both global HTTP proxy objects remained unchanged. See r26.json in the local evidence directory.

<a id="r27"></a>
### R27. Screenshot failure prevents trace capture

- **Status:** Closed, 2026-11-16, after agent-executed targeted verification. Fix committed in `5cdb42c` on `fix/review-p1-p2`.
- **Implementation:** Trace capture is attempted in a finally block around screenshot capture/attachment, so screenshot failure cannot skip trace capture. TestRunner still preserves the original test failure. Compiled and verified with an injected screenshot failure in headed Chrome.
- **Where:** Tests/Infrastructure/UiTestBase.cs:83 (`AttachFailureArtifactsAsync`).
- **Problem:** Screenshot capture and attachment happen before trace capture. If either throws, the failure hook exits before saving the trace. Under the default on-failure mode, AttachTraceIfAlwaysOnAsync does not retry, losing diagnostic evidence even when tracing remains usable. This is a distinct failure-path gap in the trace support added for R4; R4 remains closed.
- **Fix:** Attempt screenshot and trace capture independently while preserving the original test failure.
- **Verification:** Passed with a real headed Chrome context and an Allure test case. Closed the page before the deliberately failing test body, forcing screenshot capture to fail. A valid 8,244-byte trace ZIP with six entries was attached, the original exception object was preserved, and browser/context cleanup completed. See r27.json and r27-trace.zip.

<a id="r28"></a>
### R28. API update tests do not verify persistence

- **Status:** Closed, 2026-11-16, based on the user-reported passing test run. Fix committed in `5cdb42c` on `fix/review-p1-p2`.
- **Implementation:** PUT and PATCH tests now GET the created booking after the write and compare the persisted record against the expected update, including unchanged PATCH fields. Compiled; subsequent test run reported passing by the user.
- **Where:** Tests/Api/BookingApiUpdateTests.cs:29 and Tests/Api/BookingApiPatchTests.cs:29.
- **Problem:** The PUT and PATCH tests assert only the write-response body. An endpoint that echoes the expected payload without persisting the update would pass, despite the tests claiming that booking fields were replaced or patched.
- **Fix:** After each successful write, GET the created booking by ID and assert the persisted fields, including unchanged fields for PATCH.
- **Verification:** User reports all tests passed after these fixes, covering the added TLS cases and updated API persistence assertions. The agent did not rerun the suite; no command, counts, browser/device matrix, or test artifacts were supplied.

<a id="r29"></a>
### R29. Shared mobile settings pin a specific emulator

- **Status:** Closed, 2026-11-16, after agent-executed targeted verification. Fix committed in `5cdb42c` on `fix/review-p1-p2`.
- **Implementation:** Restored the shared Device.Udid default to an empty string, preserving local device selection through MOBILE_Device__Udid. This restores the tracked baseline for mobilesettings.json; the prior typed-property changes in MobileOptionsBuilder are preserved. Configuration binding and capability serialization verification passed.
- **Where:** Tests/UI/Mobile/mobilesettings.json:15; Tests/UI/Mobile/Configuration/MobileSettingsProvider.cs; Tests/UI/Mobile/Driver/MobileOptionsBuilder.cs:29.
- **Original problem (corrected; shared default confirmed at `5cdb42c`):** Device.Udid was set to emulator-5554. DEVICE_NAME overrides only Device.DeviceName, so selecting another emulator or physical device through that alias still leaves the explicit UDID targeting emulator-5554.
- **Fix:** Keep the shared UDID default empty and select a specific local device using MOBILE_Device__Udid.
- **Verification:** Passed against current mobilesettings.json and SettingsLoader environment binding: default capabilities omitted both udid/appium:udid; setting MOBILE_Device__Udid=verification-device-1234 produced that exact appium:udid value in actual AppiumOptions capabilities. Environment override was restored afterward. See r29.json.

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

<a id="r33"></a>
### R33. Appium's 20 s helper-app install timeout is too short for the hosted emulator

- **Status:** Implemented (verification pending), 2026-11-17, in `c1e23a8`. Configurable UiAutomator2 timeouts (CI: 120 s); the CI script waits for background compilation, turns off adb install verification and pre-installs Appium's 3 helper apps (timed in `boot-progress.log`). Unit tests for the capability mapping; the script block was checked locally under `set -euo pipefail` with a fake `adb`. **Verify:** a manual run whose smoke test passes. **Run 118 (manual, 2026-11-17):** the fix worked (helpers pre-installed in 19/34/9 s; Appium found them, `sameVersionInstalled`, and started UiAutomator2 in 30 s; boot took 446 s). The session then failed at the next step: Appium auto-downloaded Chromedriver for the device's Chrome 113 and was starting Chrome for the first time when the .NET client's 120 s command timeout (`Server.CommandTimeoutSec`) expired. **Next (parked by the owner on 2026-11-17):** raise `MOBILE_Server__CommandTimeoutSec` on CI and pre-download Chromedriver 113 (`Android.ChromedriverExecutable`). **Follow-up `41d50c8` (2026-11-17, owner asked to resume):** the CI script pre-downloads the Chromedriver matching the device's Chrome, pre-warms Chrome with first-run screens disabled, and raises the client timeout to 300 s (checked locally with a fake `adb` for Chrome 113/131/unknown). **Run 130 (manual, 2026-11-17, `6694a9b`):** boot 405 s, helpers pre-installed, Chromedriver 113.0.5672.63 pre-downloaded for Chrome 113.0.5672.136, warm-up 204 s. The session then failed with `chrome not reachable`: Chromedriver **cleared Chrome's app data** before launching it (its default unless `androidKeepAppData` is set), which threw away the warm-up; the cold Chrome start (from 10:49:46) wasn't answering after about 70 s and Chromedriver gave up. The emulator was also overloaded (35 ANRs, mostly Google services and `com.android.phone`). **Follow-up:** `goog:chromeOptions.androidKeepAppDataDir` follows `Behavior.NoReset` (default true), so the warmed-up Chrome is reused; unit-tested. PR #35 first used `androidKeepAppData`, which Chromedriver rejects (`unrecognized chrome option`), caught by a local run before CI; corrected on `fix/mobile-keep-chrome-data-main` (replaces PRs #35/#36). **Local check (2026-11-17, Apple Silicon, Appium 3.6 + UiAutomator2 8.7.3, Android 14 arm64 emulator, Chrome 113.0.5672.136):** emulator booted in 49 s, smoke test passed in 11 s, device log shows no Chrome data clear. **Verify:** a manual run where the smoke test passes; that closes R33, R32 and R16.
- **Where:** `Core/Mobile/Driver/MobileOptionsBuilder.cs`, `Core/Mobile/Configuration/AndroidSettings.cs`, `scripts/unix/run_mobile_ci.sh`.
- **Problem:** manual run 115 (2026-11-17) was the first where the emulator booted (505 s) and the smoke test ran, but session creation failed: `adb install … appium-uiautomator2-server-v7.7.1.apk timed out after 20000ms`. Appium installs two helper apps when the session starts: `io.appium.settings` took 16.1 s, and the UiAutomator2 server hit the 20 s default. Android was still compiling apps in the background after boot (355 `dex2oat` lines in logcat) on its single vCPU (see R32).
- **Fix:** make the UiAutomator2 timeouts configurable (`adbExecTimeout`, `uiautomator2ServerInstallTimeout`, `uiautomator2ServerLaunchTimeout`; normal defaults locally, about 120 s on CI through `MOBILE_Android__*`). In the CI script, wait for background compilation to settle after boot, turn off install verification, and pre-install Appium's helper apps with plain `adb install` before the session starts, so the session doesn't have to.

<a id="r31"></a>
### R31. Allure history download never finds a report, so every build is "partially succeeded"

- **Status:** Closed, 2026-11-17 · `4304475`. Verified: PR build 109 downloaded history from #108 and succeeded; push build 114 on `main` downloaded from #111 ("Using previous history") and **succeeded**, the first fully successful `main` push build since 99.
- **Where:** `azure-pipelines.yml` (`Download Previous Allure History`, `DownloadPipelineArtifact@2` with `buildVersionToDownload: latestFromBranch`).
- **Problem:** found while checking runs 107 and 108 (2026-11-17). `latestFromBranch` only looks at builds that *succeeded* (plus failed ones, because of `allowFailedBuilds`); *partially succeeded* builds need `allowPartiallySucceededBuilds: true`. The last fully successful `main` build is 99, which ran before the report step existed, so every `main` build since (102–108) downloads from build 99, warns `Artifact allure-report was not found for build 99`, and ends "partially succeeded", which keeps it out of the next build's search too. PR builds (`refs/pull/N/merge`) have no earlier builds at all, so they warn as well. Result: no Allure trends, and no build is ever fully green.
- **Fix:** set `allowPartiallySucceededBuilds: true` and take the history from `refs/heads/main` for every build (PRs and branches then compare against `main`).

<a id="r32"></a>
### R32. Android emulator doesn't finish booting on the hosted Mac within 300 s

- **Status:** Implemented (verification pending), 2026-11-17, in `be414c3` and a follow-up on `fix/emulator-boot-progress`. Run 110 (manual, 2026-11-17) did **not** verify it: (1) the new 30-second progress report exited the script silently under `set -e`/`pipefail` when `adb` failed while the device was still offline, so the boot never got its 600 s (fixed: those `adb` calls can no longer fail the script); (2) the emulator still set 1 vCPU (`less than 6 logical cores`) despite `-cores 2` and `hw.cpu.ncore=2`, so on hosted macOS the core count can't be raised (those settings were removed). All software prerequisites were present (.NET 10.0.404, Node 22.23.2, Temurin 17, Appium 2.19.0 + UiAutomator2 4.2.9, SDK/image/AVD, Hypervisor.Framework). **Verify:** the next manual run should show `[boot]` progress lines and either "Android booted in N seconds" or how far it got in 600 s. If 1 core isn't enough, next options: a lighter image (API 30/31 `google_apis`), a self-hosted agent with more cores, or a device cloud. **Runs 115/118:** booted in 505 s / 446 s. **Run 127 (manual, 2026-11-17, `6e774e7`):** device online at 154 s and boot animation done at 186 s, but `sys.boot_completed` never reached 1 within 600 s, so the R33 Chrome steps never ran. Follow-up on `fix/mobile-boot-timeout`: the pipeline sets `BOOT_TIMEOUT_SECONDS=900` and the job limit is 45 minutes. **Verify:** two manual runs that boot and pass the smoke test close R32, R33 and R16.
- **Where:** `scripts/unix/run_mobile_ci.sh` (emulator start and boot wait), `azure-pipelines.yml` (`MobileSmoke`).
- **Problem:** the first hosted run (run 108, nightly 2026-11-17) failed with `Emulator failed to boot within 300 seconds`. The `mobile-results` logs show it isn't a missing-acceleration problem: `acceleration.log` reports Hypervisor.Framework available on the Intel (`osx-x64`) `macos-15` agent, and the emulator ran for the whole 300 s. It was too slow: `emulator.log` says `Running on a system with less than 6 logical cores. Setting number of virtual cores to 1`, rendering is all on the CPU (`-gpu swiftshader`), the emulator window is still created (no `-no-window`), and every run is a cold boot of a heavy Android 14 `google_apis` image. Knock-on: `Publish Mobile Test Results` failed because no TRX was written. Also `NodeTool@0` is deprecated (`UseNode@1`).
- **Fix:** start headless with lighter rendering (`-no-window -gpu swiftshader_indirect`), give the guest 2 cores and more RAM (`-cores 2 -memory 3072`, and `hw.cpu.ncore=2` in the AVD config), allow a longer boot (600 s), log boot progress, and switch to `UseNode@1`. If that still isn't enough, use a lighter system image.

---

## P3: Good practice

<a id="r30"></a>
### R30. Mobile troubleshooting omits explicit UDID selection

- **Status:** Closed, 2026-11-16, committed in `5cdb42c` on `fix/review-p1-p2`.
- **Where:** `docs/GETTING_STARTED.md` (mobile troubleshooting), `docs/MOBILE_ARCHITECTURE.md` (device configuration).
- **Original problem:** Troubleshooting described DeviceName but did not explain independent UDID selection or how to clear stale overrides.
- **Implementation:** Added macOS/Linux and Windows PowerShell commands for selecting the serial from `adb devices -l`, checking device readiness, and clearing the environment override. Explained that removing an environment override restores the JSON value, so stale JSON UDIDs must also be cleared. Documented that DEVICE_NAME changes DeviceName only, and cross-referenced R29's empty shared default.
- **Verification:** Checked the instructions against SettingsLoader precedence, MobileSettingsProvider aliases, and MobileOptionsBuilder's nonempty-UDID capability handling. R29's agent-executed capability check already verified omission of the default UDID and exact serialization of a different device serial. Documentation file links and whitespace checks passed; no additional product test run was required.

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

- **Status:** Closed, 2026-11-14 · `5c0fa57`. API logs now go to api-logs-{date}, the same pattern as ui-web-logs / ui-mobile-logs. Checked against the compose stack (api-logs-* got 19 entries for 7 tests: start, finish, and 5 cleanup lines). **Owner action:** update any Kibana data view or dashboard that used test-logs-*.
- **Where:** `Tests/Infrastructure/ApiTestBase.cs` (`test-logs-…`), `Tests/Infrastructure/UiTestBase.cs` (`ui-web-logs-…`), `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs` (`ui-mobile-logs-…`).
- **Problem:** the API index name doesn't follow the `<project>-logs-` pattern the others use.
- **Fix:** rename it to `api-logs-…`. It was left unchanged on purpose, because existing Elastic dashboards and index patterns use the current names. Update those at the same time.

<a id="r13"></a>
### R13. CI only runs Chrome

- **Status:** Closed, 2026-11-14 · `7a8e003`. Nightly schedule (02:00 UTC, main, only when it changed) runs the web UI tests on Playwright's Chromium, Firefox and WebKit; PRs stay on Chrome. The loop was run locally (Chromium and WebKit pass; Firefox can't start on macOS, a known local issue). **To confirm:** the first nightly run in Azure DevOps.
- **Where:** `azure-pipelines.yml`.
- **Problem:** the framework supports Chromium, Firefox and WebKit, but CI only ever runs `BROWSER=chrome`, so browser-specific breakage goes unnoticed.
- **Fix:** add a scheduled nightly pipeline (or stage) that runs `scripts/unix/run_ui_web_all_browsers.sh` with Playwright's browsers installed. Keep PR runs on Chrome only, so they stay fast.

<a id="r14"></a>
### R14. Allure published as raw results only

- **Status:** Closed, 2026-11-14 · `7a8e003`. Every run publishes an allure-report artifact (Allure CLI 2.46.1) with trends from the previous report on the same branch, plus a single-file allure-report.html. Checked locally: two generations give 2 trend points. **To confirm:** the first pipeline run after merge.
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

- **Status:** Implemented (CI verification pending), 2026-11-16, committed in `5cdb42c` on `fix/review-p1-p2`; no hosted run has been executed by the agent.
- **Where:** `azure-pipelines.yml` (`MobileSmoke`) and `scripts/unix/run_mobile_ci.sh`.
- **Original problem:** CI compiled the mobile project but never executed its tests. Hosted Ubuntu lacks Android emulator hardware acceleration; hosted macOS supports this workflow, so self-hosting or a device cloud is not required for the initial smoke job.
- **Implementation:** A separate `macOS-15` job runs on Schedule/Manual builds. It installs pinned Node.js 22.23.2, Appium 2.19.0 and UiAutomator2 4.2.9, creates an Android 14 emulator, waits for Android and Appium with bounded deadlines, sets the device through `MOBILE_*`, and runs the mobile smoke project. TRX is checked for actual execution and all-passing results. Always-run publishing retains TRX, raw Allure and diagnostics in `mobile-results`; the helper traps exits to stop its owned processes. Linux PR/push checks retain mobile compilation.
- **Verification:** Local validation passed: YAML parsing, mobile job gating/publishing checks, and shell syntax. Four isolated helper scenarios (success, zero tests, test failure, build failure) passed with controlled tool doubles; cleanup stopped all owned processes and removed temporary work. Evidence: `artifacts/mobile-ci-validation/control-flow.json` (generated, ignored). This validates helper control flow, not a real emulator or Azure execution. An actual Azure hosted run remains pending. Close only after a scheduled/manual run executes the smoke test, reports success, publishes mobile results/diagnostics, and completes cleanup.
- **Reference:** [Microsoft's Android emulator guidance](https://learn.microsoft.com/en-us/azure/devops/pipelines/ecosystems/android?view=azure-devops#test-on-the-android-emulator).

<a id="r20"></a>
### R20. No retry for transient navigation errors

- **Status:** Closed, 2026-11-14 · `95440c4`. BasePage.GotoAsync retries net::ERR_* navigation failures up to twice. Checked with Playwright routing: 2 simulated connection resets, then the page loads; 3 resets, then it fails with net::ERR_CONNECTION_RESET.
- **Where:** `Pages/SauceDemo/LoginPage.cs`, `Pages/Todo/TodoPage.cs` (`OpenAsync`), `Tests/Infrastructure/UiTestBase.cs`.
- **Problem:** on 2026-11-14 two UI tests failed once with `net::ERR_CONNECTION_RESET` while opening saucedemo.com and passed on the rerun. A momentary network blip at the demo site fails the whole test. (The failure reasons were read back from Elasticsearch, which shows the R2 fix working.)
- **Fix:** retry the initial navigation only (not the assertions), e.g. up to 2 extra attempts on network errors (`net::ERR_*`) with a short backoff, in a shared `BasePage.GotoAsync` helper. Avoid blanket test reruns, which hide real failures.

<a id="r21"></a>
### R21. Elastic probe treats 4xx as "up", so a misconfigured cluster fails every write

- **Status:** Closed, 2026-11-14 · `0f492d8`. The probe requires a 2xx GET / with the configured credentials from Elasticsearch 8+ (not OpenSearch) and explains a failure in one message. If writes fail later, Elastic logging switches to the console once and results publishing stops after one failure. Checked against an unreachable URL, a fake OpenSearch, a wrong password on a secured ES 8.15 and a read-only fake ES (one message each, all tests pass), plus the right password (everything indexed). 10 new unit tests.
- **Where:** `Core/Logging/Elastic/ElasticConnectivity.cs` (`code >= 200 && code < 500`), `ElasticLoggingService.Write`, `ElasticResultsPublisher.Publish`.
- **Problem:** the reachability probe counts any 2xx–4xx answer as "reachable". With wrong credentials (401/403), or an incompatible server (the OpenSearch 406 found in R2), the `elastic` provider is chosen anyway. Then every log line prints an indexing error, every test logs a publish failure, and nothing reaches the console fallback. Seen during the R2 investigation.
- **Fix:** probe with the configured credentials and require 2xx (and, ideally, an Elasticsearch 8.x `version.number` from `GET /`); otherwise fall back to the console with one clear message. Also disable the Elastic logger after the first failed write instead of failing on every line.

<a id="r34"></a>
### R34. Core contains app-specific code, so it can't be shared as a domain-agnostic package

- **Status:** Closed, 2026-11-17 · `d01fbf0`. App-specific code moved to a new Domain project (Domain/Api/RestfulBooker, Domain/Models/<App>, Domain/TestData/<App>); Core keeps only generic code. Tests/Unit/Architecture/CoreBoundaryTests enforces the boundary (no references to Domain, Pages or test projects; no app-named namespaces). Unit 44/44, Playwright 15/15. Not done: the optional split of Core into a light core plus Core.Playwright / Core.Logging.Elastic add-ons (see R38 for the mobile part).
- **Source:** external architecture feedback, 2026-11-17 ("the golden rule of Core: zero references to Page Objects, UI locators or specific test suites").
- **Where:** `Core/Api/RestfulBooker/`, `Core/Models/{RestfulBooker,SauceDemo,Todo}/`, `Core/TestData/{RestfulBooker,SauceDemo,Todo}/`. Only `Tests` and `Tests/Unit` use them; `Pages` doesn't.
- **Problem:** Core mixes the reusable engine (configuration, driver/client factories, `TestRunner`, logging, the `TestDataLoader`) with models, Builders/Mothers and auth for three specific apps. Another product team couldn't consume Core (for example as an internal NuGet package) without inheriting SauceDemo/Restful Booker/TodoMVC code. Core also references Playwright and the Elasticsearch client, so even a mobile-only or API-only consumer would get both.
- **Fix:** move the app-specific folders to a separate project (e.g. `Domain/<App>/{Api,Models,TestData}` or per-app client libraries) referenced by `Tests` and `Tests/Unit`, keeping `<Layer>/<App>/` inside it. Optionally split Core further into a dependency-light core (configuration, `TestRunner`, logging abstractions, `TestDataLoader`) plus add-ons (`Core.Playwright`, `Core.Logging.Elastic`), so each consumer only takes what it needs. Add an architecture test (e.g. NetArchTest) that fails if Core references app-specific namespaces.

<a id="r35"></a>
### R35. No central retry / polling policy; retries are hand-written per class

- **Status:** Closed, 2026-11-17 · `1b8f9d3`. Core/Resilience (Polly.Core 8.8.1): RetryPolicies.TransientHttp (idempotent methods only in ApiClient), Navigation, Transient, and Poll.UntilAsync; the three hand-written loops now use them; retry counts in Resilience settings. Checked with 18 unit tests and against real Playwright (GET retried twice on 503 then 200, POST not retried; navigation recovered from 2 routed resets); Playwright suite 15/15.
- **Source:** external architecture feedback, 2026-11-17.
- **Where:** `Pages/Common/BasePage.cs` (`GotoAsync` retry loop, R20), `Tests/UI/Mobile/Pages/Common/MobileBasePage.cs` (two `while (attempts++ < 3)` loops), `Core/Api/ApiClient.cs` (no retry).
- **Problem:** each retry is hand-written with its own attempts, backoff and exception filter, and `ApiClient` doesn't retry transient `502/503/504` responses or network errors from slow-starting test environments. There's no shared "poll until a condition holds" helper for eventually consistent backends, so future tests are likely to add ad-hoc loops or sleeps. (Playwright's auto-waiting already covers most UI waits.)
- **Fix:** add `Core/Resilience/` with named policies built on Polly (`Microsoft.Extensions.Resilience` / Polly v8): transient-HTTP retry with jittered backoff for **idempotent** requests only (never blindly retry a POST that creates data), navigation retry, and a `PollAsync(condition, timeout, interval)` helper. Use them in `ApiClient`, `BasePage.GotoAsync` and `MobileBasePage`. Retry counts belong in settings, and assertions are never retried.

<a id="r39"></a>
### R39. Allure is called directly from the shared runner, the base classes and every test

- **Status:** Closed, 2026-11-17 · `be94933`. Core/Reporting (ITestReporter, Report.Step, built-in console reporter) selected with "Reporting" in providers.json; Allure moved into the Core.Reporting.Allure adapter (Core no longer references Allure.Net.Commons); TestRunner, UiTestBase, MobileTestBase and the 13 test classes call the contract. Verified: Allure results of the 15 Playwright tests and the Android smoke identical to a pre-change baseline (names, status, labels, parameters, steps, attachments; ids and times ignored); TRACE=on still attaches traces; PROVIDERS_Reporting=console passes 15/15 with steps printed; AdapterBoundaryTests keeps Allure out of Core. Not done (kept per the plan): class labels are still Allure.Xunit attributes, and Allure.Xunit (the xUnit hook) still writes a basic outcome file per test, see R48.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 2.**
- **Where:** `Core/Testing/TestRunner.cs` (Allure timing parameters), `Core/Testing/AllureResults.cs`, `Tests/Infrastructure/UiTestBase.cs` (test name, browser parameter, screenshot and trace attachments), `Tests/UI/Mobile/Infrastructure/MobileTestBase.cs`, and all 13 test classes (`AllureApi.Step`, about 30 calls, plus `[AllureParentSuite]`/`[AllureSuite]`/… attributes).
- **Problem:** reporting has no contract of its own. `Core` references `Allure.Net.Commons` and test code calls Allure's static API, so swapping or adding a reporter (ReportPortal, plain HTML/JSON, none) touches the runner, every base class and every test.
- **Fix:** `Core/Reporting/ITestReporter` (start/finish test, name and parameters, `StepAsync(name, body)`, `Attach(name, contentType, bytes)`, labels) with adapters `Core/Reporting/Allure` and `Core/Reporting/Console` (the second implementation). `TestRunner` and the base classes call the contract; tests use a framework `Step(...)` helper. Class-level suite labels move to a framework attribute mapped by the adapter (Allure.Xunit reads its own attributes at discovery, so keep them in the adapter layer until then).
- **Verify:** Allure report unchanged (names, steps, parameters, attachments, labels) for the Playwright and mobile tests; the same run with `Reporting: console` passes with no Allure output; `CoreBoundaryTests`: `Core` doesn't reference Allure.

<a id="r40"></a>
### R40. Providers are chosen three different ways; there is no composition root or plugin model

- **Status:** Closed, 2026-11-17 · `659a228`. Core/Composition (Framework.Configure composition root in each TestRunSetup, IFrameworkPlugin, ProviderRegistry with explicit named registration) and providers.json (PROVIDERS_* overrides) replace the reflection scan and the per-project Key Vault wiring; plugins: ElasticLoggingPlugin, AzureKeyVaultPlugin, built-in console/environment. Verified: PROVIDERS_Logging=console switches logging and results publishing with no code change; Secrets [environment, azure-keyvault] without KEYVAULT_URI stops the run at startup with the reason; the run prints the selection once; unit 111 (+2 skipped), Playwright 15/15, Android 1/1 locally. Uses a small ProviderRegistry instead of Microsoft.Extensions.DependencyInjection (not needed yet).
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 1.**
- **Where:** `Core/Logging/LoggingServiceFactory.cs` + `ResultsPublisherFactory.cs` (reflection scan of loaded types, selection by `Provider` name), `Core/Configuration/SettingsLoader.cs` (static settable `SecretProvider`, replaced in each `TestRunSetup` module initializer), static factories `PlaywrightDriverFactory`, `ApiClient.CreateAsync`, `MobileDriverFactory`.
- **Problem:** each seam is wired differently and the wiring is spread over fixtures, base classes and module initializers; the reflection scan only finds providers in assemblies that happen to be loaded; nothing shows which implementations a run uses.
- **Fix:** a composition root in `TestRunSetup`: read a `Providers` section (e.g. `"Ui": "playwright", "Http": "playwright", "Reporting": "allure", "Logging": "elastic", "Secrets": ["environment", "azure-keyvault"], "Resilience": "polly"`) and register the chosen adapters in `Microsoft.Extensions.DependencyInjection`. Each adapter project exposes one plugin class (`IFrameworkPlugin`: kind, name, `Register(services, configuration)`); registration is explicit, not a type scan. Fixtures and base classes resolve contracts from the container. Unknown or missing provider names fail at startup with the valid names listed.
- **Verify:** switching logging between console and Elastic, and secrets between environment-only and environment + Key Vault, needs only a config change; the run log states the selected provider for each kind.

<a id="r41"></a>
### R41. Playwright types are part of the API client's public surface

- **Status:** Closed, 2026-11-17 · `1de545d`. Core/Http (IHttpTransport, IHttpTransportFactory, HttpRequestData/HttpResponseData, built-in HttpClient transport) and the Core.Http.Playwright adapter, selected with "Http" in providers.json; ApiClient keeps JSON, status and retries; SendRawAsync returns HttpResponseData; ApiFixture no longer owns Playwright. Verified: the 7 API tests pass with PROVIDERS_Http=playwright and =httpclient; Allure results identical to the baseline; HttpTransportContractTests (status, body, headers incl. Cookie, relative URLs, 404 returned, HttpRequestException when no response) pass for both transports.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 3.**
- **Where:** `Core/Api/ApiClient.cs`: `CreateAsync(IPlaywright playwright, …)` and `SendRawAsync(…)` returning Playwright's `IAPIResponse`; `Tests/Fixtures/ApiFixture.cs` owns an `IPlaywright`; raw-response assertions read `IAPIResponse`.
- **Problem:** the transport isn't behind a contract, so replacing Playwright's `APIRequestContext` changes the client's signatures and every raw-response assertion, and API-only runs need Playwright installed.
- **Fix:** `Core/Http/IHttpTransport` (method, URL, headers, body → status, headers, body) with adapters `Core/Http/Playwright` and `Core/Http/HttpClient`. `ApiClient` keeps JSON, `ApiException`, retries and test semantics; `SendRawAsync` returns a framework `ApiResponse` record; `ApiFixture` resolves the transport. Migrate one test class first, then the rest.
- **Verify:** the 7 API tests pass with `Http: playwright` and with `Http: httpclient`; the API test project no longer needs a Playwright instance when `httpclient` is selected.

<a id="r42"></a>
### R42. The web UI layer is Playwright-specific end to end

- **Status:** Closed, 2026-11-17 · `8bd245b`. Core/Ui contract (IUiDriverFactory/IUiDriver/IUiSession, IElement with Locate/WithChild/First, Ui.Expect assertions, UiException/UiAssertionException, capabilities IScreenshots/ITracing via TryGet, Native<T> escape hatch) with the Core.Ui.Playwright adapter, selected with "Ui" in providers.json; page objects, UiTestBase and web tests use only the contract; PlaywrightFixture renamed BrowserFixture. Verified: 15/15 Playwright tests with Allure results identical to the pre-phase-2 baseline; Tests/Contract UI driver contract suite 8/8 (found and fixed Playwright TimeoutException not being mapped); AdapterBoundaryTests: Pages doesn't reference Playwright.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 5.**
- **Where:** `Pages/Common/BasePage.cs` (`BasePage(IPage)`, `ILocator` helpers), all page objects, `Tests/Infrastructure/UiTestBase.cs` (`IPage`, `IBrowserContext`, tracing), `Tests/Fixtures/PlaywrightFixture.cs`, `Core/Driver/PlaywrightDriverFactory.cs`, web tests using Playwright's `Expect(...)`.
- **Problem:** swapping the browser library means rewriting the page layer, the UI base and the assertions.
- **Fix:** a small UI contract in `Core/Ui` (`IUiSession`: navigate, find; `IElement`: click, fill, text, visible, enabled; waits) plus **capability interfaces** that adapters implement only if they can (`ITracing`, `IScreenshots`, `INetworkControl`, `IMobileGestures`), queried with `session.TryGet<T>(out …)`, and a visible escape hatch `session.Native<T>()` for rare vendor-only needs. Framework assertions `Ui.Expect(element).ToBeVisible()/ToHaveText(...)` implemented per adapter (Playwright's web-first assertions; polling via `Poll` elsewhere). Do **not** flatten everything to a lowest common denominator. Adapter `Core/Ui/Playwright`; migrate one slice (SauceDemo login → inventory) end to end, then the remaining pages and tests.
- **Verify:** all web tests pass on the Playwright adapter with no `Microsoft.Playwright` reference in `Pages` or `Tests/UI/Web`; traces and screenshots still attached on failure through the capabilities.

<a id="r43"></a>
### R43. No dependency inventory, contract list or provider guide

- **Status:** Closed, 2026-11-17 · `0a3cd2a`. docs/PLUGIN_ARCHITECTURE.md (target shape, contract list, dependency inventory checked against every csproj and vendor namespace use, design rules, provider guide with per-kind guarantees, accepted host/platform dependencies); CONTRIBUTING 'Adding or replacing a library'; linked from README and ARCHITECTURE. Verified: every R39-R48 item appears in the contract list, inventory or rules.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 0.**
- **Where:** `docs/ARCHITECTURE.md`, `docs/CONTRIBUTING.md`; new `docs/PLUGIN_ARCHITECTURE.md`.
- **Problem:** nothing lists, per external dependency, whether it is core, optional (adapter) or host-level, its realistic replacement and where its types leak today; there is no written contract for adding a provider.
- **Fix:** (1) inventory table: Playwright (web UI + HTTP), Appium/Selenium, Allure, xUnit, Elasticsearch client, Polly, Azure SDK, RabbitMQ.Client, Confluent.Kafka, Microsoft.Extensions.Configuration/DependencyInjection: role, kind, replacement, current leaks, R item. (2) `docs/PLUGIN_ARCHITECTURE.md`: target shape (contracts in `Core`, one adapter project per vendor under `Core/<Area>/<Vendor>/`, composition root), the design rules (contracts describe framework behaviour, not vendor APIs; capability interfaces; framework assertions; one registration pattern; guardrail and contract tests; a second implementation per contract) and the roadmap. (3) "Writing a provider" in CONTRIBUTING. (4) Record the accepted platform dependencies: xUnit as the host (R48) and `Microsoft.Extensions.*` (already abstraction libraries; not wrapped again).
- **Verify:** docs reviewed against the code; every R39–R48 item links to its inventory row.

<a id="r44"></a>
### R44. Core references seven vendor packages; the target is none

- **Status:** Closed, 2026-11-17 · `8bd245b`. Core references no vendor package: Elasticsearch (R40), Allure (R39), Playwright HTTP (R41), Polly (R45) and Playwright UI (R42) are adapter projects; Microsoft.Extensions.Configuration stays as an accepted platform dependency with IConfiguration out of Core's public API. Verified: Core.csproj has only the Microsoft.Extensions.Configuration packages; AdapterBoundaryTests fails if Core references Elastic, Allure, Polly, Playwright, Azure, RabbitMQ or Kafka assemblies.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 1–4 (umbrella).**
- **Where:** `Core/Core.csproj`: `Microsoft.Playwright`, `Allure.Net.Commons`, `Elastic.Clients.Elasticsearch`, `Polly.Core`, `Microsoft.Extensions.Configuration.{Binder,EnvironmentVariables,Json}`; public signatures exposing them: `ApiClient` (R41), `PlaywrightDriverFactory` (R42), `RetryPolicies` returning `ResiliencePipeline` (R45), `SettingsLoader.ResolveSecrets(IConfiguration, …)`.
- **Problem:** anything that uses `Core` pulls in all of these, and their types reach callers, so a swap ripples beyond one project.
- **Fix:** move each vendor into its adapter project as the phases land: `Core/Logging/Elastic` and `Core/Logging/Console` become projects (phase 1), `Core/Reporting/Allure` (R39), `Core/Http/Playwright` (R41), `Core/Resilience/Polly` (R45), `Core/Ui/Playwright` (R42). Settings loading moves behind an `ISettingsSource` contract with a `Microsoft.Extensions.Configuration` adapter, keeping `IConfiguration` out of public `Core` signatures.
- **Verify:** `Core.csproj` has no `PackageReference` to a vendor library; enforced by R46.

<a id="r45"></a>
### R45. Polly types are part of Core's public resilience API

- **Status:** Closed, 2026-11-17 · `c557c08`. IRetryPolicy / IRetryPolicy<T> / IRetryPolicyFactory in Core/Resilience with a built-in implementation and the Core.Resilience.Polly adapter, selected with "Resilience" in providers.json; RetryPolicies keeps its named policies on the contract; ApiClient, BasePage and MobileBasePage depend on IRetryPolicy only. Verified: the former RetryPoliciesTests now run as a contract suite against both implementations (20 passing); Playwright 15/15 with PROVIDERS_Resilience=polly and =builtin; Android 1/1.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 4.**
- **Where:** `Core/Resilience/RetryPolicies.cs`: `TransientHttp<T>`, `Navigation`, `Transient` return Polly `ResiliencePipeline`/`ResiliencePipeline<T>`; callers in `ApiClient`, `BasePage.GotoAsync`, `MobileBasePage`.
- **Problem:** replacing Polly (or using a simpler retry for a lightweight build) changes every caller.
- **Fix:** `Core/Resilience/IRetryPolicy` (`ExecuteAsync(action)`, created from `RetryOptions` + a transient predicate), `Poll` stays framework code; adapter `Core/Resilience/Polly`, plus a small built-in retry as the second implementation. Callers depend on `IRetryPolicy` only.
- **Verify:** existing `RetryPoliciesTests`/`PollTests` pass against both implementations (as contract tests, R46).

<a id="r46"></a>
### R46. No guardrail or contract tests that prove adapters are replaceable

- **Status:** Closed, 2026-11-17 · `70afeff`. Guardrails: AdapterBoundaryTests (Core references no vendor assembly; Pages uses only the UI contract; each adapter depends on Core only, with the documented Core.Mobile -> Core.Ui.Selenium exception), TestHostBoundaryTests (scenario code and test assemblies), every contract has at least two registered implementations. Shared contract suites for every contract: HTTP transport, retries, UI driver (Tests/Contract), reporting, logging/publishing, secrets. Verified: all pass; each guardrail was shown to fail on an injected violation.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 1 onwards (grows with each phase).**
- **Where:** `Tests/Unit/Architecture/CoreBoundaryTests.cs` (checks project references and app-named namespaces only).
- **Problem:** nothing stops vendor namespaces creeping back into `Core`, `Pages`, `Domain` or tests, and nothing checks that two adapters of one contract behave the same; without that, "replaceable" is an assumption.
- **Fix:** (1) architecture tests: `Core`, `Domain`, `Pages` and scenario tests reference no vendor namespace; each adapter references only `Core` and its own vendor; every contract has at least two registered implementations. (2) contract test suites: one abstract test class per contract (`TestReporterContractTests`, `HttpTransportContractTests`, `RetryPolicyContractTests`, `UiSessionContractTests`, …) that each adapter's test class inherits, so the same behaviour is asserted for every implementation.
- **Verify:** deliberately adding a Playwright `using` to `Pages` or a vendor package to `Core` fails the unit tests; each contract suite runs green for every adapter.

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

- **Status:** Closed, 2026-11-14 · `536694d`. Tests/Unit (Tests.Unit.csproj): 24 tests in about 50 ms covering builders/Mothers, BookingPatch.ApplyTo, SettingsLoader precedence, Elastic URL resolution, ApiException and TestRunner hooks. Built in CI via ci.slnf and run before the Playwright suite.
- **Where:** `Core/`.
- **Problem:** framework logic (`BookingPatch.ApplyTo`, the builders, config providers, `ApiClient` error handling, `TestRunner`) is only tested indirectly, through the end-to-end suites.
- **Fix:** add a small `Core.Tests` project with fast unit tests (no browser, no network) and run it first in CI.

<a id="r19"></a>
### R19. `ITestDataLoader` has one implementation

- **Status:** Closed, 2026-11-14 · `aa1079f`. TestDataLoader is now a static class; the interface and singleton are gone.
- **Where:** `Core/TestData/ITestDataLoader.cs`, `Core/TestData/TestDataLoader.cs`.
- **Problem:** the interface has a single implementation, and the class is only used through its static `Instance` singleton. Since JSON test data is now the exception, nothing currently calls it.
- **Fix:** drop the interface (and possibly the singleton). Bring back an abstraction if a second loader (e.g. CSV, or loading from a database) is ever needed.

<a id="r22"></a>
### R22. `Console.Enabled` in `logging-config.json` does nothing

- **Status:** Closed, 2026-11-14 · `e8ba96d`. Removed ConsoleSection and the Console key from code, logging-config.json and the docs. The console stays on as the fallback when Elastic can't be used (see R21), so a switch to turn it off would only hide problems.
- **Where:** `logging-config.json` (`Console.Enabled`), `Core/Logging/ConsoleSection.cs`.
- **Problem:** the setting is loaded but never read, so `"Enabled": false` doesn't turn console logging off. A setting that looks like it works but doesn't is misleading.
- **Fix:** either honour it (e.g. the console provider logs nothing, or only errors, when disabled) or remove `ConsoleSection` and the key from the file and docs.

<a id="r23"></a>
### R23. Allure results pile up across local runs

- **Status:** Closed, 2026-11-14 · `03cf510`. Each test run clears the folder first (module initializer → Core.Testing.AllureResults.ClearPreviousRun). The pipeline copies results out after every dotnet test, including each nightly browser run. Checked: two local runs give 2 results each (before: 2, then 4); a full run keeps all 15; a chrome/chromium/webkit loop keeps all 6.
- **Where:** `Tests/bin/<Configuration>/net10.0/allure-results/` (written by `Allure.Xunit`).
- **Problem:** `Allure.Xunit` doesn't clear its results folder, so each local `dotnet test` adds to the previous runs' results (checked: 2 results after one run, 4 after two). `allure serve` on that folder then mixes old and new runs. The run scripts delete the folder first, and CI agents start clean, so only plain local `dotnet test` runs are affected.
- **Fix:** clear the folder once at the start of a test run, e.g. a `[ModuleInitializer]` in the test projects calling `AllureLifecycle.Instance.CleanupResultDirectory()`, or document running through the scripts.

<a id="r36"></a>
### R36. No secret-provider abstraction (e.g. Key Vault) behind SettingsLoader

- **Status:** Implemented (verification pending), 2026-11-17. Seam and environment provider in `f0e6b65`; Azure Key Vault add-on in `b97d9c7` (`Core.Secrets.AzureKeyVault`, on when `KEYVAULT_URI` is set, environment variables first; 5 unit tests with a `SecretClient` test double). **Verify:** resolve a `secret://` placeholder from a real vault (needs a vault and an identity with *Get* on secrets; not available here). **2026-11-17 (R40):** Key Vault is now chosen by name: `"Secrets": ["environment", "azure-keyvault"]` in `providers.json` (or `PROVIDERS_Secrets__1=azure-keyvault`) plus `KEYVAULT_URI`; `KEYVAULT_URI` alone no longer switches it on.
- **Source:** external architecture feedback, 2026-11-17.
- **Where:** `Core/Configuration/SettingsLoader.cs`.
- **Problem:** secrets (Elastic and API credentials) come from environment variables (R1, R6). That's fine for CI and local runs, but enterprise setups usually pull credentials at runtime from Azure Key Vault, AWS Secrets Manager or HashiCorp Vault, and there's no seam for that.
- **Fix:** add `Core/Security/ISecretProvider` and let `SettingsLoader` resolve placeholders such as `secret://elastic-password` through it (an environment-variable provider by default, a Key Vault provider via `Azure.Security.KeyVault.Secrets` + `DefaultAzureCredential` when configured). For Azure Pipelines alone, a variable group linked to Key Vault already delivers secrets as environment variables without code changes, so build this when a vault actually exists.

<a id="r37"></a>
### R37. No data / infrastructure helpers (database, message queue) with cleanup tracking

- **Status:** Closed, 2026-11-17 · `8a6dc6d`. Cleanup tracker and database helpers in `292c4c8` (SQLite-verified); message listeners in `8a6dc6d`: `Core/Messaging` (`MessageCollector<T>`) plus `Core.Messaging.RabbitMq` and `Core.Messaging.Kafka` add-ons, verified against RabbitMQ 3.13 and Kafka 3.8 in Docker (3/3 runs, no queues or topics left). Broker tests are skipped unless `RABBITMQ_URI` / `KAFKA_BOOTSTRAP` are set.
- **Source:** external architecture feedback, 2026-11-17.
- **Where:** would be `Core/Data/` (doesn't exist).
- **Problem:** end-to-end tests of an owned system often need to seed state or check persistence in a database or queue. The current apps under test are public demo sites without database or queue access, so nothing needs this today, but there's no pattern ready for it.
- **Fix (when a system under test needs it):** a `DbConnectionFactory` (e.g. Dapper over SQL Server/Postgres) and queue listeners (Kafka/RabbitMQ), with built-in cleanup tracking or transaction rollback, following the same pattern as `ApiTestBase.CreateBookingAsync`, which records created bookings and deletes them after the test (R5).

<a id="r38"></a>
### R38. Mobile driver bootstrapping can't be reused outside the mobile test project

- **Status:** Closed, 2026-11-17 · `cf18aa0`. New Core.Mobile library (settings + provider, driver factory/options, interaction helpers), referenced only by mobile suites, so Core has no Appium/Selenium dependency; page objects and tests stay in Tests/UI/Mobile. Boundary tests check that Core doesn't reference Appium and Core.Mobile doesn't reference Domain/Pages/test projects. Unit 67/67, Playwright 15/15, mobile test discovered.
- **Source:** external architecture feedback, 2026-11-17 (its checklist lists `MobileDriverFactory` among the bootstrapping that belongs in Core).
- **Where:** `Tests/UI/Mobile/Driver/` (`MobileDriverFactory`, `MobileOptionsBuilder`), `Tests/UI/Mobile/Configuration/`.
- **Problem:** the Appium bootstrapping and its settings live inside the mobile test project, so another team or project can't reuse them. Moving them into Core as it is would make every Core consumer depend on Appium and Selenium.
- **Fix:** together with R34's split, move the mobile driver factory, options builder and `MobileSettings` into a separate library (e.g. `Core.Mobile`) that only mobile suites reference.

<a id="r47"></a>
### R47. Only one implementation exists for the UI contract (no second browser or device driver)

- **Status:** Closed, 2026-11-17 · `1d6fdec`. Core.Ui.Selenium adapter (selenium) and the appium UI driver in Core.Mobile (WebDriver session + IMobileGestures); the mobile suite uses the shared page objects and Ui.Expect. Verified: Tests/Contract UI suite 18/18 (Playwright and Selenium); Android smoke passes through the appium driver with the shared Pages.SauceDemo.LoginPage (Allure result identical to the baseline); the migrated web slice (SauceDemo login -> inventory, plus TodoMVC and negative login) passes with PROVIDERS_Ui=selenium BROWSER=chrome (5 of 8). Limitation: SauceDemo's cart link ignores WebDriver's native clicks (reproduced with plain Selenium, no framework code), so the 3 tests that open the cart pass only on Playwright; recorded in GETTING_STARTED and CHANGELOG.
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 6.**
- **Where:** after R42: `Core/Ui/Playwright` only; `Core/Mobile` (Appium) sits outside the UI contract with its own `MobileBasePage`.
- **Problem:** a contract with one implementation hasn't proven it is replaceable; mobile page objects use a different API from web ones.
- **Fix:** a `Core/Ui/Selenium` adapter for web, and Appium as a `Core/Ui/Appium` adapter of the same contract plus `IMobileGestures`; mobile pages move onto the shared page base. Capabilities a driver lacks (e.g. Playwright tracing on Selenium) are simply not offered.
- **Verify:** the migrated web slice passes with `Ui: selenium`; the mobile smoke passes through the shared UI contract; R46's UI contract suite runs for all three adapters.

<a id="r48"></a>
### R48. xUnit-specific code isn't isolated in a thin host layer

- **Status:** Closed, 2026-11-17 · `70afeff`. Test classes use xUnit only through [Fact]/[Theory]/[Trait] and Assert; IClassFixture/IAsyncLifetime only in fixtures and base classes; report labels moved from Allure attributes to [ReportSuite]/[ReportTags]; xUnit and its Allure hook documented as the accepted host layer with the migration path (PLUGIN_ARCHITECTURE §6). Verified: TestHostBoundaryTests (3 checks, metadata only) pass and fail when a test class gets an Allure attribute or an xUnit interface; Allure results incl. labels identical to the baseline. xUnit's Assert is kept in test bodies on purpose (not wrapped).
- **Source:** external architecture feedback on replaceable adapters + owner goal "plug and play: every external library replaceable", 2026-11-17. Roadmap: [Plug-and-play roadmap](#plug-and-play-roadmap). **Phase 7.**
- **Where:** test classes (`[Fact]`, `[Theory]`, `[Trait]`, `IClassFixture`, `IAsyncLifetime`), `Tests/Fixtures/*`, base classes, `RequiresEnvironmentFactAttribute`.
- **Problem:** xUnit discovers and runs tests, so it isn't a plugin like the others, but its types are spread across fixtures and base classes; a later move to xUnit v3 or NUnit would touch more than necessary.
- **Fix:** keep test bodies on framework APIs only (R39–R42 do most of this); confine xUnit types to attributes, fixtures and base classes, and document xUnit as the accepted host in PLUGIN_ARCHITECTURE (R43).
- **Verify:** an architecture test lists the only folders allowed to reference `Xunit` besides test attributes.
