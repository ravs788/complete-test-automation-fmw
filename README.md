# Modern Test Framework

<!-- Repo Metadata -->
[![Issues](https://img.shields.io/github/issues)]() [![Forks](https://img.shields.io/github/forks?style=social)]() [![Stars](https://img.shields.io/github/stars?style=social)]() [![Contributors](https://img.shields.io/github/contributors)]() [![Last Commit](https://img.shields.io/github/last-commit)]()

---

## 🚀 Features

- Modular C# .NET 9 test automation framework
- UI automated tests for web applications using Selenium WebDriver
- Page Object Model (POM) pattern for maintainable, reusable code
- Test data parameterization via JSON
- NUnit-based test structure
- Allure reporting integration for rich test output
- Custom batch scripts for automation of testing and reporting
- Easily extensible for other platforms, e.g., mobile or API automation

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
├── UI/
│   ├── Web/               # Web UI test suite using Selenium + NUnit
│   │   ├── Pages/         # Page Object Models
│   │   ├── Models/        # Test data models and DTOs
│   │   ├── Tests/         # NUnit test classes
│   │   ├── Utilities/
│   │   ├── test-data/     # JSON test datasets
│   │   ├── allure-results/
│   │   ├── allure-report/
│   │   └── UI.Web.Tests.csproj
│   └── ...
│
├── bat/                   # Batch scripts for test automation and reporting
│   ├── run_tests_gen_report.bat
│   ├── kill_all_webdrivers.bat
│   └── ...
│
├── flow-diagram.md        # Architecture and modern test control flow (see this file)
├── .gitignore
├── README.md

```

---

## 🧪 Running UI Tests & Generating Allure Reports

Use provided batch scripts on Windows to automate clean, build, test, and report operations:

- **Run all tests and generate Allure report**  
  ```
  bat\run_tests_gen_report.bat
  ```
  This:
  - Cleans and builds the UI.Web test project
  - Runs all UI.Web tests
  - Generates and opens the Allure report for review

- **Kill all browser drivers (Chrome, Firefox, Edge)**  
  ```
  bat\kill_all_webdrivers.bat
  ```

---

## 📊 Allure Reporting

Test execution produces Allure-compliant results in `UI/Web/allure-results/`.
The `bat/run_tests_gen_report.bat` script:
- Generates a static Allure report in `UI/Web/allure-report/`
- (Optionally) serves the report interactively after each run

---

## 📖 Diagrams & Architecture

See [flow-diagram.md](flow-diagram.md) for:
- High-level architecture diagram
- Control flow for the modern Selenium test framework
- How core utilities, page objects, tests, and data combine

---

## 🤝 Contributing

PRs and suggestions are welcome! Please open issues or submit pull requests. Run `bat/run_tests_gen_report.bat` to validate your changes before submitting.

---

## 📄 License

Add your license file (if any) here.
