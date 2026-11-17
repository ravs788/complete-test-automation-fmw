# Flow Diagrams

How control and data move through the framework, drawn with [Mermaid](https://mermaid.js.org/) so GitHub renders them and they change in the same pull request as the code. The text reference for each part is in [ARCHITECTURE.md](ARCHITECTURE.md); mobile details are in [MOBILE_ARCHITECTURE.md](MOBILE_ARCHITECTURE.md).

1. [Projects and dependencies](#1-projects-and-dependencies)
2. [A web UI test run](#2-a-web-ui-test-run)
3. [An API test run](#3-an-api-test-run)
4. [A mobile test run](#4-a-mobile-test-run)
5. [Settings and secrets](#5-settings-and-secrets)
6. [CI pipeline](#6-ci-pipeline)

To edit a diagram, paste it into the [Mermaid Live Editor](https://mermaid.live/) or use an editor with a Mermaid preview, then copy it back.

---

## 1. Projects and dependencies

Arrows point from a project to the project it references. Nothing references the test projects, and `Core` references none of the others (enforced by `Tests/Unit/Architecture/CoreBoundaryTests`). The add-ons live inside `Core/` but are separate projects, so their packages reach only the suites that use them.

```mermaid
flowchart TB
    subgraph TESTS["Tests/ — test projects"]
        direction LR
        T["<b>Tests</b><br/>Playwright: API + web UI"]
        TM["<b>Tests.UI.Mobile</b><br/>Appium: Android smoke"]
        TU["<b>Tests.Unit</b><br/>framework self-tests"]
    end

    P["<b>Pages</b><br/>Playwright page objects"]
    D["<b>Domain</b><br/>models · Builders/Mothers · AuthClient"]

    subgraph CORE["Core/"]
        direction TB
        C["<b>Core</b> — domain-agnostic engine<br/>configuration · API client · browser driver · logging<br/>resilience · security · data · messaging · test runner"]
        subgraph ADDONS["optional add-on projects"]
            direction LR
            CM["Core.Mobile<br/>Appium/Selenium"]
            KV["Core.Security.AzureKeyVault<br/>Azure SDK"]
            RQ["Core.Messaging.RabbitMq<br/>RabbitMQ.Client"]
            KF["Core.Messaging.Kafka<br/>Confluent.Kafka"]
        end
    end

    T --> P & D & C & KV
    TM --> CM & KV
    TU --> C & D & CM & KV & RQ & KF
    P --> C
    D --> C
    CM & KV & RQ & KF --> C

    classDef test fill:#e8f1fb,stroke:#3b7dd8,color:#0b2a4a
    classDef app fill:#eef7ee,stroke:#3a9a4a,color:#123d1a
    classDef core fill:#fff4e5,stroke:#e08a00,color:#4a2c00
    classDef addon fill:#fbf7ef,stroke:#c9a35c,stroke-dasharray:4 3,color:#4a2c00
    class T,TM,TU test
    class P,D app
    class C core
    class CM,KV,RQ,KF addon
```

---

## 2. A web UI test run

One test class (for example `CheckoutFlowTests`) from start to finish. The browser is launched once per class; every test gets a fresh context and page. `TestRunner` (in `Core`) is the same for every suite.

```mermaid
sequenceDiagram
    autonumber
    participant X as xUnit
    participant M as TestRunSetup<br/>(module initializer)
    participant F as PlaywrightFixture<br/>(once per class)
    participant B as App test base<br/>(UiTestBase)
    participant R as TestRunner
    participant PG as Page objects
    participant O as Allure + logs

    X->>M: assembly loads
    M->>M: clear previous allure-results<br/>chain Key Vault if KEYVAULT_URI is set
    X->>F: InitializeAsync()
    F->>F: read settings (testsettings.json, env)<br/>launch browser (Settings.Browser)

    loop every test in the class
        X->>B: new test instance + InitializeAsync()
        B->>F: CreateCleanPageAsync(app BaseUrl)
        F-->>B: new BrowserContext + Page
        B->>B: start Playwright tracing (unless Trace = off)
        X->>B: [Fact] calls RunAsync(body)
        B->>R: RunAsync(info, body, onFailure, cleanup)
        R->>O: [Start] log line, browser in Allure name
        R->>PG: body: page objects, Mothers/Builders, Expect(...)
        Note over PG: GotoAsync retries navigation<br/>(Resilience.Navigation)
        PG-->>R: next page object / assertion result
        alt test failed
            R->>O: [Failed] log line
            R->>B: onFailure
            B->>O: full-page screenshot + trace zip
            Note over R: exception is rethrown, test fails
        end
        R->>O: [Finish] log, Allure timing, LogMetadata to Elastic or console
        R->>B: cleanup: attach trace if Trace = on
        X->>B: DisposeAsync(): close context
    end

    X->>F: DisposeAsync(): close browser, dispose Playwright
```

---

## 3. An API test run

API tests share one Playwright instance per class (no browser). Retries apply only to idempotent methods (GET, HEAD, OPTIONS, PUT, DELETE, TRACE) on 502/503/504 or network errors; a POST is never retried. Everything a test creates is registered for cleanup and removed after the test, last first.

```mermaid
sequenceDiagram
    autonumber
    participant X as xUnit
    participant AF as ApiFixture<br/>(once per class)
    participant AB as ApiTestBase
    participant R as TestRunner
    participant C as ApiClient
    participant API as Restful Booker
    participant CT as CleanupTracker

    X->>AF: InitializeAsync(): Playwright, no browser

    loop every test in the class
        X->>AB: new test instance + InitializeAsync()
        AB->>C: ApiClient.CreateAsync(BaseUrl, Resilience.Http)
        X->>AB: [Fact] calls RunAsync(body)
        AB->>R: RunAsync(info, body)
        R->>R: [Start] log line
        AB->>C: CreateBookingAsync(BookingMother.Standard())
        C->>API: POST /booking (never retried)
        API-->>C: 200 + booking id
        AB->>CT: Add("DELETE booking/{id}")
        AB->>C: GET / PUT / PATCH / DELETE with auth headers
        C->>API: request
        opt 502/503/504 or network error on an idempotent method
            C->>API: retry with backoff (Resilience.Http)
        end
        API-->>C: response
        C-->>AB: typed model, or ApiException on non-2xx
        R->>R: [Finish] log, Allure timing, LogMetadata
        X->>AB: DisposeAsync()
        AB->>CT: RunAsync(): run cleanups, last first
        CT->>API: DELETE /booking/{id} (one auth token per test)
        Note over CT,API: a cleanup failure is logged, never fails the test
        AB->>C: DisposeAsync()
    end
```

---

## 4. A mobile test run

The Android suite runs against an Appium server and an emulator or device. Settings come from `mobilesettings.json` (`MOBILE_*` env vars override). In CI, `scripts/unix/run_mobile_ci.sh` prepares the emulator, Chromedriver and Chrome before the tests start.

```mermaid
sequenceDiagram
    autonumber
    participant X as xUnit
    participant MB as MobileTestBase
    participant R as TestRunner
    participant DF as MobileDriverFactory<br/>(Core.Mobile)
    participant AP as Appium server
    participant DEV as Emulator / device<br/>(Chrome via Chromedriver)

    X->>MB: new test instance
    X->>MB: [Fact] calls Run(body)
    MB->>R: Run(info, body, onFailure, cleanup)
    R->>R: [Start] log line
    R->>MB: body, step 1: start the session
    MB->>DF: CreateDriver(settings)
    DF->>DF: MobileOptionsBuilder: platform, app type,<br/>timeouts, Chrome options (androidKeepAppDataDir = NoReset)
    DF->>AP: POST /session
    AP->>DEV: start UiAutomator2, then Chromedriver + Chrome (web)<br/>or the app under test (native)
    AP-->>DF: session
    MB->>DEV: web: navigate to BaseUrl
    R->>DEV: body, step 2: the test: mobile page objects,<br/>SafeClick/SafeType (retried)
    alt test failed
        R->>MB: onFailure: screenshot to Allure
    end
    R->>R: [Finish] log, Allure timing, LogMetadata
    R->>MB: cleanup: quit driver
    MB->>AP: DELETE /session
```

---

## 5. Settings and secrets

Every settings record is loaded the same way by `SettingsLoader` and cached for the run. Later sources override earlier ones. Any value of the form `secret://NAME` is then resolved through `SettingsLoader.SecretProvider`.

```mermaid
flowchart LR
    J["JSON file<br/><i>testsettings.json · logging-config.json · mobilesettings.json</i>"]
    E["Prefixed env vars<br/><i>TESTS_ · LOGGING_ · MOBILE_</i><br/>__ = nesting"]
    A["Short aliases<br/><i>BROWSER · HEADLESS · TRACE · ELASTIC_* · APPIUM_URL · DEVICE_NAME</i>"]
    S{"value is<br/>secret://NAME ?"}
    EP["Environment variables<br/><i>NAME, then NAME upper-cased</i>"]
    KV["Azure Key Vault<br/><i>only when KEYVAULT_URI is set</i>"]
    X["Missing secret:<br/>fail with setting + secret named"]
    REC["Typed record, cached for the run<br/><i>TestSettings · LoggingConfig · MobileSettings</i>"]

    J --> E --> A --> S
    S -- no --> REC
    S -- yes --> EP
    EP -- found --> REC
    EP -- not found --> KV
    KV -- found --> REC
    KV -- not found --> X

    classDef src fill:#e8f1fb,stroke:#3b7dd8,color:#0b2a4a
    classDef secret fill:#fff4e5,stroke:#e08a00,color:#4a2c00
    classDef ok fill:#eef7ee,stroke:#3a9a4a,color:#123d1a
    classDef bad fill:#fdecea,stroke:#d93025,color:#5a0f0a
    class J,E,A src
    class EP,KV secret
    class REC ok
    class X bad
```

---

## 6. CI pipeline

GitHub is the source; Azure Pipelines (`azure-pipelines.yml`) runs the builds and reports back as GitHub checks. There is one parallel job, so jobs queue one after another.

```mermaid
flowchart TB
    PR(["Pull request into main"])
    PUSH(["Push to main / release/*"])
    NIGHT(["Nightly 02:00 UTC<br/>(only if main changed)"])
    MAN(["Manual run"])

    subgraph LINUX["Job: Playwright UI and API Tests — ubuntu"]
        direction TB
        L1["Build ci.slnf<br/>(warnings as errors)"]
        L2["Unit tests<br/>Tests/Unit"]
        L3["Playwright API + web UI<br/>on Chrome"]
        L4["Web UI on Chromium,<br/>Firefox, WebKit"]
        L5["Allure report<br/>with history from main"]
        L1 --> L2 --> L3 --> L5
        L3 -. nightly only .-> L4 --> L5
    end

    subgraph MAC["Job: Android 14 Chrome Smoke — macOS"]
        direction TB
        M1["Boot emulator<br/>(up to 900 s)"]
        M2["Pre-install Appium helpers,<br/>Chromedriver, warm up Chrome"]
        M3["Start Appium,<br/>run mobile smoke"]
        M1 --> M2 --> M3
    end

    subgraph WIN["Job: Windows helper check — windows-2025"]
        W1["A failed build never<br/>starts the tests"]
    end

    PR -- "all except non-smoke<br/>SauceDemo, no coverage" --> LINUX
    PUSH -- "all tests + coverage" --> LINUX
    NIGHT --> LINUX & MAC & WIN
    MAN --> LINUX & MAC & WIN

    LINUX --> A1[/"TRX · allure-results · allure-report"/]
    MAC --> A2[/"mobile-results: TRX, Allure, Appium/emulator/logcat logs"/]
    WIN --> A3[/"windows-helper-verification"/]
    LINUX & MAC & WIN --> GH(["Status checks on GitHub"])

    classDef trigger fill:#e8f1fb,stroke:#3b7dd8,color:#0b2a4a
    classDef out fill:#eef7ee,stroke:#3a9a4a,color:#123d1a
    class PR,PUSH,NIGHT,MAN,GH trigger
    class A1,A2,A3 out
```
