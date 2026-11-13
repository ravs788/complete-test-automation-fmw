#!/usr/bin/env bash
# Run the Restful Booker API tests (Playwright APIRequestContext) and collect Allure results.
exec "$(dirname "${BASH_SOURCE[0]}")/_run_playwright_tests.sh" "ApiTests" "FullyQualifiedName~Tests.Api."
