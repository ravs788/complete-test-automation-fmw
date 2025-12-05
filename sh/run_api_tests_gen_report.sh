#!/bin/bash
# ============================================================================
# Script: run_api_tests_gen_report.sh
# Purpose:
#   1. Clean the API test project / solution
#   2. Execute the API tests (Allure JSON files are produced automatically
#      inside the bin folder by the Allure adapter)
#   3. Copy the generated Allure results from
#        API/bin/Debug/{framework}/allure-results/
#      to a top-level folder called
#        allure-results
# ============================================================================

# Source the framework detection utilities
source sh/detect_net_version.sh

# --------------------------------------------------------------------------
# 1. Clean API solution / project
# --------------------------------------------------------------------------
echo "Cleaning API solution / project..."
dotnet clean API/API.Tests.csproj
CLEAN_EXIT=$?

# Get the Allure results directory dynamically
API_ALLURE_RESULTS_DIR=$(get_allure_results_dir "API/API.Tests.csproj")
if [ $? -ne 0 ]; then
    echo "Error: Failed to detect Allure results directory"
    exit 1
fi

# --------------------------------------------------------------------------
# Additional clean-up: remove previous Allure result folders
# --------------------------------------------------------------------------
if [ -d "$API_ALLURE_RESULTS_DIR" ]; then
    echo "Removing previous API allure-results folder..."
    rm -rf "$API_ALLURE_RESULTS_DIR"
fi

if [ -d "allure-results" ]; then
    echo "Removing previous root allure-results folder..."
    rm -rf allure-results
fi

# --------------------------------------------------------------------------
# 2. Build API test project
# --------------------------------------------------------------------------
echo "Building API test project..."
dotnet build API/API.Tests.csproj --no-restore
BUILD_EXIT=$?

# --------------------------------------------------------------------------
# 3. Run API tests (this generates the Allure result *.json files)
#    NOTE:
#      • --no-build  → skip build because we already cleaned
#      • --logger    → keep trx for Azure DevOps / any CI reporting
#      • /p:AllureResultsDirectory → tell Allure adapter where to drop results
# --------------------------------------------------------------------------
echo "Running API tests..."
dotnet test API/API.Tests.csproj --no-build --logger "trx;LogFileName=APITests.trx" /p:AllureResultsDirectory="$API_ALLURE_RESULTS_DIR"
TEST_EXIT=$?

# --------------------------------------------------------------------------
# 4. Add timestamp to Allure results
# --------------------------------------------------------------------------
echo "Adding timestamp to Allure results..."
TS=$(date +"%Y-%m-%d_%H-%M-%S")
echo "run.timestamp=$TS" > "$API_ALLURE_RESULTS_DIR/environment.properties"
TIMESTAMP=$TS

# --------------------------------------------------------------------------
# 5. Copy Allure results to top-level allure-results directory
# --------------------------------------------------------------------------
echo "Preparing target allure-results directory..."
if [ -d "allure-results" ]; then
    rm -rf allure-results
fi
mkdir -p allure-results

echo "Copying Allure results..."
cp -r "$API_ALLURE_RESULTS_DIR"/* allure-results/
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
