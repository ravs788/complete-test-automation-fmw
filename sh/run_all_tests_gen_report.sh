#!/usr/bin/env bash

# ============================================================================
# Script: run_all_tests_gen_report.sh
# Purpose:
#   Run API and UI/Web test suites and create a merged Allure result folder.
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

API_RESULTS_DIR="API/bin/Debug/net10.0/allure-results"
WEB_RESULTS_DIR="UI/Web/bin/Debug/net10.0/allure-results"
ROOT_RESULTS_DIR="allure-results"

"$SCRIPT_DIR/kill_all_webdrivers.sh"

echo "Cleaning test projects..."
"$DOTNET_CMD" clean API/API.Tests.csproj
CLEAN_API_EXIT=$?
"$DOTNET_CMD" clean UI/Web/UI.Web.Tests.csproj
CLEAN_WEB_EXIT=$?

echo "Removing previous Allure result folders..."
rm -rf "$API_RESULTS_DIR"
rm -rf "$WEB_RESULTS_DIR"
rm -rf "$ROOT_RESULTS_DIR"

echo "Building projects..."
"$DOTNET_CMD" build API/API.Tests.csproj --no-restore
BUILD_API_EXIT=$?
"$DOTNET_CMD" build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_WEB_EXIT=$?

echo "Running API tests..."
"$DOTNET_CMD" test API/API.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=APITests.trx" \
  /p:AllureResultsDirectory="$API_RESULTS_DIR"
TEST_API_EXIT=$?

echo "Running UI.Web tests..."
"$DOTNET_CMD" test UI/Web/UI.Web.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=WebTests.trx" \
  --test-adapter-path:. \
  /p:AllureResultsDirectory="$WEB_RESULTS_DIR"
TEST_WEB_EXIT=$?

echo "Adding timestamp to Allure results..."
TIMESTAMP="$(date +"%Y-%m-%d_%H-%M-%S")"
mkdir -p "$API_RESULTS_DIR" "$WEB_RESULTS_DIR"
echo "run.timestamp=$TIMESTAMP" > "$API_RESULTS_DIR/environment.properties"
echo "run.timestamp=$TIMESTAMP" > "$WEB_RESULTS_DIR/environment.properties"

echo "Merging Allure result sets..."
mkdir -p "$ROOT_RESULTS_DIR"
if [[ -d "$API_RESULTS_DIR" ]]; then
  cp -R "$API_RESULTS_DIR"/. "$ROOT_RESULTS_DIR"/
fi
if [[ -d "$WEB_RESULTS_DIR" ]]; then
  cp -R "$WEB_RESULTS_DIR"/. "$ROOT_RESULTS_DIR"/
fi
COPY_EXIT=$?

echo
echo "===================== SUMMARY ====================="
echo "Clean  API exit code : $CLEAN_API_EXIT"
echo "Clean  WEB exit code : $CLEAN_WEB_EXIT"
echo "Build  API exit code : $BUILD_API_EXIT"
echo "Build  WEB exit code : $BUILD_WEB_EXIT"
echo "Test   API exit code : $TEST_API_EXIT"
echo "Test   WEB exit code : $TEST_WEB_EXIT"
echo "Merge  step exit code: $COPY_EXIT"
echo "Run Timestamp        : $TIMESTAMP"
echo "==================================================="
echo

if [[ "$TEST_API_EXIT" -ne 0 ]]; then
  exit "$TEST_API_EXIT"
fi

exit "$TEST_WEB_EXIT"
