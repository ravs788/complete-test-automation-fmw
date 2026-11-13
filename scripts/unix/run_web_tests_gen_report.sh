#!/usr/bin/env bash
# Run the Playwright web UI tests (SauceDemo + TodoMVC) and collect Allure results.
# Pick the browser with BROWSER=chromium|chrome|msedge|firefox|webkit.
exec "$(dirname "${BASH_SOURCE[0]}")/_run_playwright_tests.sh" "WebTests" "FullyQualifiedName!~Tests.Api."
