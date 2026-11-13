#!/usr/bin/env bash

# ============================================================================
# Script: run_web_tests_gen_report.sh
# Purpose:
#   1. Kill stray WebDriver processes
#   2. Clean and build the UI/Web test project
#   3. Execute Web UI tests and generate Allure results
#   4. Add run.timestamp to environment.properties
#   5. Copy generated results to root-level allure-results
# ============================================================================

set +e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT" || exit 1

if [[ -x "tools/dotnet/dotnet" ]]; then
  DOTNET_CMD="tools/dotnet/dotnet"
else
  DOTNET_CMD="dotnet"
fi

WEB_RESULTS_DIR="UI/Web/bin/Debug/net10.0/allure-results"
ROOT_RESULTS_DIR="allure-results"

"$SCRIPT_DIR/kill_all_webdrivers.sh"

echo "Cleaning UI.Web test project..."
"$DOTNET_CMD" clean UI/Web/UI.Web.Tests.csproj
CLEAN_EXIT=$?

if [[ -d "$WEB_RESULTS_DIR" ]]; then
  echo "Removing previous UI Web allure-results folder..."
  rm -rf "$WEB_RESULTS_DIR"
fi

if [[ -d "$ROOT_RESULTS_DIR" ]]; then
  echo "Removing previous root allure-results folder..."
  rm -rf "$ROOT_RESULTS_DIR"
fi

echo "Building UI.Web test project..."
"$DOTNET_CMD" build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_EXIT=$?

echo "Running UI.Web tests..."
"$DOTNET_CMD" test UI/Web/UI.Web.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=WebTests.trx" \
  --test-adapter-path:. \
  /p:AllureResultsDirectory="$WEB_RESULTS_DIR"
TEST_EXIT=$?

echo "Adding timestamp to Allure results..."
mkdir -p "$WEB_RESULTS_DIR"
TIMESTAMP="$(date +"%Y-%m-%d_%H-%M-%S")"
echo "run.timestamp=$TIMESTAMP" > "$WEB_RESULTS_DIR/environment.properties"

echo "Preparing target allure-results directory..."
rm -rf "$ROOT_RESULTS_DIR"
mkdir -p "$ROOT_RESULTS_DIR"

echo "Copying Allure results..."
if [[ -d "$WEB_RESULTS_DIR" ]]; then
  cp -R "$WEB_RESULTS_DIR"/. "$ROOT_RESULTS_DIR"/
  COPY_EXIT=$?
else
  COPY_EXIT=1
fi

echo
echo "===================== SUMMARY ====================="
echo "Clean  step exit code : $CLEAN_EXIT"
echo "Build  step exit code : $BUILD_EXIT"
echo "Test   step exit code : $TEST_EXIT"
echo "Copy   step exit code : $COPY_EXIT"
echo "Run Timestamp        : $TIMESTAMP"
echo "==================================================="
echo

exit "$TEST_EXIT"
