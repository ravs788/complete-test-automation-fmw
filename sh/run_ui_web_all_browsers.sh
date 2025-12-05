#!/bin/bash
# ============================================================================
# Script: run_ui_web_all_browsers.sh
# Purpose:
#   • Run UI/Web tests on multiple browsers (Chrome, Firefox, Edge)
#   • Produce separate Allure result sets per browser with
#       run.timestamp and browser name in environment.properties
#   • Merge all results into a single root-level allure-results folder
# ============================================================================

# Source the framework detection utilities
source sh/detect_net_version.sh

# Get the base Allure results directory dynamically
WEB_BASE_ALLURE_DIR=$(get_allure_results_dir "UI/Web/UI.Web.Tests.csproj")
if [ $? -ne 0 ]; then
    echo "Error: Failed to detect Web Allure results directory"
    exit 1
fi

# --------------------------------------------------------------------------
# 0. Kill any orphaned browser driver processes
# --------------------------------------------------------------------------
./bat/kill_all_webdrivers.sh

# --------------------------------------------------------------------------
# 1. Define browsers to test
# --------------------------------------------------------------------------
BROWSERS=("chrome" "firefox" "edge")

# --------------------------------------------------------------------------
# 2. Global clean-up of previous run folders
# --------------------------------------------------------------------------
for browser in "${BROWSERS[@]}"; do
    RESULT_DIR="${WEB_BASE_ALLURE_DIR}-$browser"
    if [ -d "$RESULT_DIR" ]; then
        rm -rf "$RESULT_DIR"
    fi
done

if [ -d "allure-results" ]; then
    rm -rf allure-results
fi

if [ -d "allure-report" ]; then
    rm -rf allure-report
fi

# --------------------------------------------------------------------------
# 3. Clean & build the UI/Web test project once
# --------------------------------------------------------------------------
echo "Cleaning UI.Web test project..."
dotnet clean UI/Web/UI.Web.Tests.csproj
CLEAN_EXIT=$?

echo "Building UI.Web test project..."
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_EXIT=$?

# --------------------------------------------------------------------------
# 4. Execute the test suite for each browser
# --------------------------------------------------------------------------
for browser in "${BROWSERS[@]}"; do
    echo "================================================================"
    echo "Running UI.Web tests on $browser..."
    echo "================================================================"
    
    RESULT_DIR="UI/Web/bin/Debug/net9.0/allure-results-$browser"

    # Run tests for current browser
    dotnet test UI/Web/UI.Web.Tests.csproj \
      --no-build \
      --logger "trx;LogFileName=WebTests_$browser.trx" \
      --test-adapter-path:. \
      /p:BROWSER=$browser
    
    if [ $? -ne 0 ]; then
        echo "$browser run failed"
    fi

    # Copy Allure results to per-browser directory
    if [ -d "UI/Web/bin/Debug/net9.0/allure-results" ]; then
        rm -rf "$RESULT_DIR"
        cp -r "UI/Web/bin/Debug/net9.0/allure-results/" "$RESULT_DIR/"
        rm -rf "UI/Web/bin/Debug/net9.0/allure-results"
    fi

    # Add timestamp & browser info to environment.properties
    TS=$(date +"%Y-%m-%d_%H-%M-%S")
    echo "run.timestamp=$TS" > "$RESULT_DIR/environment.properties"
    echo "browser=$browser" >> "$RESULT_DIR/environment.properties"
done

# --------------------------------------------------------------------------
# 5. Merge all browser result sets
# --------------------------------------------------------------------------
rm -rf allure-results
mkdir -p allure-results

for browser in "${BROWSERS[@]}"; do
    RESULT_DIR="UI/Web/bin/Debug/net9.0/allure-results-$browser"
    if [ -d "$RESULT_DIR" ]; then
        # Copy all files except environment.properties
        find "$RESULT_DIR" -type f ! -name "environment.properties" -exec cp {} allure-results/ \;
        
        # Copy environment.properties as environment.$browser.properties
        if [ -f "$RESULT_DIR/environment.properties" ]; then
            cp "$RESULT_DIR/environment.properties" "allure-results/environment.$browser.properties"
        fi
    fi
done

# --------------------------------------------------------------------------
# Summary
# --------------------------------------------------------------------------
echo
echo "===================== SUMMARY ====================="
echo "Clean step exit code : $CLEAN_EXIT"
echo "Build step exit code : $BUILD_EXIT"
echo "Results merged into  : allure-results"
echo "==================================================="
echo
