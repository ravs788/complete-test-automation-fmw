# Framework Review

Findings from the 2026-11-14 consistency review (see CHANGELOG) and the 2026-11-16 project review using the code-review, test-review, coverage-review, and documentation-review skills. Each item names the affected files, what's wrong and a suggested fix. Priority: **P1** fix soon · **P2** should do · **P3** good practice · **P4** nice to have.

**How this list is maintained**

- IDs (`R1`, `R2`, …) never change, so re-sorting doesn't renumber anything. New findings get the next free ID.
- Use **Implemented (verification pending)** when the fix is present but runtime verification is outstanding. Once verified, set **Status** to `Closed` and fill in **Closed in** with the date and the commit (or PR) that fixed it. Do not invent a commit reference for working-tree changes.
- **Sort order:** unresolved items (Open or Implemented with verification pending) first, highest priority on top. Then closed items, most recently closed first. Items closed in the same commit: highest priority first. Ties within a priority: by ID.
- The table below is the sorted view. The detail sections after it stay grouped by priority, so links don't move.

| ID | Priority | Status | Closed in | Item | Area |
|---|---|---|---|---|---|
| R31 | P2 | Implemented (verification pending) | – | [Allure history download never finds a report, so every build is "partially succeeded"](#r31) | CI / reporting |
| R32 | P2 | Implemented (verification pending) | – | [Android emulator doesn't finish booting on the hosted Mac within 300 s](#r32) | CI / mobile |
| R16 | P3 | Implemented (CI verification pending) | – | [Mobile suite not in CI](#r16) | CI |
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

<a id="r31"></a>
### R31. Allure history download never finds a report, so every build is "partially succeeded"

- **Status:** Implemented (verification pending), 2026-11-17, in `4304475`. `allowPartiallySucceededBuilds: true` and history always from `refs/heads/main`. **Verify:** this PR's build should log a history download from a recent `main` build (107/108 have `allure-report`) without the "not found for build 99" warning; after merge, `main` builds should end **succeeded** and the report should show a trend.
- **Where:** `azure-pipelines.yml` (`Download Previous Allure History`, `DownloadPipelineArtifact@2` with `buildVersionToDownload: latestFromBranch`).
- **Problem:** found while checking runs 107 and 108 (2026-11-17). `latestFromBranch` only looks at builds that *succeeded* (plus failed ones, because of `allowFailedBuilds`); *partially succeeded* builds need `allowPartiallySucceededBuilds: true`. The last fully successful `main` build is 99, which ran before the report step existed, so every `main` build since (102–108) downloads from build 99, warns `Artifact allure-report was not found for build 99`, and ends "partially succeeded", which keeps it out of the next build's search too. PR builds (`refs/pull/N/merge`) have no earlier builds at all, so they warn as well. Result: no Allure trends, and no build is ever fully green.
- **Fix:** set `allowPartiallySucceededBuilds: true` and take the history from `refs/heads/main` for every build (PRs and branches then compare against `main`).

<a id="r32"></a>
### R32. Android emulator doesn't finish booting on the hosted Mac within 300 s

- **Status:** Implemented (verification pending), 2026-11-17, in `be414c3`. Headless emulator with 2 cores / 3 GB, `swiftshader_indirect`, 600 s boot budget, `boot-progress.log`, `UseNode@1`. All flags checked against emulator 37.1 help; script syntax and YAML checked. **Verify:** a manual (or the next nightly) run whose `MobileSmoke` job boots Android, runs the smoke test and passes; that also closes R16.
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
