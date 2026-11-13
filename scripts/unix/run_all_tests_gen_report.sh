#!/usr/bin/env bash
# Run every Playwright UI + API test and collect Allure results in ./allure-results.
exec "$(dirname "${BASH_SOURCE[0]}")/_run_playwright_tests.sh" "AllTests" ""
