# Modern C# Test Automation Framework Architecture

## Overview

This document describes the architecture of a modular, scalable test automation framework written in C#. The framework is designed to support API, Web UI (browser-based), and Desktop UI automation. Portability, ease of configuration, and compatibility with CI/CD systems like Azure DevOps are foundational goals. This document will be updated to reflect any major architectural changes.

---

## 1. Goals and Vision

- **Transportable:** Easily adapted and reused across diverse application types and environments with minimal setup.
- **Maintainable:** Strong separation of concerns with an extensible, modular design.
- **Comprehensive:** Unified support for API, browser, and desktop UI testing within one structure.
- **Modern Reporting:** Integration with rich HTML and developer-friendly reporting tools.
- **CI/CD Ready:** Out-of-the-box compatibility with Azure DevOps for seamless automation and reporting in pipelines.

---

## 2. Architectural Principles

- **SOLID Design:** Classes and interfaces are designed for single responsibility, open/closed extension, and easy testability.
- **Clean Architecture:** Core abstractions are separated from technical details (driver code, external dependencies).
- **Dependency Injection:** Test code and drivers are loosely coupled and configurable at runtime.
- **Configuration-Driven:** All runtime specifics (servers, credentials, endpoints, driver options) are externalized.

---

## 3. Project & Folder Structure

```
/modern-test-framework/
│
├── src/
│   ├── Core/                # Cross-cutting abstractions, interfaces, and shared models
│   ├── API/
│   │   ├── Drivers/         # Wrappers for HTTP clients (e.g., RestSharp, Flurl)
│   │   ├── Tests/           # API test suites by domain
│   │   └── Helpers/         # Utilities for requests, assertions, deserialization
│   ├── UI/
│   │   ├── Browser/
│   │   │   ├── Drivers/     # Selenium/Playwright setups
│   │   │   ├── Pages/       # Page Object Models
│   │   │   └── Tests/       # Browser UI test suites
│   │   ├── Desktop/
│   │   │   ├── Drivers/     # WinAppDriver/FlaUI
│   │   │   ├── Pages/       # Window/View models
│   │   │   └── Tests/       # Desktop UI test suites
│   │   ├── Mobile/
│   │   │   ├── Drivers/     # Appium-based (cross-platform Android/iOS)
│   │   │   ├── Pages/       # Screen/Page Models for mobile
│   │   │   └── Tests/       # Mobile UI test suites
│   ├── Reporting/           # Integrations for ExtentReports, Allure, etc.
│   ├── Configs/             # Environment/config files (JSON, XML, ENV)
│   └── Utils/               # Shared utilities, logging, custom assertions
│
├── pipelines/               # Azure DevOps YAMLs, helper scripts for CI/CD
├── test-data/               # Data for data-driven testing (CSV/JSON/Excel)
├── docs/                    # Technical/design documentation
├── .gitignore
├── README.md
├── ARCHITECTURE.md
└── modern-test-framework.sln
```

Each major artifact (API, UI-Browser, UI-Desktop, UI-Mobile) is structured to allow for maximum code reuse and minimal coupling.

---

## 4. Technology Stack

- **.NET:** C# class libraries and test projects (.NET 6+ preferred)
- **API:** RestSharp or Flurl.Http for REST endpoints
- **Web UI:** Selenium WebDriver or Playwright for browser automation (driver wrappers abstracted for swap-ability)
- **Desktop UI:** WinAppDriver, FlaUI, or TestStack.White (abstracted via common interfaces)
- **Mobile UI:** Appium via Appium.WebDriver (industry standard for cross-platform mobile UI automation; supports Android/iOS emulators/devices). Alternatives: Xamarin.UITest, MAUI UITest (less common, more restricted).
- **Test Runner:** NUnit (flexible and widely supported), with extensibility for xUnit/MSTest
- **Reporting:** ExtentReports for rich HTML outputs; Allure for advanced pipeline visualization (optional)
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection or Autofac
- **Configuration:** JSON, XML, or ENV file-driven

---

## 5. Modularity and Portability

- **Full Stack Coverage:** UI layer is now split into Browser, Desktop, and Mobile modules—all following the same pluggable Page Object and Driver patterns for consistency.
- **Abstractions:** All automation interfaces (drivers, pages, services) are defined in Core and implemented in feature modules.
- **Plug-and-Play:** Add new drivers or test types by implementing interfaces; no impact on unrelated modules.
- **Configuration:** All environment, credential, and runtime values are externalized for maximum portability.
- **Test Data:** Separated from test code, enabling easy reuse and data-driven scripting.

---

## 6. CI/CD and Reporting

- **Azure Pipelines Integration:** YAML-based pipelines to run tests, publish results, and upload HTML/Allure reports as pipeline artifacts.
- **Test Results:** Test result adapters (NUnit XML, Allure) ensure all test outcomes are captured and visualized in Azure DevOps.
- **Reporting:** ExtentReports and/or Allure are generated as part of the test run, accessible post-build.

---

## 7. Extending the Framework

- Add new automation technology by:
  - Creating interface in `/Core`
  - Implementing in `/API`, `/UI/Browser`, or `/UI/Desktop` as appropriate
  - Wiring up with dependency injection in startup/config
- Configuration should be updated with any new environment or driver requirements
- Update this `ARCHITECTURE.md` when making major design changes

---

## 8. Revision History

| Date       | Author        | Notes               |
|------------|---------------|---------------------|
| 2025-09-03 | Initial Draft | Baseline definition |
| 2025-09-03 | Arch update   | Added Mobile UI layer; Appium integration and updated docs |

---

This document is a starting point and will be maintained to reflect architectural refinements and significant design decisions.
