#!/usr/bin/env bash
# ============================================================================
# Shared by the run_*_gen_report.sh scripts.
# Builds Tests/Tests.csproj, runs it with an optional `dotnet test --filter`,
# stamps run.timestamp/browser into environment.properties, and copies the
# Allure results (plus categories.json) to ./allure-results.
#
# Usage: _run_playwright_tests.sh <label> [filter]
# Env:   BROWSER=chromium|chrome|msedge|firefox|webkit, HEADLESS=true|false
# ============================================================================

set +e

LABEL="${1:?label required}"
FILTER="${2:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$REPO_ROOT" || exit 1

if [[ -x "tools/dotnet/dotnet" ]]; then
  DOTNET_CMD="tools/dotnet/dotnet"
else
  DOTNET_CMD="dotnet"
fi

PROJECT="Tests/Tests.csproj"
RESULTS_DIR="Tests/bin/Debug/net10.0/allure-results"
ROOT_RESULTS_DIR="allure-results"

echo "Removing previous Allure results..."
rm -rf "$RESULTS_DIR" "$ROOT_RESULTS_DIR"

echo "Building $PROJECT..."
"$DOTNET_CMD" build "$PROJECT"
BUILD_EXIT=$?

FILTER_ARGS=()
if [[ -n "$FILTER" ]]; then
  FILTER_ARGS=(--filter "$FILTER")
fi

echo "Running $LABEL tests${FILTER:+ (filter: $FILTER)}${BROWSER:+ on $BROWSER}..."
"$DOTNET_CMD" test "$PROJECT" --no-build "${FILTER_ARGS[@]}" --logger "trx;LogFileName=${LABEL}.trx"
TEST_EXIT=$?

TIMESTAMP="$(date +"%Y-%m-%d_%H-%M-%S")"
mkdir -p "$RESULTS_DIR" "$ROOT_RESULTS_DIR"
{
  echo "run.timestamp=$TIMESTAMP"
  echo "browser=${BROWSER:-testsettings.json}"
} > "$RESULTS_DIR/environment.properties"

cp -R "$RESULTS_DIR"/. "$ROOT_RESULTS_DIR"/
COPY_EXIT=$?
cp allure-categories.json "$ROOT_RESULTS_DIR/categories.json" 2>/dev/null

echo
echo "===================== SUMMARY ====================="
echo "Build  exit code : $BUILD_EXIT"
echo "Test   exit code : $TEST_EXIT"
echo "Copy   exit code : $COPY_EXIT"
echo "Run Timestamp    : $TIMESTAMP"
echo "Results          : $ROOT_RESULTS_DIR (allure serve $ROOT_RESULTS_DIR)"
echo "==================================================="

exit "$TEST_EXIT"
