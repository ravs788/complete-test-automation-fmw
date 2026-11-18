# Framework Review

Findings from the 2026-11-18 security review (R61–R65), the owner-requested facade design recommendation (2026-11-18, R60), the 2026-11-18 five-skill repository review (R54–R59; code, test, coverage, documentation and test modernization), the earlier 2026-11-18 solution and adapter review (R49–R53), the 2026-11-14 consistency review (see CHANGELOG), external architecture feedback on the Core layer (2026-11-17, R34–R38) and on replaceable adapters plus the owner's plug-and-play goal (2026-11-17, R39–R48; see [Plug-and-play roadmap](#plug-and-play-roadmap)), CI investigations (R31–R33) and the 2026-11-16 project review using the code-review, test-review, coverage-review, and documentation-review skills. Each item names the affected files, what's wrong and a suggested fix. Priority: **P1** fix soon · **P2** should do · **P3** good practice · **P4** nice to have.

**How this list is maintained**

- All substantive findings and actionable recommendations from `code-review`, `test-review`, `coverage-review`, `documentation-review`, `test-modernization-skill` and `security-review` must be recorded here for standalone and pre-commit reviews, including issues fixed in the same task. Attribute the skill(s), date and reviewed scope, with evidence, priority, action, status and verification criteria. Distinguish defects, verification gaps and optional recommendations; redact sensitive evidence. Record no-findings/not-applicable outcomes in a dated review summary rather than creating empty findings. See [required review records](../AGENTS.md#required-review-records).
- IDs (`R1`, `R2`, …) never change, so re-sorting doesn't renumber anything. New findings get the next free ID.
- Use **Blocked** for explicitly deferred or externally blocked work, with the reason, date and condition for resuming. Preserve the earlier implementation and verification evidence.
- Use **Implemented (verification pending)** when the fix is present but runtime verification is outstanding. Once verified, set **Status** to `Closed` and fill in **Closed in** with the date and the commit (or PR) that fixed it. Do not invent a commit reference for working-tree changes.
- **Sort order:** unresolved items (Open, Blocked, or Implemented with verification pending) first, highest priority on top. Then closed items, most recently closed first. Items closed in the same commit: highest priority first. Ties within a priority: by ID.
- The table below is the sorted view. The detail sections after it stay grouped by priority, so links don't move.

| ID | Priority | Status | Closed in | Item | Area |
|---|---|---|---|---|---|
| R32 | P2 | Blocked (mobile work deferred) | – | [Android emulator doesn't finish booting on the hosted Mac within 300 s](#r32) | CI / mobile |
| R33 | P2 | Blocked (mobile work deferred) | – | [Appium's 20 s helper-app install timeout is too short for the hosted emulator](#r33) | CI / mobile |
| R61 | P2 | Implemented (verification pending) | – | [Local logging stack publishes unauthenticated ports beyond loopback](#r61) | Security / local infrastructure |
| R16 | P3 | Blocked (mobile work deferred) | – | [Mobile suite not in CI](#r16) | CI |
| R58 | P3 | Implemented (verification pending) | – | [RabbitMQ queue-removal test accepts unrelated exceptions](#r58) | Tests / messaging |
| R60 | P3 | Open | – | [Move scenario orchestration from page objects to a facade workflow API](#r60) | Architecture / test design |
| R65 | P3 | Implemented (verification pending) | – | [Define an artifact sensitivity and publication policy](#r65) | Security hardening / diagnostics |
| R36 | P4 | Implemented (verification pending) | – | [No secret-provider abstraction (e.g. Key Vault) behind SettingsLoader](#r36) | Security / configuration |
| R62 | P2 | Closed | 2026-11-18 · `3b2d62a` | [Diagnostic sinks expose credential-bearing URLs and exception messages](#r62) | Security / logging |
| R63 | P2 | Closed | 2026-11-18 · `3b2d62a` | [Remote Elastic endpoints accept Basic credentials over HTTP](#r63) | Security / transport |
| R64 | P2 | Closed | 2026-11-18 · `3b2d62a` | [Manual authentication headers can cross redirect destinations](#r64) | Security / HTTP adapters |
| R54 | P2 | Closed | 2026-11-18 · `572cad6` | [Database setup and disposal failures can leak connections](#r54) | Data / cleanup |
| R55 | P2 | Closed | 2026-11-18 · `572cad6` | [Selenium role locators differ from Playwright semantics](#r55) | UI / adapter contracts |
| R56 | P2 | Closed | 2026-11-18 · `572cad6` | [Selenium CheckAsync can succeed without checking the element](#r56) | UI / adapter contracts |
| R57 | P2 | Closed | 2026-11-18 · `572cad6` | [HTTP providers resolve base URLs without trailing slashes differently](#r57) | HTTP / adapter contracts |
| R59 | P3 | Closed | 2026-11-18 · `572cad6` | [Setup and dependency documentation contradicts current implementation](#r59) | Documentation |
| R49 | P2 | Closed | 2026-11-18 · `28b32bf` | [Selenium silently chooses among ambiguous element matches](#r49) | UI / adapter contracts |
| R50 | P2 | Closed | 2026-11-18 · `28b32bf` | [Playwright HTTP transport ignores cancellation](#r50) | HTTP / adapter contracts |
| R51 | P2 | Closed | 2026-11-18 · `28b32bf` | [RabbitMQ startup failures leak partially created resources](#r51) | Messaging / cleanup |
| R52 | P2 | Closed | 2026-11-18 · `28b32bf` | [Valid duplicate Kafka headers can fault the listener and bypass cleanup](#r52) | Messaging / cleanup |
| R53 | P3 | Closed | 2026-11-18 · `28b32bf` | [Top-level review status and validation summaries are stale](#r53) | Documentation / review ledger |
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

## Mobile work status — 2026-11-18

At the user's request, **R16, R32 and R33 are Blocked (mobile work deferred)** until mobile work is resumed. Their existing implementations and verification history remain recorded. Closed mobile findings remain closed; shared web/API adapter findings remain active. This is a review-tracking update; pipeline scheduling/configuration is unchanged.

## Earlier adapter review scope and validation — 2026-11-18

- **Comparison:** `d8d9aee` (last local commit in the November 16 session) to `a45b1d7` (current main at review time), plus working-tree inspection. The reflog confirms the checkout was pulled forward from the earlier main on November 18. The working tree was clean at review time.
- **Change scale:** 64 subsequent commits; 216 changed files; 6,348 additions and 1,055 deletions. The solution expanded from 5 to 17 projects, separating Domain and vendor adapters from Core and adding shared contract/architecture tests.
- **Findings:** R49–R52 are P2 adapter/failure-path gaps; R53 is a P3 documentation consistency issue. These extend the earlier contract-test work; they do not reopen the historical architecture findings.
- **Completed checks:** Restored the newly pulled project dependencies, then built `ci.slnf` with warnings treated as errors: 0 warnings, 0 errors. `git diff --check` passed. No source files were changed by the review.
- **Original static-review limits (before the fixes below):** Tests were not rerun for the initial review. The existing local TRX records 37 passing tests on November 16 and does not validate the newer adapter refactor. No current measured coverage report was available. Verification steps for the new findings are proposed regression checks, not completed executions.
- **Historical summaries below:** The November 16 snapshots retain their original evidence and are now explicitly labelled historical. R53 reconciles them with the current summary and validation below.

## Current status and verification — 2026-11-18

| State | Findings | Count |
|---|---|---|
| Open; facade workflow migration recommendation | R60 | 1 |
| Blocked at user request; mobile work deferred | R16, R32, R33 | 3 |
| Implemented; live Key Vault, RabbitMQ, Docker isolation and Azure policy verification pending | R36, R58, R61, R65 | 4 |
| Closed; verified security changes in `3b2d62a` | R62–R64 | 3 |
| Closed in the R54–R59 fix pass | R54–R57, R59 | 5 |
| Closed after the earlier framework fix pass | R49–R53 | 5 |
| Closed in earlier reviews | R1–R15, R17–R31, R34–R35, R37–R48 | 44 |
| Total | R1–R65 | 65 |

R49–R53 were fixed and verified in `28b32bf` on `fix/framework-adapter-contracts`, based on `a45b1d7`; that branch has since been merged into main and is included in the new branch base `5636b07`. R25 is closed, as recorded by its existing finding. Mobile work remains blocked until the user resumes it; no mobile tests or CI changes were made in this pass. R36 still requires a real vault/identity; KEYVAULT_URI is not configured.

**Earlier framework-fix validation (reused evidence, not rerun by the five-skill review):** Full solution build passed with warnings treated as errors (0 warnings, 0 errors). The final framework suite passed **191 tests, 0 failed, 2 skipped** (193 total); the skipped cases require live Kafka/RabbitMQ brokers. Controlled vendor-interface tests cover the new broker failure paths without external services. The shared UI suite passed **28 tests, 0 failed, 0 skipped** against Playwright and Selenium in headed Chrome, and fixture cleanup closed its owned browsers. Shell/.NET execution used the user's earlier explicit verification exception. `git diff --check` passed.

**Evidence:** [Summary](../artifacts/framework-fixes-2026-11-18/summary.json), [framework TRX](../artifacts/framework-fixes-2026-11-18/framework-fixes.trx), and [UI contract TRX](../artifacts/framework-fixes-2026-11-18/framework-ui-fixes.trx). These generated local artifacts are ignored by Git. An initial RabbitMQ test-double type-conversion error was corrected before the final passing run.

**Earlier framework-fix coverage boundary:** No instrumented coverage was collected in that earlier run. UI checks used controlled local pages, HTTP checks used loopback servers, and broker failure checks used interface doubles; this does not claim a fresh external-app, live-broker, mobile or Key Vault integration run. The November 16 37-test result below is historical and does not establish current coverage.

## Five-skill repository review — 2026-11-18

- **Scope:** Revision `8337c0453be7471bdc9ce4067a967cf9a9c14062` plus the working-tree `AGENTS.md` and `docs/CONTRIBUTING.md` pre-commit rules. Inspected implementation, representative scenarios, fixtures, contracts, project/package configuration, CI and documentation using `code-review`, `test-review`, `coverage-review`, `documentation-review` and `test-modernization-skill`. This was a repository review, not just a review of the documentation diff.
- **Outcome:** Six new findings: R54–R57 are P2, R58–R59 are P3. No new P1 finding was identified in the inspected paths. These are additional failure-path and consistency findings; earlier closed findings remain closed.
- **Fresh verification:** `dotnet build ci.slnf --no-restore -m:1 -p:UseSharedCompilation=false -p:TreatWarningsAsErrors=true` passed with zero warnings/errors. `git diff --check` passed. Scanned local Markdown file targets in README, AGENTS and docs; none were missing. Anchor/external-link validity was not exhaustively checked.
- **Execution limits:** No tests were rerun. Existing November 18 TRX files record 191 framework passes, two broker skips and 28 UI contract passes; their summary was generated before the fixes were committed and describes that earlier working tree. These are reused test records, not fresh verification of R54–R59. No instrumented coverage report was found locally. No live broker, Key Vault, external-application or mobile run was performed.
- **Deferrals:** R16/R32/R33 remain blocked at the user's request. R36 still needs live Key Vault verification. R59 permits correcting descriptions of mobile status; it does not resume mobile implementation or execution.

### Coverage gaps and evidence

At the five-skill review, measured line/branch coverage was **unavailable locally**; the subsequent unit-only collection is recorded below. Test counts and a successful build do not establish percentages. CI supplies `--collect "XPlat Code Coverage"` to unit and application suites outside PR runs, but the UI contract invocation in `azure-pipelines.yml` does not receive that option and `Tests/Contract/Tests.Contract.csproj` has no collector reference. Before interpreting reports as browser-adapter coverage, instrument that suite and retain the revision, executed suites, assemblies, exclusions and report provenance. See [Microsoft coverage guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage).

| Behavior / risk | Existing evidence | Gap / smallest valuable test | Tracking |
|---|---|---|---|
| Database lifecycle | SQLite tests exercise normal rollback and commit | Inject connection-open, begin-transaction, rollback and disposal failures; verify every owned resource is released and the first failure is preserved | R54 |
| UI provider equivalence | Shared happy-path and ambiguity contracts; earlier 28-pass TRX | Associated labels, `aria-labelledby`, hidden role matches, invalid checkbox targets and prevented checkbox changes | R55, R56 |
| HTTP URL resolution | Shared relative-URL test uses a trailing-slash base | Run identical endpoint cases against both providers with and without a trailing slash | R57 |
| Broker cleanup | Controlled failure tests; environment-gated live tests were skipped in the latest local run | Require RabbitMQ's precise missing-queue response; retain live broker checks for actual protocol behavior | R58 |

These scenario gaps are static inferences from inspected tests, not measured uncovered lines. A native collection command for an authorized follow-up is `dotnet test Tests/Unit/Tests.Unit.csproj --collect "XPlat Code Coverage"`; UI contract collection first needs its collector/configuration wiring and must follow the active execution rules.

### Modernization assessment and roadmap

Research date: **2026-11-18**. The current .NET 10 / xUnit v2 / Playwright 1.64.1 framework already has useful domain boundaries, explicit composition, replaceable adapters, isolated sessions, shared contract tests and controlled failure tests. Retain these choices and improve observable guarantees before considering a broader migration. The recommendations below are review outcomes, not authorization to add packages, provision infrastructure or change CI.

| Current approach | Recommendation and current reference | Effort / risk, prerequisites and acceptance |
|---|---|---|
| Driver-neutral adapters with shared contracts | **Retain and strengthen.** Extend contracts around R55–R57 instead of adding another provider. [Playwright role semantics](https://playwright.dev/dotnet/docs/api/class-locator#locator-get-by-role), [checked-state guarantee](https://playwright.dev/dotnet/docs/api/class-locator#locator-check), [URL resolution](https://playwright.dev/dotnet/docs/api/class-apirequest#api-request-new-context) | Small–medium. Existing Playwright/Selenium implementations must give equivalent outcomes for the added cases without changing intended page-object behavior. |
| Coverage collection omits UI contracts | **Adopt consistent collection and revision-labelled reports.** Keep behavior coverage distinct from execution metrics. [Microsoft guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage) | Small. Verify collector compatibility with the current VSTest/xUnit host; retain suite/assembly/exclusion information with the report. Do not introduce an arbitrary percentage threshold. |
| Broker integration tests depend on externally configured services | **Pilot an isolated RabbitMQ fixture.** Reuse existing Docker scripts first, or evaluate [Testcontainers for .NET](https://dotnet.testcontainers.org/modules/rabbitmq/). | Medium; requires Docker-capable CI and an approved broker image. Validate startup readiness, actual test execution, unique resources and cleanup after success/failure. Keep the current setup if compatibility, resource cost or cleanup fails the pilot. No package/version migration was validated in this review. |
| xUnit 2.9.3, runner 2.8.2 and Allure 2.15.0 | **Retain for now.** [Current Allure guidance](https://allurereport.org/docs/xunit/) still states xUnit v3 is unsupported and runner v3 cannot load its reporter. | No migration proposed. Reconsider only after a compatible host/reporter combination demonstrates equivalent results, labels, steps and attachments. |
| .NET 10 target | **Retain the supported LTS target.** [Microsoft's support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) lists .NET 10 as active LTS. | No major-version change justified. This support check does not establish that every installed SDK/runtime/package is on its latest servicing patch. |

**Sequence:** (1) fix R54–R57 with focused regression tests; tighten R58 and correct R59; (2) collect coverage including UI contracts with clear provenance; (3) pilot isolated broker execution if Docker CI capacity is available. Larger runner/tool migrations remain conditional on demonstrated compatibility and benefit. No speed or flake reduction is claimed without measurements. Mobile work remains deferred throughout.

## R54–R59 implementation and verification — 2026-11-18

Work continues on **`fix/review-r54-r59`**, created from the latest `origin/main` at `5636b07` after the previous adapter branch was merged. Earlier uncommitted AGENTS/contribution rules and this review ledger were preserved. These fixes were committed in `572cad6`; the subsequent security work is recorded separately below.

- **Implemented:** R54–R57 and R59 are verified and closed in this commit. R58's stricter RabbitMQ assertion is implemented, with live protocol verification pending. R16/R32/R33 remain blocked and R36 remains pending.
- **Build:** `dotnet build ci.slnf --no-restore -m:1 -p:UseSharedCompilation=false -p:TreatWarningsAsErrors=true` passed with zero warnings/errors, including the final Selenium checkbox implementation.
- **Framework tests:** `dotnet test Tests/Unit/Tests.Unit.csproj --no-build --no-restore --logger 'trx;LogFileName=review-r54-r59.trx' --collect 'XPlat Code Coverage'` passed **217**, failed **0**, skipped **2** (219 total). The skips require live Kafka/RabbitMQ endpoints. New database failure and HTTP URL-resolution regressions passed.
- **UI contracts:** An initial headed Chrome run passed 38 cases. The final run passed **42**, failed **0**, skipped **0**, including custom ARIA checkbox success/idempotence and failure cases against both adapters. Fixtures dispose their sessions and owned browsers.
- **Execution:** Used the user's earlier shell/.NET verification exception. Initial sandbox attempts aborted before test execution because VSTest could not bind local sockets; the authorized runs outside the sandbox executed successfully.
- **Coverage:** The unit suite produced Cobertura data: **1,249 / 2,733 lines**, **304 / 776 branches** across its reported assemblies. This is a unit-run denominator, including referenced Pages, scenario/host and adapter assemblies, not whole-solution behavior coverage. UI contracts were not instrumented. The final Selenium ARIA follow-up was validated separately; this collection is evidence for the database/HTTP fixes and must not be represented as coverage of that follow-up. The earlier review's missing-report observation remains historical.
- **Limits:** No fresh external-application, live-broker, mobile or live Key Vault test run. Docker and a local RabbitMQ executable are unavailable; R58 remains verification pending. Optional CI coverage wiring and an isolated broker fixture remain recommendations, not implemented changes.
- **Local evidence:** [Summary](../artifacts/review-r54-r59-2026-11-18/summary.json), [framework TRX](../artifacts/review-r54-r59-2026-11-18/review-r54-r59.trx), [unit coverage](../artifacts/review-r54-r59-2026-11-18/coverage.cobertura.xml), [final UI contract TRX](../artifacts/review-r54-r59-2026-11-18/review-ui-r54-r59.trx). Generated artifacts are ignored by Git; the tracked ledger records their scope and results for readers without local artifacts.

## Commit-specific five-skill review — 2026-11-18 (`572cad6`)

Scope: the staged R54–R59 fixes, regression tests, documentation/review ledger and new AGENTS pre-commit rule on `fix/review-r54-r59`, based on `5636b07`. All five skills were applied to the staged diff and relevant callers/configuration. Earlier repository review conclusions were rechecked rather than treated as a substitute for this review.

| Skill | Outcome | Evidence / remaining limit |
|---|---|---|
| Code review | Database ownership and first-error preservation, provider URL parity, computed role lookup and checkbox postconditions reviewed. The pre-commit review found and corrected an additional R56 enabled-state wait regression. | An unchecked disabled element now waits until enabled; an already checked disabled element remains a no-op. No further defect identified in the inspected changes. Mobile runtime compatibility remains deferred. |
| Test review | Failure injection checks actual resource disposal; loopback checks server-observed URLs; shared UI contracts check real DOM effects and failed state changes. | Added delayed-enable regression. R58 requires the exact exception type and reply code; its live case remains skipped/pending. |
| Coverage review | Reused unit-only Cobertura and TRX evidence, with UI contracts reported separately. No whole-solution or changed-line percentage claimed. | Database factory lines/branches: 100%/100%; DbTestScope: 100%/75% in that unit report. Covered lines do not prove assertion effectiveness. UI contracts are not instrumented and the final checkbox follow-up is outside the unit coverage snapshot. |
| Documentation review | Current CI/device/Selenium descriptions traced to configuration/projects; current and historical test evidence distinguished; stable finding IDs retained. | Local file targets and finding anchors checked. The mobile deferral and live Key Vault/RabbitMQ verification limits remain explicit. |
| Test modernization | Current online guidance rechecked; retain existing contracts and browser-computed accessibility rather than introduce another dependency or migrate runners. | Small follow-ups remain: instrument UI contracts, then pilot isolated broker execution if infrastructure is available. Optional improvements do not block this commit. |

**Final commit checks:** Formatting completed for the changed C# files. The post-review build passed with zero warnings/errors. The fresh headed Chrome contract run passed **44/44**, including the delayed-enable R56 regression; owned sessions/browsers completed fixture cleanup. The earlier **217-pass / 2-skip** framework run and unit coverage were reused because the database/HTTP code and unit assertions are unchanged. Initial formatter attempts could not open sandbox-local named pipes; the authorized outside-sandbox formatter succeeded. No live broker or mobile checks were added.

Modernization sources checked live for this commit: [Selenium computed accessibility properties](https://www.selenium.dev/selenium/docs/api/dotnet/webdriver/OpenQA.Selenium.WebElement.html), [Playwright checked-state/actionability semantics](https://playwright.dev/dotnet/docs/api/class-locator#locator-check), and [Allure xUnit compatibility](https://allurereport.org/docs/xunit/). These support the incremental changes and retaining xUnit v2/runner 2.8.2 while Allure's current documentation excludes v3. No dependency upgrade or broader migration was proposed or performed. The earlier modernization matrix records effort, prerequisites, acceptance criteria and pilot stop conditions.

## Security review — 2026-11-18

- **Skill / scope:** `security-review`, revision `758bab4045b74f99e3fb74841cd337eb787080e5` on `docs/facade-design-review`, plus current AGENTS/contribution/review-ledger changes. Inspected relevant settings and secret providers, HTTP/UI and logging/reporting adapters, database/broker helpers, run scripts, package manifests/restore assets and pipeline configuration. Assets include service credentials, test data, source, CI identities and diagnostic artifacts. Boundaries include the developer host/network, application endpoints and redirect destinations, logging backend and CI artifact readers.
- **Outcome:** R61–R64 are new P2 security gaps supported by code/configuration and primary documentation; R65 is a P3 optional hardening recommendation. No new P1 finding was identified in the inspected scope. Exposure depends on the prerequisites in each finding; no actual credential theft or externally exposed running deployment was demonstrated. No source, tests, pipeline or infrastructure were modified by this review.
- **Standards:** Consulted [OWASP ASVS 5.0.0](https://asvs.owasp.org/), [NIST SSDF 1.1](https://csrc.nist.gov/projects/ssdf) and relevant vendor/OWASP guidance live on the assessment date. These inform applicable transport, data-handling and delivery topics; this is not a complete ASVS requirement mapping, certification or compliance assessment. CWE-532 is used only for the concrete conditional logging path in R62.
- **Dependency check:** .NET SDK **10.0.404**, `net10.0`, `dotnet list ci.slnf package --vulnerable --include-transitive --no-restore --format json`. The authorized network-enabled run completed across **17 projects** and reported **zero vulnerable packages** in the NuGet advisory response. A separate resolved inventory contains **65 unique package/version pairs** (360 per-project/framework entries). The first sandbox attempt failed DNS lookup and provided no audit result. This uses existing restore assets and NuGet's public advisory feed, not fresh restores or proof of vulnerability absence. Container images, npm/tools, browsers, JDK, OS and a complete native-binary inventory were not audited.
- **Secret inspection:** A bounded pattern scan of tracked working-tree text files up to 1 MB found no private-key headers, GitHub-token shapes or AWS access-key shapes. Four credential-assignment matches were inspected as sample/demo locations, with values omitted from the report. This was not a dedicated secret-scanner run or a Git-history scan. R1's owner action to rotate any previously real historical credential remains unverified; its implementation stays closed and no historical credential was retrieved or reproduced.
- **Verification limits / deferrals:** Static inspection and read-only inventory/advisory checks only; no tests, exploitation, external target scans, pipeline runs or deployment changes. Azure/GitHub permissions, fork secret policy, artifact access/retention and real vault authorization were not inspected remotely. R36/R58 stay verification pending; R16/R32/R33 remain blocked. The facade pilot R60 is not implemented by this security review.
- **Local evidence:** [Summary](../artifacts/security-review-2026-11-18/summary.json), [NuGet advisory result](../artifacts/security-review-2026-11-18/nuget-vulnerabilities.json), [resolved inventory](../artifacts/security-review-2026-11-18/nuget-inventory.json). Generated evidence is ignored by Git; tracked findings below retain the actionable details and limitations.

| Control area / applicability | Inspected evidence | Outcome / next check |
|---|---|---|
| Local service isolation; applicable to the development stack | Docker port bindings with Elasticsearch security disabled | Finding R61; validate loopback-only publishing after remediation. |
| Credential transport and destination scope; applicable when service credentials are configured | Elastic URI/auth construction and HTTP redirect/header defaults | Findings R63/R64; add isolated synthetic-credential contract checks, without probing public sites. |
| Diagnostic confidentiality; applicable when URLs/errors/data contain secrets | ApiException, TestRunner, Elastic fallback and artifact capture/publication | Finding R62; optional hardening R65. Use synthetic canaries for follow-up verification. |
| HTTPS certificate verification; applicable to HTTPS adapters | No IgnoreHTTPSErrors flag in HTTP adapters; Elastic fingerprint/default policy, loopback-only development exception and existing TLS tests inspected | No new certificate-validation bypass identified; R24 stays closed. Tests were not rerun. |
| Secret sourcing and access; applicable to configured secrets | SettingsLoader precedence, environment/composite/Key Vault providers; bounded pattern scan | No new secret-source defect identified in inspected paths. R1 rotation and R36 live authorization remain not verified. |
| Input/command/SQL safety; applicable to reachable inputs | Database parameters and shell quoting; diagnostic command strings are locally constructed | No reachable remote-input injection established in sampled callers. This does not certify arbitrary SQL/commands supplied by trusted test authors. |
| NuGet dependencies; applicable to resolved .NET packages | Successful direct/transitive advisory query and resolved inventory | No vulnerable package reported by that feed; non-NuGet components not verified. |
| CI permissions and artifact governance; applicable to hosted runs | YAML checkout, execution and publication steps only | Remote permission/fork/retention settings not verified. R65 records the artifact-policy recommendation without alleging public artifact access. |
| Server-side authentication, browser security headers and application authorization | This repository is a test client/framework, not the application server | Not applicable as framework implementation requirements; application-under-test assessment requires separate scope. |

## Security fixes and verification — 2026-11-18

- **Branch / scope:** `fix/security-r61-r65`, created from latest `origin/main` at `758bab4`. The uncommitted six-skill/ledger rules and R60 facade recommendation were carried over. R60 is still open; this pass addresses R61–R65 and does not resume mobile work. The security fixes were committed in `3b2d62a` and merged into main in `c788c61`.
- **Implementation:** Local development port bindings are explicit loopback; remote Elastic connections require HTTPS; HTTP redirects are returned without network replay; framework diagnostics are redacted at sinks/report/error boundaries; sensitive web captures and web CI publishing can be suppressed independently. [Security controls](SECURITY.md) documents intentional behavior changes, scope and operational prerequisites.
- **Build / tests:** The full CI solution build passes with warnings as errors (zero warnings/errors). The initial framework regression passed **252**, skipped **2** live broker cases, failed **0**; the final run, including actual serialized Elastic payload verification, passed **253**, skipped **2**, failed **0** (255 total). The shared headed Chrome UI contracts passed **44/44**. Native execution used the user's existing shell/.NET verification authorization; synthetic security checks targeted only controlled loopback/data pages.
- **Controlled failure evidence:** The isolated headed harness exercised the actual UiTestBase and real Allure writer. Standard mode captured a screenshot and trace; restricted mode collected neither. Both modes disposed owned sessions and browsers. Allure attachment counts were standard=1, restricted=0, and both error JSON records omitted synthetic canaries. The harness first found an unstarted-trace cleanup edge case, fixed by tracking trace startup. Its next attempt assumed Allure filenames followed case UUIDs; correcting only that harness assumption allowed final checks to pass. Generated harness/evidence are ignored, not new production dependencies.
- **Configuration checks:** Parsed Compose YAML confirms loopback-only host bindings. Parsed Azure YAML confirms four web publication tasks are gated and diagnostic capture mode is scoped to the web job; history/download/generation use the same publication parameter. These are static configuration checks, not a successful hosted Azure run or Docker network test.
- **Status / limits:** R62/R63/R64 are closed with the stated local verification. R61 and R65 are implemented with environment verification pending (Docker connectivity and effective Azure publication/access/retention). R36/R58 remain pending and R16/R32/R33 remain blocked. No real secret, external application security probe, live broker/vault test or mobile run was used. No new coverage measurement was collected; the earlier NuGet audit is historical evidence, not rerun here. The first harness restore was stalled by sandbox network lookup and superseded by a successful dependency-cache restore; initial missing import/build errors were corrected before the passing build.
- **Local evidence:** [Summary](../artifacts/security-fixes-2026-11-18/summary.json), [UI contract TRX](../artifacts/security-fixes-2026-11-18/security-ui-r61-r65.trx), [controlled policy results](../artifacts/security-fixes-2026-11-18/policy-results.json). [Final framework TRX](../artifacts/security-fixes-2026-11-18/security-final-r61-r65.trx); artifacts are ignored by Git.

## Commit-specific six-skill review — 2026-11-18 (security fixes)

**Scope:** Exact staged security changes on `fix/security-r61-r65`, based on `758bab4`, including code, tests, Docker/Azure configuration, security guidance, six-skill review rules and the ledger. Each skill's workflow was applied; earlier repository reviews were not treated as the commit review. Subsequent settings/test/ledger edits were rechecked before committing.

| Skill | Outcome | Evidence / remaining limits |
|---|---|---|
| Code review | Found an R62 follow-up: secret resolution under an ordinary key was not registered, while secret-provider collection names were registered accidentally. Corrected registration at resolution and limited plaintext-field inference to the leaf key. No further supported defect identified in the inspected changes. | Traced settings, diagnostic sinks/errors, transport defaults, lifecycle and publication controls. The missing namespace import and trace cleanup issue from implementation were already resolved. |
| Test review | Reviewed actual secret removal, preserved request data, real Elastic serialization, both transport redirect outcomes and controlled capture/report behavior. Added regressions for opaque secrets under ordinary keys and readable provider names. | New tests use unique values and do not mutate shared secret-provider selection. The earlier 253-pass/2-skip framework run, 44 headed UI passes and controlled failure results are reused for unchanged behavior; fresh follow-up result is recorded below. |
| Coverage review | Existing Cobertura reports predate the security changes and cannot establish current line/branch or changed-line coverage. No percentages claimed for this commit. | Behavior evidence and remaining acceptance checks are mapped in the matrix below. TRX success is execution evidence, not instrumentation. |
| Documentation review | Traced security defaults/options and publication conditions to code/YAML, including loss of automatic redirects, restricted capture scope, native-host redaction limits and unchanged mobile job configuration. | Local Markdown targets, stable R1–R65 IDs/anchors/counts and parsed configuration checked. Docker networking and effective Azure permissions/retention remain unverified, explicitly tracked by R61/R65. |
| Test modernization | Researched current official guidance and retained shared transport contracts, synthetic canaries and explicit artifact policy. No new package, second provider or runner migration is justified by this change. | Comparison and decision below; dependencies/runtime targets are unchanged. |
| Security review | Rechecked data flow from secret sources through logs/report errors, HTTP redirect destinations and Elastic transport setup. R62 registration follow-up fixed; no further supported defect identified in this staged scope. | Existing NuGet audit reused because dependency manifests are unchanged; not rerun. R61/R65 environment checks, R36/R58 live checks and the R1 owner rotation follow-up remain unverified. Mobile findings R16/R32/R33 stay blocked. No external attack/probe performed. |

| Behavior / risk | Test evidence | Remaining gap / smallest useful check |
|---|---|---|
| Registered and URL/header secrets | Synthetic-canary unit tests, actual serialized Elastic payload and real Allure error JSON | Fresh tests cover ordinary-key secret resolution/provider-name distinction. Arbitrary private data and binary traces remain outside text redaction; use restricted capture mode. |
| Redirect credential/body replay | Shared tests return the first 301/302/303/307/308 response; no request reaches the second loopback origin | Reuse these contracts for future providers. No live application security probe needed. |
| Capture and publication policy | Unit callbacks and controlled headed UiTestBase/Allure checks; parsed Azure conditions | R65: execute a hosted parameterized run and inspect effective reader/retention settings. |
| Local service isolation | Exact loopback port bindings validated in parsed Compose YAML | R61: localhost/authorized second-host check on a Docker-capable host. |

| Current approach | Alternative / decision | Benefit, effort/risk and acceptance |
|---|---|---|
| Explicit first-response HTTP policy | **Retain** instead of automatic header/body forwarding; reviewed [Microsoft redirect behavior](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.allowautoredirect?view=net-10.0) and [Playwright redirect controls](https://playwright.dev/dotnet/docs/api/class-apirequestcontext#api-request-context-fetch) live on 2026-11-18 | Shared contract now demonstrates no replay. No further migration; a future redirect feature requires explicit destination/method/body acceptance tests. |
| Capture suppression plus independent web publishing parameter | **Retain** instead of trying to rewrite binary trace ZIPs; reviewed [Azure parameter expansion](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/runtime-parameters?view=azure-devops) and [OWASP diagnostic exclusions](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html#data-to-exclude) live | Existing demo defaults preserved. Small local changes verified; hosted verification is still needed under R65 before claiming effective Azure governance. |

**Final commit checks:** The post-review full solution build passed with zero warnings/errors. Fresh settings/redaction tests passed **22/22**, including both new regressions. The earlier **253-pass / 2-skip** framework run, **44/44** headed UI contract result and controlled standard/restricted failure checks are reused for unchanged behavior; they were not rerun as part of this commit review. Formatting completed for the follow-up C# files; whitespace, local file targets and ledger IDs/counts were checked. No current security-diff coverage percentage is available.

**Sequence / stop conditions:** Commit the locally verified fixes and their limits; complete R61's Docker and R65's Azure acceptance checks when those environments are available. Preserve the current policy if a proposed expansion cannot prove credential isolation, capture suppression and trustworthy test failures. No broader modernization or mobile work is authorized by this review.

## Commit-specific six-skill review — 2026-11-18 (AAA comments)

**Scope:** Staged changes against `c788c61` on `main`: Arrange, Act and Assert comments in all **177 test methods across 52 C# files**, plus this ledger update. Reviewed the staged diff and relevant test bodies, web/mobile/API bases, browser fixture, shared contract fixtures, project/package/build files, solution filter and CI suite/coverage wiring. No executable code, assertion, test name, test data, dependency or pipeline changes are intended. Combined action/assertion expressions retain an explicit `Act & Assert` label; intermediate checks and cleanup are identified without moving statements.

| Skill | Outcome | Evidence / limits |
|---|---|---|
| Code review | No new correctness, compatibility or security defect identified in the changed scope. | C# syntax/token comparison verifies unchanged executable tokens; session ownership, callbacks, configuration and dependency boundaries are untouched. |
| Test review | No new assertion, isolation, synchronization or cleanup defect introduced. | Traced representative scenario, unit, exception, shared-contract and integration patterns. Comments preserve setup supplied by fixtures and existing intermediate assertions. All 177 methods have phase annotations; this is static evidence, not test execution. |
| Coverage review | No changed runtime behavior requiring additional tests; no new scenario gap identified for this comment-only change. | One historical Cobertura report exists under `artifacts/review-r54-r59-2026-11-18/`; it predates the security fixes and has no current-diff coverage value. No measured percentage claimed or fresh instrumentation performed. |
| Documentation review | Comments agree with the inspected actions and assertions. Replaced ambiguous historical security-fix references with `3b2d62a` so this commit does not appear to implement R61–R65. | Checked this ledger's stable finding IDs, detail anchors and counts; the historical commit is present locally. Existing documentation and setup instructions are otherwise unchanged. |
| Test modernization | Retain the current framework and explicit AAA comments; no migration or new tool is warranted by this change. | Current primary-source research and the scoped comparison below support a readability improvement without altering test execution. |
| Security review | No new security weakness identified in the staged comments/ledger. | Inspected added text for sensitive values and checked unchanged code tokens and manifests. This is not a new dependency audit, external scan or compliance assessment; applicability is recorded below. |

**Coverage / behavior matrix:**

| Behavior or risk | Evidence | Gap / smallest useful check |
|---|---|---|
| Test phase readability | All 177 Fact/Theory/custom-Fact method bodies are annotated; representative nested and combined phases inspected. | None identified for this change; check phase accuracy when behavior is changed later. |
| Assertions and execution order | Syntax/token equality against the pre-change source, including attributes and literal values. | No new runtime scenario; no additional test needed for comments. |
| Whole-framework coverage and live integrations | Historical reports and existing R36/R58/R61/R65 acceptance criteria. | Unchanged pending checks remain; this commit does not close them or establish current measured coverage. |

**Modernization comparison and sequence:** Research checked live on **2026-11-18** against [Microsoft's .NET unit-testing guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#arrange-your-tests), which describes AAA as a readability aid. The installed .NET 10 / xUnit 2.9.3 / runner 2.8.2 / Playwright 1.64.1 package selections are unchanged; no upgrade or support-status claim is made.

| Current approach | Alternative / decision | Benefit, effort/risk and acceptance |
|---|---|---|
| Existing assertions and workflows with explicit phase comments | **Retain** instead of extracting/reordering every inline assertion solely for three visually separate sections. | Makes intent visible with low implementation risk; acceptance is phase annotation completeness and unchanged C# tokens. |
| Existing shared contracts, page objects and fixtures | **Retain** for this commit; R60 remains the separately tracked facade recommendation. | No new migration or pilot follows from comments. First commit the annotated tests; reconsider structure only in authorized behavior/design work. Stop and reassess if a proposed comment edit requires changing execution. |

**Security control applicability:** The relevant changed asset is source documentation. [OWASP logging guidance](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html#data-to-exclude), checked live on **2026-11-18**, supports excluding sensitive values from diagnostic text; no standard control ID or compliance mapping is asserted here.

| Control / boundary | Outcome | Evidence / next check |
|---|---|---|
| Sensitive data in added source comments and review text | No issue identified in inspected additions. | Phase descriptions introduce no credentials, tokens or payload dumps. Existing diagnostic test canaries and data are unchanged. |
| HTTP/TLS, secret resolution, SQL/process inputs, session cleanup and artifact capture | No behavior change; controls not re-certified. | Identical C# tokens and no configuration changes. Existing R61/R65 environment acceptance checks remain pending. |
| Dependencies, package provenance and current advisories | No dependency changes; fresh advisory scan not performed. | Project and central package manifests are unchanged. No claim that dependencies are currently vulnerability-free. |
| Effective CI identity permissions, artifact readers/retention and live services | Not verified; outside this local comment change. | R36/R58/R61/R65 remain verification pending; mobile R16/R32/R33 remain blocked. Server-side application authentication controls are not applicable to these comments. |

**Verification and residual state:** Static C# syntax/token and annotation checks passed for 177 methods in 52 files; whitespace checks passed. `dotnet format TestAutomationPlatform.slnx --no-restore --include Tests` completed successfully (the initial sandbox attempt could not bind its build-host pipe; the permitted retry succeeded). Final staged consistency checks confirm the same comments-only C# scope and valid ledger links, IDs and counts. No tests were executed for this comment-only change, and prior test passes are not presented as a fresh run. Native xUnit/coverage execution is unnecessary for unchanged code and was not substituted with browser checks. No new R-number is warranted: **65 total findings, 57 closed, 1 open (R60), 3 mobile-blocked and 4 verification-pending**, unchanged by this review.

## Historical status snapshot — 2026-11-16

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

## Historical coverage snapshot — 2026-11-16

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

<a id="r61"></a>
### R61. Local logging stack publishes unauthenticated ports beyond loopback

- **Status / type:** Implemented (verification pending), 2026-11-18; security fixes in `3b2d62a` on `fix/security-r61-r65`. Original `security-review` assessment scope is preserved above.
- **Implementation:** Compose now explicitly binds both ports to 127.0.0.1. Parsed YAML asserts the exact Elasticsearch/Kibana bindings.
- **Verification / remaining checks:** Docker is unavailable here. Verify the resolved Compose configuration and localhost/authorized second-host connectivity on a Docker-capable host before closing.
- **Where:** `docker-compose.yml:14`, `:23` and `:36`.
- **Original trigger / boundary / consequence:** Starting the documented local stack disables Elasticsearch authentication but publishes 9200 and 5601 without a host IP. With Docker's normal binding defaults, another machine able to reach the host ports can access the unauthenticated logging services, including stored results and available administrative operations. Host firewall/daemon settings can mitigate exposure but were not inspected; no running deployment was probed.
- **Existing mitigation:** The file labels this stack local-development-only. That comment does not enforce network isolation.
- **Fix:** Explicitly bind published ports to loopback (for example `127.0.0.1:9200:9200` and `127.0.0.1:5601:5601`). Require an authenticated/TLS deployment for deliberate shared access rather than relying on the development stack.
- **Acceptance / verification:** Inspect resolved Compose bindings and, on a Docker-capable host, confirm localhost access still works while another authorized host cannot connect. No Docker runtime check was performed here.
- **Primary reference:** [Docker publishing behavior and loopback bindings](https://docs.docker.com/engine/network/port-publishing/) states that unspecified host addresses normally publish on all interfaces.

<a id="r62"></a>
### R62. Diagnostic sinks expose credential-bearing URLs and exception messages

- **Status / type:** Closed, 2026-11-18; security fixes in `3b2d62a` on `fix/security-r61-r65`. Original `security-review` assessment scope is preserved above.
- **Implementation:** Added DiagnosticRedactor for URI user information, query values, fragments, common credential fields/headers and registered configuration secrets. API errors, fallback reasons, console/Elastic messages and metadata, report-step failures and native host failure boundaries sanitize diagnostics. Secret-bearing failures have no raw inner exception; requests/responses are not altered.
- **Commit-review follow-up (`code-review`, `security-review`, `test-review`):** Register values at the secret-provider resolution boundary even when the destination key is ordinary; avoid classifying provider-list names as secret values. Added two regressions.
- **Verification / remaining checks:** Synthetic-canary unit tests passed for URI/JSON/header/registered-secret redaction, idempotence, metadata and the host failure. Controlled headed failures showed sanitized console output, and real Allure result JSON omitted the canary in standard/restricted modes. The final 253-pass framework run also verified the actual serialized Elastic request payload excludes synthetic secrets.
- **Where:** `Core/Api/ApiException.cs:5`, `Core/Logging/Elastic/ElasticConnectivity.cs:104`, `Core/Testing/TestRunner.cs:48`, `Core/Logging/Console/ConsoleLoggingService.cs:31`; Elastic logging/results also persist message/reason fields.
- **Original trigger / boundary / consequence:** If an API endpoint contains sensitive query parameters or a configured Elastic URL contains user information, these strings enter exception/fallback messages verbatim. TestRunner logs exception messages and publishes the reason without sanitization, allowing credentials to cross into console/build logs and configured log/result indexes whose readers may have broader access. This is supported by the string construction and sink path, not by an assumption that current public demo credentials are production secrets.
- **Existing mitigation:** Credentials can be supplied through environment/Key Vault providers and default demo endpoints do not require sensitive query strings. Those mechanisms do not redact a secret after it enters a message.
- **Fix:** Use safe URI formatting that removes user information and redacts sensitive query values; centralize redaction at framework diagnostic boundaries for messages/metadata and review native error propagation into test reports. Retain enough sanitized route/status context for debugging. Do not assume a console sanitizer alone removes secrets from Allure/TRX or attached artifacts.
- **Acceptance / verification:** With synthetic canaries only, force failed API/logging paths containing sensitive URL components and verify that console, metadata, Elastic payloads and published report errors omit the canaries while preserving useful diagnostics. These checks are proposed, not executed.
- **Primary references / mapping:** [OWASP logging exclusions](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html#data-to-exclude), [CWE-532](https://cwe.mitre.org/data/definitions/532.html). Artifact content/policy is tracked separately in R65.

<a id="r63"></a>
### R63. Remote Elastic endpoints accept Basic credentials over HTTP

- **Status / type:** Closed, 2026-11-18; security fixes in `3b2d62a` on `fix/security-r61-r65`. Original `security-review` assessment scope is preserved above.
- **Implementation:** ElasticClientFactory rejects non-HTTPS remote endpoints before constructing a client and rejects URL user information. HTTP remains allowed on loopback; ordinary HTTPS/fingerprint validation remains intact.
- **Verification / remaining checks:** Constructor tests passed for rejected remote HTTP (cloud and default insecure profiles), rejected URL credentials and accepted loopback HTTP/remote HTTPS configuration; existing TLS tests passed in the framework run. No real remote endpoint was contacted.
- **Where:** `Core/Logging/Elastic/ElasticClientFactory.cs:33`, `:38` and `:49`; `Core/Logging/Elastic/ElasticConnectivity.cs:97` uses the same factory for its startup probe.
- **Original trigger / boundary / consequence:** The cloud profile accepts an arbitrary configured scheme, and the default insecure profile also accepts a non-loopback URL. When username/password are set with a remote `http://` endpoint, the factory applies Basic authentication without requiring TLS. Credentials can traverse the network in recoverable form, including during the initial probe. HTTPS certificate validation from R24 does not protect an HTTP connection.
- **Existing mitigation:** The secure-local profile rewrites HTTP to HTTPS; correctly configured HTTPS endpoints retain default/fingerprint validation. The no-auth local development stack need not send credentials.
- **Fix:** Reject non-HTTPS authenticated remote endpoints before creating/probing a client. Constrain any intentional plaintext development exception to explicit loopback use and document it; preserve normal TLS/fingerprint behavior.
- **Acceptance / verification:** Isolated construction/probe checks should reject authenticated remote HTTP without sending any request, allow a supported no-auth loopback setup and preserve HTTPS trust-policy tests. Use synthetic credentials; no remote endpoint was tested.
- **Primary reference:** [RFC 7617 security considerations](https://www.rfc-editor.org/info/rfc7617/) explains that Basic authentication does not protect passwords without transport protection such as HTTPS. This is separate from the closed certificate-validation finding R24.

<a id="r64"></a>
### R64. Manual authentication headers can cross redirect destinations

- **Status / type:** Closed, 2026-11-18; security fixes in `3b2d62a` on `fix/security-r61-r65`. Original `security-review` assessment scope is preserved above.
- **Implementation:** HttpClient AllowAutoRedirect=false and Playwright MaxRedirects=0 return the first 3xx response. The common contract/security guide documents the explicit policy for same-origin requests as well.
- **Verification / remaining checks:** Fourteen shared loopback redirect cases passed across both adapters: 301/302/303/307/308, manual Cookie/Authorization/custom key headers in defaults and requests, synthetic bodies, same-origin targets and loops. No request reached the second origin.
- **Where:** `Domain/Api/RestfulBooker/AuthClient.cs:20`; `Core/Http/HttpClientTransport.cs:14` and `:34`; `Core/Http/Playwright/PlaywrightHttpTransport.cs:69`–`:76`.
- **Original trigger / boundary / consequence:** Restful Booker authentication is a manually supplied Cookie header. Both transports retain automatic redirect behavior with no framework origin policy. HttpClient does not clear manually supplied Cookie headers on redirects; Playwright documents request headers applying to redirected requests. If a trusted endpoint returns a redirect to another destination, authentication material can be sent outside the intended origin. No claim is made that the current demo application provides an exploitable redirect or that .NET forwards Authorization identically to Cookie.
- **Existing mitigation:** Original requests normally use configured HTTPS demo URLs. TLS protects each connection but does not authorize a different redirect destination to receive the original credentials.
- **Fix:** Define a shared safe redirect policy, preferably disabling automatic redirects for credential-bearing requests or handling redirects explicitly with origin/scheme checks and sensitive-header removal. Do not forward request bodies or authentication material to untrusted destinations; preserve intentional same-origin behavior with bounded redirects.
- **Acceptance / verification:** Shared HTTP contracts using two controlled local origins should verify that synthetic Cookie/Authorization/custom secret headers never reach an unauthorized destination. Cover same-origin redirects, loops and any supported method/body replay. Do not probe public demo targets for this behavior.
- **Primary references:** [.NET 10 AllowAutoRedirect remarks](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.allowautoredirect?view=net-10.0), [Playwright FetchAsync header/redirect options](https://playwright.dev/dotnet/docs/api/class-apirequestcontext#api-request-context-fetch). The documented Playwright options predate installed version 1.64.1.


<a id="r54"></a>
### R54. Database setup and disposal failures can leak connections

- **Status:** Closed, 2026-11-18; verified in `572cad6` on `fix/review-r54-r59`.
- **Implementation:** Connection setup and transaction startup now dispose the acquired connection on failure without masking the setup exception. Scope disposal attempts rollback, transaction disposal and connection disposal independently and rethrows the first failure.
- **Verification completed:** Ten new DbFailureTests cases passed (setup/transaction failures and cancellation, disposal failure combinations, committed cleanup), alongside the existing SQLite tests in the 217-pass framework run.
- **Where:** `Core/Data/DbConnectionFactory.cs:18` and `:26`; `Core/Data/DbTestScope.cs:50`.
- **Original problem / trigger:** A connection is created but not disposed if assigning its connection string or opening it fails; an opened connection is also retained if beginning its transaction throws or is cancelled. During scope cleanup, a transaction-disposal exception prevents connection disposal. Repeated failures can retain provider resources or exhaust a connection pool.
- **Fix:** Retain ownership until each successful handoff. Dispose partially acquired resources on setup failure, attempt transaction and connection cleanup independently, and preserve the original/first failure.
- **Verify:** Inject failures in connection setup/open, transaction creation, rollback and disposal. Assert that all acquired resources receive disposal and that later cleanup errors do not replace the first failure; retain the normal SQLite rollback/commit checks.

<a id="r55"></a>
### R55. Selenium role locators differ from Playwright semantics

- **Status:** Closed, 2026-11-18; verified in `572cad6` on `fix/review-r54-r59`.
- **Implementation:** Replaced hand-built accessible-name inference with WebDriver computed role/label commands. Role candidates are filtered by actual role and ARIA/rendering visibility; explicit overrides, associated labels and aria-labelledby precedence are respected. Transparent elements remain eligible for role lookup.
- **Verification completed:** Shared headed Chrome contracts cover associated labels, aria-labelledby precedence, hidden/ARIA-hidden duplicates, explicit role overrides and transparent role matches against both adapters; see the recorded UI verification results.
- **Where:** `Core/Ui/Selenium/WebDriverLocate.cs:20` and `:30`; compare `Core/Ui/Playwright/PlaywrightElement.cs` role resolution and `Core/Ui/Locate.cs` accessible-name contract.
- **Original problem / trigger:** Selenium's CSS role query includes hidden matches and approximates accessible names from aria-label, text, value or an ancestor label. It misses ordinary associated labels (`<label for="email">Email</label><input id="email">`) and `aria-labelledby`. Switching providers can make an otherwise valid role locator miss its element or become ambiguous because of hidden duplicates.
- **Fix:** Define equivalent accessible-name and hidden-element semantics across providers and implement them for Selenium. Keep the intentional strict ambiguity rule; do not hide the discrepancy by always choosing First.
- **Reference:** [Playwright role locators](https://playwright.dev/dotnet/docs/api/class-locator#locator-get-by-role) match accessible names and exclude ARIA-hidden elements by default; these capabilities predate the installed 1.64.1 version.
- **Verify:** Add shared provider contracts for associated labels, aria-labelledby and visible/hidden role duplicates, and confirm the same intended element is selected by both adapters.

<a id="r56"></a>
### R56. Selenium CheckAsync can succeed without checking the element

- **Status:** Closed, 2026-11-18; verified in `572cad6` on `fix/review-r54-r59`.
- **Implementation:** Selenium validates native checkbox/radio or ARIA checkbox/radio targets, preserves already checked state and verifies the state after a click. Unchecked disabled controls wait until enabled; a prevented state change throws UiException.
- **Verification completed:** Shared contracts cover normal checking, disabled already-checked no-op, invalid targets without clicks, prevented native changes, repeated ARIA checks and prevented ARIA changes against both adapters; see the recorded UI verification results.
- **Where:** `Core/Ui/Selenium/WebDriverElement.cs:42`; compare `IElement.CheckAsync` and the Playwright adapter.
- **Original problem / trigger:** The action clicks any unselected element and returns without checking its type or final selected state. A checkbox whose click handler prevents the state change can therefore produce a successful action while remaining unchecked; a wrong element can also receive a click rather than a checkbox error.
- **Fix:** Validate supported checkbox targets, retain no-op behavior for an already checked checkbox, and verify the checked postcondition after interaction. Return a framework failure when checking did not succeed.
- **Reference:** [Playwright CheckAsync](https://playwright.dev/dotnet/docs/api/class-locator#locator-check) validates the target and checked state after clicking.
- **Verify:** Add shared contracts for an already checked checkbox, a normal unchecked checkbox, a prevented state change and an invalid target. Require equivalent success/failure outcomes and preserve framework exception mapping.

<a id="r57"></a>
### R57. HTTP providers resolve base URLs without trailing slashes differently

- **Status:** Closed, 2026-11-18; verified in `572cad6` on `fix/review-r54-r59`.
- **Implementation:** Playwright now applies the same directory-base convention as HttpClient: append a trailing slash when absent. The factory contract and provider guide document relative, root-relative and absolute resolution.
- **Verification completed:** Sixteen added shared loopback cases passed across both transports, covering trailing/non-trailing path bases, parent-relative and root-relative endpoints, and an absolute URL overriding an unreachable base host.
- **Where:** `Core/Http/HttpClientTransport.cs:16`; `Core/Http/Playwright/PlaywrightHttpTransport.cs:30`; `Tests/Unit/Http/HttpTransportContractTests.cs` relative-URL test.
- **Original problem / trigger:** HttpClientTransport appends a trailing slash to the base URL, while Playwright preserves it. For base `https://host/api` and endpoint `items`, the providers resolve `/api/items` and `/items` respectively. Changing the configured provider can send API calls to a different route. The existing shared test only exercises a base ending in `/`.
- **Fix:** Choose and document one resolution policy, apply it consistently in both factories/transports, and retain absolute-URL handling.
- **Reference:** [Playwright base URL resolution](https://playwright.dev/dotnet/docs/api/class-apirequest#api-request-new-context) explicitly distinguishes a path base with and without a trailing slash.
- **Verify:** Extend loopback contracts with trailing/non-trailing path bases and relative/root-relative/absolute endpoints; assert identical server-observed request paths for both providers.


<a id="r49"></a>
### R49. Selenium silently chooses among ambiguous element matches

- **Status:** Closed, 2026-11-18, verified and committed in `28b32bf` on `fix/framework-adapter-contracts`.
- **Implementation:** Selenium now requires an unambiguous match before checking enabled/visible state. Single-element actions, text/visibility reads and scalar assertions reject multiple matches; collection operations and explicit First remain supported. The contract is documented in IElement/IElementAssertions.
- **Where:** `Core/Ui/Selenium/WebDriverElement.cs:85` (`ActAsync`); compare `Core/Ui/Playwright/PlaywrightElement.cs:20`.
- **Original problem:** When a locator matches multiple actionable elements, the Selenium adapter chooses the first displayed/enabled candidate. The Playwright adapter delegates to a strict locator and rejects ambiguous action targets. Switching the configured driver can therefore turn a selector error into a click or fill on an unintended element. The shared UI contract does not make this difference explicit, and the current contract suite does not exercise ambiguous matches.
- **Fix:** Define a consistent single-element action policy across adapters. Prefer rejecting ambiguous matches and requiring the existing `First` operation when selection of the first match is intentional. Align applicable single-element assertions as well.
- **Verification:** The headed Chrome driver contract run passed 28/28 for Playwright and Selenium. New shared cases cover duplicate click/fill targets, hidden/disabled duplicate matches, scalar reads/assertions, list/count operations and intentional First selection.
- **Reference:** [Playwright locator strictness](https://playwright.dev/dotnet/docs/locators#strictness).

<a id="r50"></a>
### R50. Playwright HTTP transport ignores cancellation

- **Status:** Closed, 2026-11-18, verified and committed in `28b32bf` on `fix/framework-adapter-contracts`.
- **Implementation:** Playwright HTTP requests now own independent native request contexts. Pre-cancelled tokens are checked before context/request creation; in-flight cancellation aborts and drains the owned native operation. Disposing a transport cancels its active contexts without poisoning another request or later reuse after per-request cancellation. Default headers are snapshotted; implicit cookie jars are not retained, matching HttpClientTransport.
- **Where:** `Core/Http/Playwright/PlaywrightHttpTransport.cs:8` (`SendAsync`); compare `Core/Http/IHttpTransport.cs` and `Core/Http/HttpClientTransport.cs`.
- **Original problem:** The Playwright implementation accepts a CancellationToken but never checks or observes it. A request can reach the server even when its token is already cancelled, and cancellation during a request does not interrupt the operation. HttpClientTransport passes the token to send/body-reading operations, so cancellation behavior changes when the transport provider is switched.
- **Fix:** Honor pre-cancelled tokens before starting a request, and define and implement in-flight cancellation and resource cleanup for the Playwright transport. Do not report cancellation while silently leaving an unowned operation or response behind.
- **Verification:** The framework suite passed 191 tests with 2 environment-gated broker skips. Both HTTP implementations passed shared tests for pre-cancelled POST/no server request, cancellation during a gated response, concurrent/future request success and cancellation on transport disposal.

<a id="r51"></a>
### R51. RabbitMQ startup failures leak partially created resources

- **Status:** Closed, 2026-11-18, verified and committed in `28b32bf` on `fix/framework-adapter-contracts`.
- **Implementation:** RabbitMQ startup retains ownership of partial resources and disposes them on every startup exception. Cleanup independently attempts channel close/dispose and connection close/dispose, preserving the first error; startup cleanup cannot mask the original startup exception.
- **Where:** `Core/Messaging/RabbitMq/RabbitMqListener.cs:36` (`StartAsync`) and `:64` (`DisposeAsync`).
- **Original problem:** The connection, channel and temporary queue are acquired before startup completes, with no failure cleanup. If channel creation, queue declaration/binding or BasicConsumeAsync fails (for example, a missing exchange causes binding to fail), StartAsync throws without returning a listener the caller can dispose. The connection and any surviving exclusive queue can remain allocated. During normal disposal, a channel-close exception also skips connection cleanup.
- **Fix:** Keep ownership of partially created resources until startup succeeds and release them on every failure path. Use nested try/finally or equivalent cleanup during disposal so a channel failure cannot prevent connection disposal.
- **Verification:** Six lifecycle regression cases passed using controlled vendor-interface doubles: channel creation, queue declaration, binding and consumer-start failure; cancelled startup; and channel close/dispose failure while still releasing the connection. No live RabbitMQ broker was configured; its separate integration test was skipped.

<a id="r52"></a>
### R52. Valid duplicate Kafka headers can fault the listener and bypass cleanup

- **Status:** Closed, 2026-11-18, verified and committed in `28b32bf` on `fix/framework-adapter-contracts`.
- **Implementation:** Kafka repeated header keys use an explicitly documented last-value-wins projection; null values become empty strings. Fatal consumption errors are observable through Completion and WaitForAsync. Dispose always attempts consumer close/dispose and cancellation-source disposal even when the worker or close operation fails, preserving the first failure.
- **Where:** `Core/Messaging/Kafka/KafkaListener.cs:90` (`ConsumeLoop`) and `:103` (`DisposeAsync`); `Core/Messaging/ReceivedMessage.cs`.
- **Original problem:** Kafka allows repeated header keys, but the listener converts headers directly with ToDictionary. A message with duplicate keys throws ArgumentException, which the KafkaException catch does not handle, terminating the background loop. DisposeAsync then awaits the faulted loop before closing/disposing the consumer and cancellation source, so cleanup is skipped as well. Existing integration coverage uses only a single unique header.
- **Fix:** Define how the framework represents repeated header keys (preserve all values or document an explicit reduction policy), and handle nullable header values consistently. Ensure worker faults are observable and that consumer/cancellation resources are released in finally even when the worker fails.
- **Verification:** Two worker regression cases passed using controlled vendor-interface doubles: duplicate/null headers followed by successful receipt of a later message, and fatal worker failure with prompt wait failure and consumer cleanup even when Close throws. No live Kafka broker was configured; its separate integration test was skipped.
- **Reference:** [Confluent message metadata](https://docs.confluent.io/platform/current/clients/confluent-kafka-dotnet/_site/api/Confluent.Kafka.MessageMetadata.html) documents that duplicate header keys are allowed.

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

- **Status:** Blocked (mobile work deferred), 2026-11-18, at the user's request.
- **Blocker / scope:** Further mobile framework and hosted emulator investigation/verification is deferred for now. Existing implementation and evidence are retained; this finding is not closed.
- **Resume when:** The user resumes mobile work; continue from the recorded implementation, run history and outstanding verification below.
- **Prior status / evidence:** Implemented (verification pending), 2026-11-17, in `c1e23a8`. Configurable UiAutomator2 timeouts (CI: 120 s); the CI script waits for background compilation, turns off adb install verification and pre-installs Appium's 3 helper apps (timed in `boot-progress.log`). Unit tests for the capability mapping; the script block was checked locally under `set -euo pipefail` with a fake `adb`. **Verify:** a manual run whose smoke test passes. **Run 118 (manual, 2026-11-17):** the fix worked (helpers pre-installed in 19/34/9 s; Appium found them, `sameVersionInstalled`, and started UiAutomator2 in 30 s; boot took 446 s). The session then failed at the next step: Appium auto-downloaded Chromedriver for the device's Chrome 113 and was starting Chrome for the first time when the .NET client's 120 s command timeout (`Server.CommandTimeoutSec`) expired. **Next (parked by the owner on 2026-11-17):** raise `MOBILE_Server__CommandTimeoutSec` on CI and pre-download Chromedriver 113 (`Android.ChromedriverExecutable`). **Follow-up `41d50c8` (2026-11-17, owner asked to resume):** the CI script pre-downloads the Chromedriver matching the device's Chrome, pre-warms Chrome with first-run screens disabled, and raises the client timeout to 300 s (checked locally with a fake `adb` for Chrome 113/131/unknown). **Run 130 (manual, 2026-11-17, `6694a9b`):** boot 405 s, helpers pre-installed, Chromedriver 113.0.5672.63 pre-downloaded for Chrome 113.0.5672.136, warm-up 204 s. The session then failed with `chrome not reachable`: Chromedriver **cleared Chrome's app data** before launching it (its default unless `androidKeepAppData` is set), which threw away the warm-up; the cold Chrome start (from 10:49:46) wasn't answering after about 70 s and Chromedriver gave up. The emulator was also overloaded (35 ANRs, mostly Google services and `com.android.phone`). **Follow-up:** `goog:chromeOptions.androidKeepAppDataDir` follows `Behavior.NoReset` (default true), so the warmed-up Chrome is reused; unit-tested. PR #35 first used `androidKeepAppData`, which Chromedriver rejects (`unrecognized chrome option`), caught by a local run before CI; corrected on `fix/mobile-keep-chrome-data-main` (replaces PRs #35/#36). **Local check (2026-11-17, Apple Silicon, Appium 3.6 + UiAutomator2 8.7.3, Android 14 arm64 emulator, Chrome 113.0.5672.136):** emulator booted in 49 s, smoke test passed in 11 s, device log shows no Chrome data clear. **Verify:** a manual run where the smoke test passes; that closes R33, R32 and R16.
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

- **Status:** Blocked (mobile work deferred), 2026-11-18, at the user's request.
- **Blocker / scope:** Further mobile framework and hosted emulator investigation/verification is deferred for now. Existing implementation and evidence are retained; this finding is not closed.
- **Resume when:** The user resumes mobile work; continue from the recorded implementation, run history and outstanding verification below.
- **Prior status / evidence:** Implemented (verification pending), 2026-11-17, in `be414c3` and a follow-up on `fix/emulator-boot-progress`. Run 110 (manual, 2026-11-17) did **not** verify it: (1) the new 30-second progress report exited the script silently under `set -e`/`pipefail` when `adb` failed while the device was still offline, so the boot never got its 600 s (fixed: those `adb` calls can no longer fail the script); (2) the emulator still set 1 vCPU (`less than 6 logical cores`) despite `-cores 2` and `hw.cpu.ncore=2`, so on hosted macOS the core count can't be raised (those settings were removed). All software prerequisites were present (.NET 10.0.404, Node 22.23.2, Temurin 17, Appium 2.19.0 + UiAutomator2 4.2.9, SDK/image/AVD, Hypervisor.Framework). **Verify:** the next manual run should show `[boot]` progress lines and either "Android booted in N seconds" or how far it got in 600 s. If 1 core isn't enough, next options: a lighter image (API 30/31 `google_apis`), a self-hosted agent with more cores, or a device cloud. **Runs 115/118:** booted in 505 s / 446 s. **Run 127 (manual, 2026-11-17, `6e774e7`):** device online at 154 s and boot animation done at 186 s, but `sys.boot_completed` never reached 1 within 600 s, so the R33 Chrome steps never ran. Follow-up on `fix/mobile-boot-timeout`: the pipeline sets `BOOT_TIMEOUT_SECONDS=900` and the job limit is 45 minutes. **Verify:** two manual runs that boot and pass the smoke test close R32, R33 and R16.
- **Where:** `scripts/unix/run_mobile_ci.sh` (emulator start and boot wait), `azure-pipelines.yml` (`MobileSmoke`).
- **Problem:** the first hosted run (run 108, nightly 2026-11-17) failed with `Emulator failed to boot within 300 seconds`. The `mobile-results` logs show it isn't a missing-acceleration problem: `acceleration.log` reports Hypervisor.Framework available on the Intel (`osx-x64`) `macos-15` agent, and the emulator ran for the whole 300 s. It was too slow: `emulator.log` says `Running on a system with less than 6 logical cores. Setting number of virtual cores to 1`, rendering is all on the CPU (`-gpu swiftshader`), the emulator window is still created (no `-no-window`), and every run is a cold boot of a heavy Android 14 `google_apis` image. Knock-on: `Publish Mobile Test Results` failed because no TRX was written. Also `NodeTool@0` is deprecated (`UseNode@1`).
- **Fix:** start headless with lighter rendering (`-no-window -gpu swiftshader_indirect`), give the guest 2 cores and more RAM (`-cores 2 -memory 3072`, and `hw.cpu.ncore=2` in the AVD config), allow a longer boot (600 s), log boot progress, and switch to `UseNode@1`. If that still isn't enough, use a lighter system image.

---

## P3: Good practice

<a id="r65"></a>
### R65. Define an artifact sensitivity and publication policy

- **Status / type:** Implemented (verification pending), 2026-11-18; security fixes in `3b2d62a` on `fix/security-r61-r65`. Original `security-review` assessment scope is preserved above.
- **Implementation:** Added DIAGNOSTIC_ARTIFACTS standard/restricted mode. The shared web base suppresses screenshots/tracing before capture; built-in reporters suppress attachments. Added web-only Azure capture/publication parameters, security guidance and diagrams. Mobile job configuration remains unchanged.
- **Verification / remaining checks:** Controlled headed failures passed through the real web base: standard captured one screenshot/trace and two attachments; restricted started no trace, took no screenshot and attached nothing. Both disposed owned sessions/browsers. Real Allure tests produced one standard attachment, zero restricted attachments and sanitized failure JSON. YAML publication guards were checked statically. Effective Azure access/retention and a hosted parameterized run remain unverified, so this finding stays verification pending.
- **Where:** `Core/Ui/Playwright/PlaywrightUiSession.cs:34`, `Tests/Infrastructure/UiTestBase.cs:86` and `:120`, `Core/Reporting/Allure/AllureTestReporter.cs:34`, `azure-pipelines.yml:158` and `:200`.
- **Original observation / rationale:** Failure capture collects full-page screenshots and traces with screenshots, DOM snapshots and sources; Allure attaches the raw bytes and CI publishes raw results and generated reports. For a future authenticated/non-demo application, captured page/network content may include sensitive data that message redaction in R62 cannot address. Actual artifact reader permissions and retention were not inspected, so their adequacy is unverified rather than assumed insecure.
- **Existing mitigation:** Trace modes include off/on-failure/on; tracing defaults to attaching on failure. Generated output is ignored by Git and temporary trace ZIPs are deleted. These controls do not govern copies already retained in reports or CI artifacts.
- **Recommendation / tradeoff:** Add an explicit capture/publication policy for sensitive suites: configurable screenshot masking or suppression, trace/source capture choices, restricted publication and documented access/retention. Preserve useful diagnostics for normal demo tests. Treat raw traces as sensitive rather than attempting fragile string replacement inside trace ZIPs, and verify effective Azure policies before changing permissions.
- **Acceptance / verification:** An authorized controlled run with synthetic sensitive data should show that the selected policy excludes/masks it, normal failures retain the intended artifacts and owned browsers clean up. Record actual artifact access/retention settings separately. No such run or permission change was performed in this review; mobile work stays deferred.
- **Primary references:** [Playwright trace contents](https://playwright.dev/dotnet/docs/trace-viewer#network), [OWASP logging data exclusions](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html#data-to-exclude).


<a id="r60"></a>
### R60. Move scenario orchestration from page objects to a facade workflow API

- **Status:** Open, 2026-11-18. Owner-requested architecture improvement; no migration implemented or runtime benefit measured.
- **Source / intent:** The owner requested a review comment to move from Page Object Model to the Facade design pattern. Move the scenario-facing API toward business workflows through a bounded facade pilot; retain useful page/component objects as implementation details during the migration.
- **Where / local evidence:** `Tests/UI/Web/SauceDemo/CheckoutFlowTests.cs` directly coordinates inventory, cart, checkout information, overview and completion pages. `Pages/SauceDemo/InventoryPage.cs`, `CartPage.cs`, `CheckoutInformationPage.cs` and `CheckoutOverviewPage.cs` encapsulate individual page interactions but expose navigation steps to the scenario. `Tests/UI/Web/SauceDemo/SauceDemoTestBase.cs` already shares login setup; compare that existing helper before adding another abstraction.
- **Motivation / consequence:** Multi-page workflow knowledge currently lives in the test. As more scenarios reuse checkout, changes to navigation could require edits across those scenarios. This is a maintainability opportunity, not evidence that the current POM is incorrect or that workflow duplication is already widespread.
- **Proposed change:** Pilot a focused `CheckoutFacade` in a scenario-support folder such as `Tests/Workflows/SauceDemo/`. Let it coordinate the existing driver-neutral pages and expose business operations such as preparing a cart, submitting checkout information and completing an order. Keep app-specific orchestration out of generic Core and preserve the Domain/Pages dependency direction. Return observable state or a small result model so tests retain explicit business assertions and can inspect intermediate outcomes. Preserve `RunAsync`, report steps, failure artifacts and per-test session ownership; the facade must not create shared browser state, swallow failures or add blanket retries.
- **Design rationale / alternatives:** A facade supplies a simpler interface over collaborating objects; page objects encapsulate UI mechanics. These responsibilities can coexist, so deleting all page objects is not an acceptance criterion. Compare the pilot with keeping the current small scenario or extracting a narrow helper. Avoid one application-wide facade that accumulates every feature, and keep lower-level page access available for tests specifically checking an intermediate UI state. References checked 2026-11-18: [Facade intent and tradeoffs](https://refactoring.guru/design-patterns/facade), [Fowler on Page Object responsibilities and layering](https://martinfowler.com/bliki/PageObject.html).
- **Migration / effort:** Small–medium pilot: introduce one checkout facade, migrate the successful checkout scenario and compare readability, repeated workflow code, assertion visibility and failure diagnostics. Broader adoption depends on that comparison. Do not change mobile tests while R16/R32/R33 are deferred.
- **Acceptance / verification:** The migrated scenario expresses business intent without constructing the intermediate checkout pages. Existing cart/content and order-completion assertions remain meaningful; intermediate checkpoints remain possible. Run it through both configured web adapters when authorized, and verify a controlled failure retains useful report steps and artifacts. Reject or revise the pilot if it hides assertions, changes tested behavior, weakens isolation, or merely relocates an equally complex navigation script without a useful API. Record actual results before closing this finding; no tests were run for this documentation addition.


<a id="r58"></a>
### R58. RabbitMQ queue-removal test accepts unrelated exceptions

- **Status:** Implemented (verification pending), 2026-11-18; implemented in `572cad6` on `fix/review-r54-r59`.
- **Implementation / evidence:** The passive-declaration check now requires OperationInterruptedException and broker reply code 404. The solution builds with warnings as errors. The live RabbitMQ case was skipped in the framework run because RABBITMQ_URI is unset; neither Docker nor rabbitmq-server is available locally.
- **Remaining verification:** Run the integration case against a broker and confirm missing-queue, existing-queue and unrelated-channel-error outcomes. This item is not closed by compilation or the skipped test.
- **Where:** `Tests/Unit/Messaging/RabbitMqListenerIntegrationTests.cs:39`.
- **Problem / trigger:** `Assert.ThrowsAnyAsync<Exception>` treats every failed passive queue declaration as proof that the queue disappeared. An unrelated error on the checking channel can satisfy the assertion even when queue removal has not been established.
- **Fix:** Assert the broker's specific missing-queue protocol response and let unrelated channel/connection errors fail the check.
- **Verify:** Run against a live broker: the removed queue produces the expected not-found response, an existing queue fails the removal assertion, and an unrelated checking-channel failure is not accepted as successful verification.

<a id="r59"></a>
### R59. Setup and dependency documentation contradicts current implementation

- **Status:** Closed, 2026-11-18; verified in `572cad6` on `fix/review-r54-r59`.
- **Implementation:** Corrected README, GETTING_STARTED and ARCHITECTURE to describe configured scheduled/manual mobile CI with blocked hosted verification, automatic UDID selection and the optional Selenium web adapter. Updated the package comment and removed the driver-specific Pages label.
- **Verification completed:** Compared wording with pipeline jobs, the empty shared UDID and Selenium/Appium project references. Local Markdown target/anchor checks and git diff --check validate the edited documentation; no mobile execution was needed.
- **Where / evidence:** `README.md:478` says mobile tests are only compiled and hosted agents have no emulator, whereas `azure-pipelines.yml` defines scheduled/manual `MobileSmoke` execution. `docs/GETTING_STARTED.md:24` and `docs/ARCHITECTURE.md` still say the first CI run is pending, although R32/R33 record completed failed attempts. `docs/GETTING_STARTED.md:153` says shared settings expect emulator-5554, whereas `Tests/UI/Mobile/mobilesettings.json` has the empty UDID established by R29. `Directory.Packages.props:29` says Selenium exists only for Appium, whereas `Core/Ui/Selenium/Core.Ui.Selenium.csproj` implements an optional web provider.
- **Original consequence:** Readers receive conflicting explanations of CI readiness, device selection and the purpose of Selenium in the current framework.
- **Fix:** Describe mobile CI as configured for scheduled/manual runs with successful hosted verification blocked/deferred (R16/R32/R33); explain automatic device selection with optional explicit UDID; describe Selenium as the optional web adapter and shared WebDriver basis for Appium. Correcting prose does not resume mobile work or reopen R29/R30.
- **Verify:** Trace replacement text to the pipeline, shared configuration and adapter references; check affected links and remove contradictory current-state claims while preserving dated historical evidence.


<a id="r53"></a>
### R53. Top-level review status and validation summaries are stale

- **Status:** Closed, 2026-11-18, verified and committed in `28b32bf` on `fix/framework-adapter-contracts`.
- **Implementation:** Added a current November 18 status/validation summary reconciled with all 53 finding rows and details. Renamed the old November 16 sections as historical status/coverage snapshots, preserved their original evidence, and recorded the current native test scope separately.
- **Where:** `docs/REVIEW.md`, sections `Current state — 2026-11-16` and `Coverage state — 2026-11-16`; compare the findings table and individual R25, R32, R33 and R36 entries.
- **Original problem:** The top-level summary still describes the November 16 state, including R25 awaiting its first Windows run and a 30-finding status breakdown. Before this review, the ledger already contained 48 findings and marked R25 closed. The dated 37-test result is valid historical evidence, but presenting these snapshots as current obscures newer implementation and verification states.
- **Fix:** Label the November 16 status/coverage sections as historical and add a current summary reconciled with every finding, or refresh the current summaries while retaining the original evidence as history. Keep reported test runs distinct from current locally observed evidence.
- **Verification:** Validated 53 unique table/detail IDs and anchors, 49 closed findings, 3 explicitly blocked mobile findings, and 1 implemented Key Vault finding awaiting live verification. R25 is closed. Current TRX reports distinguish 191 unit/framework passes plus 2 broker skips and 28 headed UI contract passes from the historical 37-test run; no coverage percentage is claimed.

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

- **Status:** Blocked (mobile work deferred), 2026-11-18, at the user's request.
- **Blocker / scope:** Further mobile framework and hosted emulator investigation/verification is deferred for now. Existing implementation and evidence are retained; this finding is not closed.
- **Resume when:** The user resumes mobile work; continue from the recorded implementation, run history and outstanding verification below.
- **Prior status / evidence:** Implemented (CI verification pending), 2026-11-16, committed in `5cdb42c` on `fix/review-p1-p2`; no hosted run has been executed by the agent.
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

- **Status:** Closed, 2026-11-17 · `1d6fdec`. Core.Ui.Selenium adapter (selenium) and the appium UI driver in Core.Mobile (WebDriver session + IMobileGestures); the mobile suite uses the shared page objects and Ui.Expect. Verified: Tests/Contract UI suite 18/18 (Playwright and Selenium); Android smoke passes through the appium driver with the shared Pages.SauceDemo.LoginPage (Allure result identical to the baseline); the web suite passes with PROVIDERS_Ui=selenium BROWSER=chrome. Final check (same day, clean build): the full web suite passes 8/8 on Selenium in 5 consecutive runs. Earlier, SauceDemo's cart link briefly ignored WebDriver's native clicks (3 runs, reproduced with plain Selenium); later, plain Selenium navigated with the same markup, so it was the site. Noted in GETTING_STARTED.
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
