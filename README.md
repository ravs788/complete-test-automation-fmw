# Modern Test Framework

<!-- Repo Metadata -->
[![Main Regression](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/main-regression.yml/badge.svg)](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/main-regression.yml)
[![PR Smoke](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/pr-smoke.yml/badge.svg)](https://github.com/ravs788/complete-test-automation-fmw/actions/workflows/pr-smoke.yml)
[![Issues](https://img.shields.io/github/issues/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/issues)
[![Forks](https://img.shields.io/github/forks/ravs788/complete-test-automation-fmw?style=social)](https://github.com/ravs788/complete-test-automation-fmw/network/members)
[![Stars](https://img.shields.io/github/stars/ravs788/complete-test-automation-fmw?style=social)](https://github.com/ravs788/complete-test-automation-fmw/stargazers)
[![Contributors](https://img.shields.io/github/contributors/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/graphs/contributors)
[![Last Commit](https://img.shields.io/github/last-commit/ravs788/complete-test-automation-fmw)](https://github.com/ravs788/complete-test-automation-fmw/commits/main)

---

## 🚀 Features

- Modular C# .NET 9 test automation framework
- Unified UI and API test layers: Selenium-based UI automation and REST API testing side-by-side
- Page Object Model (POM) for maintainable, reusable UI code
- Test data parameterization via JSON
- NUnit-based test structure (for both UI and API)
- Automatic test timing: Each test&#39;s start time, end time, and duration are added to Allure (UI and API)
- Allure reporting integration for rich, actionable output
- Batch scripts that are independent of system-installed .NET: prefer bundled runtime if present, fallback to system
- Easily extensible for mobile or other platforms
- Pluggable logging system with providers (console, elastic) configured via logging-config.json
- Per-project configuration: each project owns its config.json and strongly-typed ConfigSettings, loaded via Core.ConfigLoader

---

## 📂 Project Structure

```plaintext
modern-test-framework/
│
├── Core/                        # Shared utilities and abstractions
│   ├── Models/
│   │   ├── LoggingConfig.cs
│   │   ├── ElasticSection.cs
│   │   └── ConsoleSection.cs
│   └── Utilities/
│       ├── Config/
│       │   └── ConfigLoader.cs        # Generic loader for project-level config.json
│       ├── Logging/                   # Pluggable logging + results publishing
│       │   ├── Common/
│       │   │   ├── ILoggingProviderFactory.cs
│       │   │   ├── ILoggingService.cs
│       │   │   ├── IResultsPublisher.cs
│       │   │   ├── IResultsPublisherFactory.cs
│       │   │   ├── LoggingServiceFactory.cs
│       │   │   └── ResultsPublisherFactory.cs
│       │   ├── Console/
│       │   │   ├── ConsoleLoggingProviderFactory.cs
│       │   │   ├── ConsoleLoggingService.cs
│       │   │   ├── ConsoleResultsPublisher.cs
│       │   │   └── ConsoleResultsPublisherFactory.cs
│       │   └── Elastic/
│       │       ├── ElasticClientFactory.cs
│       │       ├── ElasticConnectivity.cs
│       │       ├── ElasticLoggingProviderFactory.cs
│       │       ├── ElasticLoggingService.cs
│       │       ├── ElasticResultsPublisher.cs
│       │       └── ElasticResultsPublisherFactory.cs
│       └── Screenshots/
│           ├── IScreenshotHelper.cs
│           └── AllureScreenshotHelper.cs     # Core stub (UI layer provides implementation)
│
├── API/                         # API automation suite
│   ├── Models/
│   ├── Tests/
│   ├── Utilities/
│   │   ├── ApiClient.cs
│   │   ├── AuthHelper.cs
│   │   └── ConfigSettings.cs     # API-specific settings: BaseUrl, DefaultUsername, DefaultPassword
│   ├── config.json               # Copied to output on build
│   ├── test-data/
│   └── API.Tests.csproj
│
├── UI/
│   ├── Web/                      # Web UI test suite using Selenium + NUnit
│   │   ├── Pages/
│   │   ├── Models/
│   │   ├── Tests/
│   │   │   └── BaseWebTest.cs    # Common setup/teardown, logging, timing, screenshot-on-failure
│   │   ├── Utilities/
│   │   │   ├── AllureScreenshotHelper.cs   # UI implementation used by tests
│   │   │   ├── AssertHelper.cs
│   │   │   └── ConfigSettings.cs  # UI-specific settings: RunTestsInParallel, Browser, Headless, BaseUrl
│   │   ├── config.json           # Copied to output on build
│   │   ├── test-data/
│   │   └── UI.Web.Tests.csproj
│   └── ...
│
├── logging-config.json          # Global logging provider configuration (console/elastic)
├── bat/                         # Batch scripts for test automation and reporting
│   ├── run_all_tests_gen_report.bat
│   ├── run_api_tests_gen_report.bat
│   ├── run_web_tests_gen_report.bat
│   ├── run_ui_web_all_browsers.bat
│   └── kill_all_webdrivers.bat
│
├── docs/
│   └── ARCHITECTURE.md
├── .gitignore
├── README.md
└── modern-test-framework.sln
```

---

## ⚙️ Configuration (Per Project)

Each project owns its own config.json and strongly-typed ConfigSettings class. Load settings via the shared ConfigLoader, which reads config.json from the test output directory (copied on build by each .csproj).

- UI/Web example
  - config.json:
    ```
    {
      "RunTestsInParallel": false,
      "Browser": "chrome",
      "Headless": true,
      "BaseUrl": "https://www.saucedemo.com/"
    }
    ```
  - Use in tests:
    ```csharp
    using Core.Utilities;
    var cfg = ConfigLoader.Load<UI.Web.Utilities.ConfigSettings>();
    var headless = cfg.Headless;
    var baseUrl = cfg.BaseUrl;
    ```

- API example
  - config.json:
    ```
    {
      "BaseUrl": "https://restful-booker.herokuapp.com/",
      "DefaultUsername": "admin",
      "DefaultPassword": "password123"
    }
    ```
  - Use in utilities:
    ```csharp
    using Core.Utilities;
    var cfg = ConfigLoader.Load<API.Utilities.ConfigSettings>();
    var client = new RestClient(cfg.BaseUrl);
    ```

Notes:
- Core ConfigManager and Core ConfigSettings have been removed. Prefer ConfigLoader + project-local ConfigSettings moving forward.
- Ensure the project&#39;s .csproj includes:
  ```
  <Content Include="config.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
  ```

---

## 🧾 Logging (Pluggable Providers)

Logging is configured centrally via logging-config.json at the repo root. The framework dynamically discovers logging provider factories and selects one based on configuration, with fallbacks.

- Configure provider and settings in logging-config.json:
  ```json
  {
    "Provider": "elastic", // or "console"
    "Elastic": {
      "Url": "https://your-elastic:9200",
      "Username": "elastic",
      "Password": "changeme"
    },
    "Console": {
      "MinLevel": "Information"
    }
  }
  ```

- Create a logger:
  ```csharp
    var logger = LoggingServiceFactory.CreateLogger("ui-web-logs-{0:yyyy.MM.dd}");
    logger.Info("message");
  ```

Providers:
- elastic: sends logs and test results to Elasticsearch using Elastic.Clients.Elasticsearch via the Elastic* helpers.
- console: writes logs and publishes results to console (helpful for local dev or CI without Elastic).

---

## 📣 Results Publishing

Test outcomes are published through the ResultsPublisherFactory. Each provider exposes a corresponding results publisher (e.g., ElasticResultsPublisher, ConsoleResultsPublisher).

Typical usage in TearDown:
```csharp
var publisher = ResultsPublisherFactory.Create();
publisher.Publish(new LogMetadata {
  ProjectName = "ui-web",
  TestClassName = ctx.Test.ClassName ?? string.Empty,
  TestMethodName = ctx.Test.MethodName ?? ctx.Test.Name,
  Status = ctx.Result.Outcome.Status.ToString(),
  Duration = elapsedSeconds,
  Reason = ctx.Result.Message ?? string.Empty,
  RunTime = DateTime.Now.ToString("o"),
  RunName = ctx.Test.FullName ?? ctx.Test.Name,
  TriggeredBy = Environment.UserName,
  Browser = browser
});
```

- When Provider=elastic, results are indexed into Elasticsearch.
- When Provider=console, results are printed to the console.

---

## 🖼️ Screenshots

- Core defines IScreenshotHelper and provides a stub AllureScreenshotHelper to keep Core UI-agnostic.
- UI/Web provides the working implementation in UI/Web/Utilities/AllureScreenshotHelper.cs.
- BaseWebTest captures a screenshot and attaches to Allure on failures.

---

## 🧪 Running UI & API Tests / Generating Reports

Batch scripts automate clean, build, test, and report generation, and do not require a globally installed .NET runtime:

- The scripts will use a local tools/dotnet/dotnet.exe if present, for full isolation.
- If not bundled, they will fallback to your system dotnet install.

Run everything and generate a merged Allure report:
```
bat\run_all_tests_gen_report.bat
```

Just API tests:
```
bat\run_api_tests_gen_report.bat
```

Just UI/Web tests:
```
bat\run_web_tests_gen_report.bat
```

UI tests on all supported browsers and merge Allure results:
```
bat\run_ui_web_all_browsers.bat
```

Kill all (possibly orphaned) browser drivers:
```
bat\kill_all_webdrivers.bat
```

---

## 🛠️ BaseWebTest and API Test Infrastructure

- UI (Web) common base: UI/Web/Tests/BaseWebTest.cs
  - WebDriver setup/teardown (Chrome, Firefox, Edge, headless)
  - Browser lifecycle management
  - Screenshot on failure (with Allure attach)
  - Automatic recording of test timing and adding timing info to Allure report
  - Parameterization of browser runs

- API tests: Each class includes timing and Allure reporting in SetUp/TearDown, and uses ApiClient/AuthHelper utilities.

---

## 📖 Diagrams & Architecture

See docs/ARCHITECTURE.md for:
- High-level architecture diagram
- Test runner flow for modern .NET automation
- How core utilities, test abstraction, Allure reporting, and layer separation work

---

## 🆕 What&#39;s New Since Last Push to main

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

---

## 🤝 Contributing

Contributions and suggestions are welcome.
Open an issue or PR. Please use the provided batch scripts for validation—they&#39;ll work out of the box if a local dotnet runtime is bundled, otherwise you need system dotnet installed.

---

## 📄 License

Add your license file (if any) here.
