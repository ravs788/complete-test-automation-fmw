#!/bin/bash
# ============================================================================
# Script: run_all_tests_gen_report.sh
# Purpose:
#   • Run both API and UI/Web test suites and create a merged Allure report
#   • Common workflow:
#       0. Kill orphaned WebDriver processes
#       1. Clean projects and old Allure result folders
#       2. Build projects
#       3. Execute tests (single AllureResultsDirectory each → no duplicates)
#       4. Inject run.timestamp into each environment.properties
#       5. Merge results to root-level allure-results
#       6. Print summary of exit codes and timestamp
# ============================================================================

# Source the framework detection utilities
source sh/detect_net_version.sh

# Get the Allure results directories dynamically
API_ALLURE_RESULTS_DIR=$(get_allure_results_dir "API/API.Tests.csproj")
if [ $? -ne 0 ]; then
    echo "Error: Failed to detect API Allure results directory"
    exit 1
fi

WEB_ALLURE_RESULTS_DIR=$(get_allure_results_dir "UI/Web/UI.Web.Tests.csproj")
if [ $? -ne 0 ]; then
    echo "Error: Failed to detect Web Allure results directory"
    exit 1
fi

# --------------------------------------------------------------------------
# 0. Kill orphaned WebDriver processes
# --------------------------------------------------------------------------
./sh/kill_all_webdrivers.sh

# --------------------------------------------------------------------------
# 1. Clean projects and previous Allure folders
# --------------------------------------------------------------------------
echo "Cleaning test projects..."
dotnet clean API/API.Tests.csproj
CLEAN_API_EXIT=$?
dotnet clean UI/Web/UI.Web.Tests.csproj
CLEAN_WEB_EXIT=$?

echo "Removing previous Allure result folders..."
if [ -d "$API_ALLURE_RESULTS_DIR" ]; then
    rm -rf "$API_ALLURE_RESULTS_DIR"
fi

if [ -d "$WEB_ALLURE_RESULTS_DIR" ]; then
    rm -rf "$WEB_ALLURE_RESULTS_DIR"
fi

if [ -d "allure-results" ]; then
    rm -rf allure-results
fi

# --------------------------------------------------------------------------
# 2. Build projects
# --------------------------------------------------------------------------
echo "Building projects..."
dotnet build API/API.Tests.csproj --no-restore
BUILD_API_EXIT=$?
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_WEB_EXIT=$?

# --------------------------------------------------------------------------
# 3. Run tests
# --------------------------------------------------------------------------
echo "Running API tests..."
dotnet test API/API.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=APITests.trx" \
  /p:AllureResultsDirectory="$API_ALLURE_RESULTS_DIR"
TEST_API_EXIT=$?

echo "Running UI.Web tests..."
dotnet test UI/Web/UI.Web.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=WebTests.trx" \
  --test-adapter-path:. \
  /p:AllureResultsDirectory="$WEB_ALLURE_RESULTS_DIR"
TEST_WEB_EXIT=$?

# --------------------------------------------------------------------------
# 4. Add timestamp to both result folders
# --------------------------------------------------------------------------
echo "Adding timestamp to Allure results..."
TS=$(date +"%Y-%m-%d_%H-%M-%S")
echo "run.timestamp=$TS" > "$API_ALLURE_RESULTS_DIR/environment.properties"
echo "run.timestamp=$TS" > "$WEB_ALLURE_RESULTS_DIR/environment.properties"
TIMESTAMP=$TS

# --------------------------------------------------------------------------
# 5. Merge results to root allure-results
# --------------------------------------------------------------------------
echo "Merging Allure result sets..."
mkdir -p allure-results
cp -r "$API_ALLURE_RESULTS_DIR"/* allure-results/
cp -r "$WEB_ALLURE_RESULTS_DIR"/* allure-results/
COPY_EXIT=$?

# --------------------------------------------------------------------------
# Summary
# --------------------------------------------------------------------------
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
