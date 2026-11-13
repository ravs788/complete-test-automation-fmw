#!/usr/bin/env bash
# ============================================================================
# Run the Playwright web UI tests once per browser and merge the Allure results
# into ./allure-results (one environment.<browser>.properties per browser).
# Browsers: $BROWSERS (default "chromium firefox webkit"). Install Playwright's
# browsers first: pwsh Tests/bin/Debug/net10.0/playwright.ps1 install
# ============================================================================

set +e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$REPO_ROOT" || exit 1

read -r -a BROWSER_LIST <<< "${BROWSERS:-chromium firefox webkit}"
MERGED_DIR="$(mktemp -d)"
OVERALL_EXIT=0

for browser in "${BROWSER_LIST[@]}"; do
  echo "================================================================"
  echo "Running web UI tests on $browser..."
  echo "================================================================"
  BROWSER="$browser" "$SCRIPT_DIR/_run_playwright_tests.sh" "WebTests_$browser" "FullyQualifiedName!~Tests.Api."
  code=$?
  [[ $code -ne 0 ]] && OVERALL_EXIT=$code && echo "$browser run failed (exit $code)"
  mv allure-results/environment.properties "allure-results/environment.$browser.properties" 2>/dev/null
  cp -R allure-results/. "$MERGED_DIR"/
done

rm -rf allure-results
mv "$MERGED_DIR" allure-results
echo
echo "Merged results for: ${BROWSER_LIST[*]} -> allure-results (allure serve allure-results)"
exit "$OVERALL_EXIT"
