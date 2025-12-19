# Getting Started Guide

This page walks through running the framework **locally** with extra tips, screenshots, and troubleshooting notes.  
If you only need the quick 4-step version, see the “🚀 Getting Started in 5 Minutes” section in the root README.

---

## 1  Clone & Restore

```bash
git clone https://github.com/ravs788/complete-test-automation-fmw.git
cd complete-test-automation-fmw
dotnet restore
```

![Clone & restore](./images/getting_started/clone_and_restore.png)

---

## 2  Verify Prerequisites

| Requirement | Recommended Version | Check Command |
|-------------|--------------------|---------------|
| .NET SDK    | 9.0.*              | `dotnet --version` |
| Java (for Allure) | 17+          | `java -version` |
| Allure CLI  | 2.24+             | `allure --version` |
| Browser Driver | ChromeDriver matching your Chrome | `chromedriver --version` |

> **Tip:** If `chromedriver` is missing, place it on your `PATH` or set the `WEBDRIVER_CHROME_DRIVER` env var.

---

## 3  Run Everything

```bash
bat\run_all_tests_gen_report.bat          # Windows
sh/run_all_tests_gen_report.sh            # macOS/Linux
```

On first run, dependencies will compile – expect 1-2 minutes.

![Running tests](./images/getting_started/running_tests.png)

---

## 4  View the Report

After the script completes:

```bash
allure open allure-report
```

> The report auto-opens in your browser on Windows.  
> On macOS/Linux open `http://localhost:5050/` if it doesn’t launch.

![Allure report](./images/getting_started/allure_report.png)

---

## 5  Troubleshooting

### “Cannot find Chrome binary”

Set the environment variable:

```bash
set CHROME_BINARY=C:\Program Files\Google\Chrome\Application\chrome.exe   # Windows
export CHROME_BINARY=/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome   # macOS
```

### “Elasticsearch connection refused”

The framework falls back to **console logging** automatically – no action needed.  
If you do want Elastic, update `logging-config.json` and ensure your cluster is reachable.

---

## 6  Next Steps

* Explore 👉 `UI/Web/Tests/SauceDemoTests.cs` and `API/Tests/BookingApiPostTests.cs`
* Review the [Architecture doc](ARCHITECTURE.md) for design details.
* Customize `config.json` in each project for your environment.
