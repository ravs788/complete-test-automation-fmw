#!/bin/bash
# ============================================================================
# Script: run_web_tests_gen_report.sh
# Purpose:
#   1. Kill stray WebDriver processes
#   2. Clean and build the UI/Web test project
#   3. Execute the Web UI tests (Allure JSON files produced in bin folder)
#   4. Inject a timestamp into environment.properties so it appears in Allure
#   5. Copy the results from
#        UI/Web/bin/Debug/{framework}/allure-results/
#      to a top-level folder called
#        allure-results
# ============================================================================

# Source the framework detection utilities
source sh/detect_net_version.sh

# Get the Allure results directory dynamically
WEB_ALLURE_RESULTS_DIR=$(get_allure_results_dir "UI/Web/UI.Web.Tests.csproj")
if [ $? -ne 0 ]; then
    echo "Error: Failed to detect Allure results directory"
    exit 1
fi

# --------------------------------------------------------------------------
# 0. Kill any orphaned browser driver processes
# --------------------------------------------------------------------------
./bat/kill_all_webdrivers.sh

# --------------------------------------------------------------------------
# 1. Clean UI.Web project
# --------------------------------------------------------------------------
echo "Cleaning UI.Web test project..."
dotnet clean UI/Web/UI.Web.Tests.csproj
CLEAN_EXIT=$?

# --------------------------------------------------------------------------
# Additional clean-up: remove previous Allure result folders
# --------------------------------------------------------------------------
if [ -d "$WEB_ALLURE_RESULTS_DIR" ]; then
    echo "Removing previous UI Web allure-results folder..."
    rm -rf "$WEB_ALLURE_RESULTS_DIR"
fi

if [ -d "allure-results" ]; then
    echo "Removing previous root allure-results folder..."
    rm -rf allure-results
fi

# --------------------------------------------------------------------------
# 2. Build UI.Web test project
# --------------------------------------------------------------------------
echo "Building UI.Web test project..."
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_EXIT=$?

# --------------------------------------------------------------------------
# 3. Run UI.Web tests (generates Allure result *.json files)
# --------------------------------------------------------------------------
echo "Running UI.Web tests..."
dotnet test UI/Web/UI.Web.Tests.csproj \
  --no-build \
  --logger "trx;LogFileName=WebTests.trx" \
  --test-adapter-path:. \
  /p:AllureResultsDirectory="$WEB_ALLURE_RESULTS_DIR"
TEST_EXIT=$?

# --------------------------------------------------------------------------
# 4. Add timestamp to Allure results
# --------------------------------------------------------------------------
echo "Adding timestamp to Allure results..."
TS=$(date +"%Y-%m-%d_%H-%M-%S")
echo "run.timestamp=$TS" > "$WEB_ALLURE_RESULTS_DIR/environment.properties"
TIMESTAMP=$TS

# --------------------------------------------------------------------------
# 5. Copy Allure results to top-level allure-results directory
# --------------------------------------------------------------------------
echo "Preparing target allure-results directory..."
mkdir -p allure-results
cp -r "$WEB_ALLURE_RESULTS_DIR"/* allure-results/
COPY_EXIT=$?

# --------------------------------------------------------------------------
# Summary
# --------------------------------------------------------------------------
echo
echo "===================== SUMMARY ====================="
echo "Clean  step exit code : $CLEAN_EXIT"
echo "Build  step exit code : $BUILD_EXIT"
echo "Test   step exit code : $TEST_EXIT"
echo "Copy   step exit code : $COPY_EXIT"
echo "Run Timestamp        : $TIMESTAMP"
echo "==================================================="
echo
