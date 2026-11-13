# Getting Started Guide

This page walks through running the framework **locally**, with tips and troubleshooting notes.
If you only need the short version, see the "⚡ Quick Start" section in the root README.

---

## 1  Clone & Build

```bash
git clone https://github.com/ravs788/complete-test-automation-fmw.git
cd complete-test-automation-fmw
dotnet build TestAutomationPlatform.slnx
```

---

## 2  Verify Prerequisites

| Requirement | Recommended Version | Check Command |
|-------------|--------------------|---------------|
| .NET SDK    | 10.0.*             | `dotnet --version` |
| PowerShell (to install Playwright browsers) | 7+ | `pwsh --version` |
| Java (for the Allure CLI) | 17+ | `java -version` |
| Allure CLI  | 2.24+              | `allure --version` |

Install Playwright's browsers once (after the build):

```bash
pwsh Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium   # or "install" for all browsers
```

> **Tip:** you can skip the download and use a browser you already have: `BROWSER=chrome` (Google Chrome) or `BROWSER=msedge` (Microsoft Edge).

---

## 3  Run Everything

```bash
scripts\windows\run_all_tests_gen_report.bat   # Windows
scripts/unix/run_all_tests_gen_report.sh       # macOS/Linux
```

The script builds `Tests/Tests.csproj`, runs the web UI and API tests, and collects the Allure results into `./allure-results`. Use `run_api_tests_gen_report` or `run_web_tests_gen_report` for one area, and `run_ui_web_all_browsers` to run the web tests on Chromium, Firefox and WebKit.

To pick a browser or watch it run:

```bash
BROWSER=webkit HEADLESS=false scripts/unix/run_web_tests_gen_report.sh
```

---

## 4  View the Report

```bash
allure serve allure-results
```

This generates the report in a temporary folder and opens it in your browser.

---

## 5  Troubleshooting

### "Executable doesn't exist" / browser not found

Playwright's browsers aren't installed for the current Playwright version. Run the `playwright.ps1 install` command from step 2, or use `BROWSER=chrome`.

### Firefox won't start on macOS ("Could not find profile folder")

Reinstall Playwright's Firefox build with `pwsh Tests/bin/Debug/net10.0/playwright.ps1 install --force firefox`, or run with another browser.

### "Elasticsearch connection refused"

The framework falls back to **console logging** automatically, so no action is needed.
If you do want Elastic, update `logging-config.json` and make sure your cluster is reachable (`docker compose up -d` starts a local one).

---

## 6  Next Steps

* Explore 👉 `Tests/UI/Web/SauceDemo/CheckoutFlowTests.cs`, `Pages/SauceDemo/` and `Tests/Api/BookingApiPostTests.cs`
* Review the [Architecture doc](ARCHITECTURE.md) for design details.
* Adjust `Tests/testsettings.json` (URLs, browser, credentials) for your environment.
* For mobile tests, follow [MOBILE_ARCHITECTURE.md](MOBILE_ARCHITECTURE.md) to set up Appium and an emulator.
