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
- **Unified UI and API test layers**: Selenium-based UI automation and REST API testing side-by-side
- Page Object Model (POM) pattern for maintainable, reusable web code
- Test data parameterization via JSON
- NUnit-based test structure (for both UI and API)
- **Automatic test timing**: Each test's start time, end time, and duration are visible directly in the Allure report, for both UI and API layers
- Allure reporting integration for rich, actionable output
- Batch scripts that are independent of system-installed .NET: Prefer local dotnet runtime if available, fallback to system
- Easily extensible for mobile or other platforms

---

## 📂 Project Structure

```plaintext
modern-test-framework/
│
├── Core/                  # Shared core utilities and abstractions (data loaders, config, etc.)
│   └── Utilities/
│       ├── ConfigManager.cs
│       ├── ITestDataLoader.cs
│       ├── TestDataLoader.cs
│       └── ...
│
├── API/                   # API automation suite
│   ├── Models/            # DTOs for API payloads
│   ├── Tests/             # NUnit API test classes (with timing + Allure reporting)
│   ├── Utilities/         # API-specific test utilities
│   ├── test-data/
│   └── API.Tests.csproj
│
├── UI/
│   ├── Web/               # Web UI test suite using Selenium + NUnit
│   │   ├── Pages/         # Page Object Models
│   │   ├── Models/        # Test data models and DTOs
│   │   ├── Tests/         # NUnit test classes (with common BaseTest managing browser and timing)
│   │   ├── Utilities/
│   │   ├── test-data/     # JSON test datasets
│   │   ├── allure-results/
│   │   ├── allure-report/
│   │   └── UI.Web.Tests.csproj
│   └── ...
│
├── bat/                   # Batch scripts for test automation and reporting
│   ├── run_all_tests_gen_report.bat   # Run UI & API and merge Allure
│   ├── run_api_tests_gen_report.bat   # Run just API tests and Allure
│   ├── run_web_tests_gen_report.bat   # Run just UI/Web and Allure
│   ├── run_ui_web_all_browsers.bat    # Run UI tests across browsers; Allure merge
│   ├── kill_all_webdrivers.bat
│   └── ...
│
├── tools/                 # (Optional) Bundled dotnet runtime (tools/dotnet/dotnet.exe)
│
├── docs/                  # Architecture diagrams, guides
│   ├── ARCHITECTURE.md
│   └── ...
│
├── .gitignore
├── README.md

```

---

## 🧪 Running UI & API Tests / Generating Reports

Batch scripts automate clean, build, test, and report generation, and do **not** require a globally installed .NET runtime:

- The scripts will use a local `tools/dotnet/dotnet.exe` if present, for full isolation.
- If not bundled, they will fallback to your system `dotnet` install.

**To run everything and generate a merged Allure report:**  
```
bat\run_all_tests_gen_report.bat
```
Includes both API and UI tests, reporting, result merge.

**To run just API tests:**  
```
bat\run_api_tests_gen_report.bat
```

**To run just UI/Web tests:**  
```
bat\run_web_tests_gen_report.bat
```

**To run UI tests on all supported browsers and merge Allure results:**  
```
bat\run_ui_web_all_browsers.bat
```

**To kill all (possibly orphaned) browser drivers:**  
```
bat\kill_all_webdrivers.bat
```

---

## 📊 Allure Reporting and Per-Test Timing

Test execution produces Allure-compliant results in appropriate allure-results folders.

Every test in both UI and API suites reports the following as Allure parameters (visible in Allure test result details):
- **Start Time**: When the test began (down to milliseconds)
- **End Time**: When the test ended
- **Duration (s)**: How long the test ran (in seconds, with millisecond precision)

> In UI, this is managed via a BaseTest; for API, timing/reporting is added in each test class's SetUp/TearDown.

Batch scripts ensure results are copied and merged as needed.
Review your results in the Allure report UI for full traceability and timing insight.

---

## 🛠️ BaseTest and API Test Infrastructure

For UI (Web), a common `BaseTest` class (in `UI/Web/Tests/BaseTest.cs`) manages:
- WebDriver setup/teardown (Chrome, Firefox, Edge, headless, etc.)
- Common browser lifecycle management
- Screenshot on failure (with Allure attach)
- **Automatic recording of test timing and adding timing info to Allure report**
- Parameterization of browser runs

For API, **each test class** includes timing and Allure reporting logic in SetUp/TearDown to provide similar visibility.

---

## 📖 Diagrams & Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for:
- High-level architecture diagram
- Test runner flow for modern .NET automation
- How core utilities, test abstraction, Allure reporting, and layer separation work

---

## 🤝 Contributing

Contributions and suggestions are welcome!  
Open an issue or PR. Please use the provided batch scripts for validation—they'll work out of the box if a local dotnet runtime is bundled, otherwise you need system dotnet installed.

---

## 📄 License

Add your license file (if any) here.
