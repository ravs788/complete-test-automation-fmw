# Mobile Automation Framework (Appium) — Architecture and Plan

Purpose: Define a concrete, reusable Mobile automation layer for both Native apps and Mobile Web using Appium, aligned with the existing Core layer for logging, reporting, screenshots, and config.

This document is the single source of truth for the Mobile layer architecture, conventions, and the step-by-step implementation plan.

- Status: Proposed plan (ready to implement)
- Stack: .NET (C#), NUnit, Allure, Appium v2, Android (local), iOS (optional via macOS or cloud)

## Table of Contents
- Goals and Scope
- Assumptions
- Architecture Overview
  - Key Abstractions
  - Driver Bootstrap Flow
- Project Structure
- Tooling and Dependencies
  - NuGet Packages
  - Appium Server and Drivers
  - Devices
  - Browsers (Mobile Web)
- Cross-Platform Strategy
  - Page Contracts and Implementations
  - Locators
  - Base Page Reuse
  - Waits and Gestures
- Configuration Model
  - Files and Selection
  - Example Config (Android Native/Web, iOS)
- Implementation Plan and Milestones
- CI/CD Integration
  - Local Android
  - iOS/macOS and Cloud Providers
- Reuse from Core Layer
- Deliverables
- Open Items to Confirm
- Getting Started (Local Setup)
  - Prerequisites
  - Install Appium and Drivers
  - Start Appium and Emulator
  - Run Tests
- Next Steps

---

## Goals and Scope
- Build a unified Mobile layer that supports:
  - Native apps (Android, iOS)
  - Mobile web (Android Chrome, iOS Safari)
- Cleanly separate:
  - Driver bootstrap (capabilities/options and session lifecycle)
  - Test fixtures (setup/teardown, logging, failure handling)
  - Page objects and flows
  - Utilities (waits, gestures, screenshots)
  - Configuration and test data
- Reuse existing Core services for consistency across API, Web, and Mobile layers.

## Assumptions
- .NET with NUnit test framework (UI/Web uses NUnit + Allure).
- Allure reporting continues.
- Appium v2 and Selenium.Appium 5.x.
- Android support locally (Windows). iOS requires macOS or a cloud provider (optional).
- Tests will be organized under `UI/Mobile/` in the same solution.

## Architecture Overview

### Key Abstractions
- Enums:
  - `MobilePlatform`: Android, iOS
  - `MobileAppType`: Native, Web
- Driver bootstrap:
  - `IMobileDriverFactory`: Creates an Appium session per test given platform + app type.
  - `MobileOptionsBuilder`: Converts our config into platform-appropriate `AppiumOptions`.
  - `MobileSessionManager` (optional): Manages lifetime/singleton per test class if needed.
- Test base:
  - `BaseMobileTest`: Reads config via Core ConfigLoader, initializes driver via factory, wires Core logging, attaches screenshots on failure, and handles teardown.
- Page layer:
  - Contracts (interfaces) for page behavior (e.g., `ILoginPage`).
  - Implementations:
    - Native Android pages
    - Native iOS pages (optional later)
    - Mobile Web pages (shared logic with Web where feasible)
  - Base classes:
    - `AndroidBasePage`, `IOSBasePage`
    - `MobileWebBasePage` (IWebDriver-like surface)
- Utilities:
  - `WaitHelper`: Explicit waits with resilience
  - `GestureHelper`: W3C actions for swipe/scroll/tap/long-press
  - `AssertHelper`: Optional, or reuse/generalize UI/Web’s
  - Screenshots: Reuse Core `IScreenshotHelper` and/or add a mobile adapter

### Driver Bootstrap Flow
1. `BaseMobileTest` loads `UI/Mobile/config.json` (or environment-specific file) using `Core.Utilities.Config.ConfigLoader`.
2. `MobileOptionsBuilder` builds `AppiumOptions` from config (platform, appType).
3. `MobileDriverFactory` uses options + server URL to create an `AndroidDriver` or `IOSDriver`.
4. Test runs. On failure, screenshot + logs go to Allure via Core services.
5. Teardown quits the session.

## Project Structure
Proposed layout within `UI/Mobile/`:

- Pages/
  - Contracts/
    - ILoginPage.cs, IHomePage.cs, ...
  - Native/
    - Android/
      - AndroidBasePage.cs
      - LoginPage.Android.cs, HomePage.Android.cs, ...
    - iOS/ (optional)
      - IOSBasePage.cs
      - LoginPage.iOS.cs, HomePage.iOS.cs, ...
  - Web/
    - MobileWebBasePage.cs
    - LoginPage.MobileWeb.cs, HomePage.MobileWeb.cs, ...
- Drivers/
  - IMobileDriverFactory.cs
  - MobileDriverFactory.cs
  - MobileOptionsBuilder.cs
  - MobileSessionManager.cs (optional)
- Utilities/
  - ConfigSettings.cs (mobile-specific config model; uses Core ConfigLoader)
  - WaitHelper.cs
  - GestureHelper.cs
  - AssertHelper.cs (optional, or reuse Web)
  - AllureScreenshotHelper.cs (thin wrapper using Core services)
- Tests/
  - BaseMobileTest.cs
  - Samples/
    - AndroidNative/
      - SmokeLoginTests.cs
    - AndroidWeb/
      - SmokeMobileWebTests.cs
    - (iOS counterparts later)
- config.json (and environment variants)
- test-data/
  - JSON data for sample tests

## Tooling and Dependencies

### NuGet Packages
- Selenium.WebDriver 4.x
- Selenium.Support 4.x (if needed)
- Appium.WebDriver (Selenium.Appium) 5.x
- NUnit + NUnit3TestAdapter (aligned with existing layers)
- Allure: Allure.NUnit and Allure.Commons (already in solution; ensure referenced)

### Appium Server and Drivers
- Appium v2 (Node)
- Android: uiautomator2 driver
- iOS: xcuitest driver (macOS only)

### Devices
- Android: Real devices or emulators (ADB).
- iOS: Simulators/real devices on macOS, or cloud providers (BrowserStack, Sauce Labs, etc.).

### Browsers (Mobile Web)
- Android: Chrome via Appium (Chromedriver managed internally).
- iOS: Safari via Appium (macOS required).

## Cross-Platform Strategy

### Page Contracts and Implementations
- Define behavior contracts in `Pages/Contracts` (e.g., `ILoginPage.Login`, `IsLoaded`).
- Implement:
  - `LoginPage.Android.cs` (native Android)
  - `LoginPage.iOS.cs` (native iOS) — later
  - `LoginPage.MobileWeb.cs` (mobile web in Chrome/Safari)

### Locators
- Native:
  - Prefer `AccessibilityId` first (cross-platform friendly)
  - Android fallbacks: `resource-id`, `UiSelector`
  - iOS fallbacks: predicate/class chain
- Mobile Web:
  - Reuse CSS/XPath strategies similar to UI/Web

### Base Page Reuse
- Reuse patterns from `UI/Web/Pages/BasePage.cs` where feasible:
  - For Mobile Web, a similar base with IWebDriver works well.
  - For Native, AppiumDriver surface needs extensions/wrappers.

### Waits and Gestures
- Centralize explicit waits in `WaitHelper` with retry-on-stale.
- `GestureHelper` for swipe/scroll/tap/long press using W3C actions or platform specifics (e.g., Android UiScrollable).

## Configuration Model

### Files and Selection
- Default: `UI/Mobile/config.json`
- Environment-specific variants:
  - `config.android.native.json`
  - `config.android.web.json`
  - `config.ios.native.json`
  - `config.ios.web.json`
- Selection strategy:
  - Environment variable (e.g., `MOBILE_CONFIG_PATH`)
  - Or test run parameter (runsettings) that `ConfigLoader` uses to pick the file

### Example Config (consolidated)
```json
{
  "server": {
    "url": "http://127.0.0.1:4723/",
    "commandTimeoutSec": 120,
    "newCommandTimeoutSec": 120
  },
  "platform": "Android", // Android | iOS
  "appType": "Native",   // Native | Web
  "device": {
    "deviceName": "emulator-5554",
    "platformVersion": "14",
    "udid": "",
    "orientation": "portrait"
  },
  "android": {
    "automationName": "UiAutomator2",
    "app": "C:/path/to/app.apk",
    "appPackage": "com.example.app",
    "appActivity": ".MainActivity",
    "browserName": "Chrome"
  },
  "ios": {
    "automationName": "XCUITest",
    "app": "/path/to/app.app",
    "bundleId": "com.example.ios",
    "browserName": "Safari"
  },
  "behavior": {
    "noReset": true,
    "fullReset": false,
    "acceptAlertsAutomatically": true
  },
  "logging": {
    "provider": "console"
  },
  "testData": {
    "basePath": "UI/Mobile/test-data"
  }
}
```

## Implementation Plan and Milestones

- Milestone 1: Foundation and Skeleton
  - Create folder structure under `UI/Mobile/`
  - Add NuGet dependencies to `UI.Mobile.Tests.csproj`
  - Implement `ConfigSettings` (mobile model) and wire to Core `ConfigLoader`
  - Implement `MobilePlatform`, `MobileAppType`
  - Implement `MobileOptionsBuilder` and `MobileDriverFactory` (Android Native + Android Web)
  - Implement `BaseMobileTest`:
    - Read config
    - Initialize driver
    - Wire Core Logging
    - On failure: capture screenshot to Allure
  - ADB/device sanity checks with console logs

- Milestone 2: Base Pages and Utilities
  - Implement `AndroidBasePage`, `MobileWebBasePage`
  - Implement `WaitHelper` and `GestureHelper`
  - Reuse/extend UI/Web `AssertHelper` if applicable
  - Mobile-friendly screenshot helper that uses Core’s services with driver capture

- Milestone 3: Sample Tests and Data
  - Android Native sample (e.g., demo app flow)
  - Android Web sample (e.g., saucedemo mobile login flow)
  - JSON test-data mirroring UI/Web patterns
  - Batch scripts:
    - `bat/run_mobile_android_native.bat`
    - `bat/run_mobile_android_web.bat`
  - Allure result publication consistent with other layers

- Milestone 4: iOS Enablement (optional)
  - iOS options in `MobileOptionsBuilder`
  - iOS native + web page implementations
  - iOS sample tests
  - Scripts or CI workflows for macOS runners
  - Cloud support (BrowserStack/Sauce) with config keys and factory toggles

- Milestone 5: Hardening and Parallelization
  - Retry-on-stale waits
  - Safe action wrappers (click/sendkeys with retries)
  - Parallel strategy: device reservation per test class (udid/emulators)
  - Enriched logging using Core `LogMetadata`; attach session caps to logs
  - Documentation updates in `/docs` and this README

## CI/CD Integration

### Android (Windows runner feasible)
- Install prerequisites:
  - Java JDK, Android SDK, Node.js
  - Appium v2 (npm), `uiautomator2` driver
- Start an emulator (or connect a real device)
- Start Appium server
- `dotnet test UI/Mobile/UI.Mobile.Tests.csproj` with desired config
- Publish Allure results

### iOS (macOS or Cloud)
- macOS runners required for local iOS:
  - Xcode tools, Node, Appium `xcuitest` driver
  - iOS simulators or real devices provisioning
- Cloud providers:
  - Add Remote Grid URL and required capability keys (`browserstack.user`, `browserstack.key`, `app`, `device`, etc.)
  - Same `BaseMobileTest`; only config changes

## Reuse from Core Layer
- `Core/Utilities/Logging/*`
  - Use `ILoggingService` to log session start/stop, device info, steps
- `Core/Utilities/Screenshots/*`
  - Use `IScreenshotHelper`/`AllureScreenshotHelper` to attach PNGs from Appium’s `GetScreenshot()`
- `Core/Utilities/TestData/*`
  - Use `TestDataLoader` for JSON-driven scenarios
- `Core/Utilities/Config/*`
  - Use `ConfigLoader` to load mobile configs
- `Core/Utilities/Reporting/*`
  - Keep results publishing consistent (Allure)

## Deliverables
- Code:
  - `UI/Mobile/Drivers/*`: options builder, factory, optional session manager
  - `UI/Mobile/Utilities/*`: waits, gestures, asserts (if needed), screenshots
  - `UI/Mobile/Pages/*`: contracts + Android native + Mobile web; iOS placeholders
  - `UI/Mobile/Tests/*`: base test + sample native and web tests
  - `UI/Mobile/config*.json` variants
  - `UI/Mobile/test-data/*`
- Scripts:
  - `bat/run_mobile_android_native.bat`
  - `bat/run_mobile_android_web.bat`
- Docs:
  - This README (kept up to date)
  - Optional: `docs/Mobile.md` for extended setup and CI details

## Open Items to Confirm
- Platforms:
  - Is iOS needed immediately? (Requires macOS or cloud)
- Devices:
  - Real devices vs emulators? Should CI spin up emulators automatically?
- App under test:
  - Do we have an Android `.apk` and/or iOS `.ipa/.app`? Otherwise start with a public sample app.
  - Mobile web: which URLs/environments?
- Cloud provider:
  - BrowserStack/Sauce/BitBar/etc.? If yes, which one?
- Test framework:
  - Confirm NUnit remains standard for Mobile (aligned with UI/Web).

## Getting Started (Local Setup)

### Prerequisites
- Java JDK (set `JAVA_HOME`)
- Android SDK (set `ANDROID_HOME` or `ANDROID_SDK_ROOT`, add `platform-tools` to PATH)
- Node.js (LTS)
- .NET SDK (matching solution)
- AVD(s) created via Android Studio or `avdmanager`

### Install Appium and Drivers
```bash
npm i -g appium
appium -v

# Install Android driver
appium driver install uiautomator2
appium driver list
```

### Start Appium and Emulator
- Start an Android emulator via Android Studio or:
```bash
# Example; replace with your AVD name
emulator -avd Pixel_4_API_34
```
- Start Appium server (default 0.0.0.0:4723):
```bash
appium
```

### Run Tests
- Ensure `UI/Mobile/config.android.native.json` (or `config.android.web.json`) is set correctly.
- Optionally set `MOBILE_CONFIG_PATH` to point to a specific config.
- Run:
```bash
dotnet test UI/Mobile/UI.Mobile.Tests.csproj
```

## Next Steps
1. Implement Milestone 1 (foundation, configs, factories, base test).
2. Add base pages/utilities and initial Android sample tests.
3. Wire batch scripts and Allure result publication.
4. Extend to iOS and/or cloud based on needs.
