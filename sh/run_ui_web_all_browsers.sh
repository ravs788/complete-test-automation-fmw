#!/usr/bin/env bash

# ============================================================================
# Script: run_ui_web_all_browsers.sh
# Purpose:
#   Run UI/Web tests on Chrome, Firefox, and Edge, then merge Allure results.
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

BROWSERS=("chrome" "firefox" "edge")
WEB_OUTPUT_DIR="UI/Web/bin/Debug/net10.0"
ROOT_RESULTS_DIR="allure-results"

"$SCRIPT_DIR/kill_all_webdrivers.sh"

for browser in "${BROWSERS[@]}"; do
  rm -rf "$WEB_OUTPUT_DIR/allure-results-$browser"
done
rm -rf "$ROOT_RESULTS_DIR" "allure-report"

echo "Cleaning UI.Web test project..."
"$DOTNET_CMD" clean UI/Web/UI.Web.Tests.csproj
CLEAN_EXIT=$?

echo "Building UI.Web test project..."
"$DOTNET_CMD" build UI/Web/UI.Web.Tests.csproj --no-restore
BUILD_EXIT=$?

declare -A TEST_EXITS

for browser in "${BROWSERS[@]}"; do
  echo "================================================================"
  echo "Running UI.Web tests on $browser..."
  echo "================================================================"

  RESULT_DIR="$WEB_OUTPUT_DIR/allure-results-$browser"
  CURRENT_RESULTS_DIR="$WEB_OUTPUT_DIR/allure-results"

  "$DOTNET_CMD" test UI/Web/UI.Web.Tests.csproj \
    --no-build \
    --logger "trx;LogFileName=WebTests_$browser.trx" \
    --test-adapter-path:. \
    /p:BROWSER="$browser"
  TEST_EXITS["$browser"]=$?

  if [[ "${TEST_EXITS[$browser]}" -ne 0 ]]; then
    echo "$browser run failed"
  fi

  rm -rf "$RESULT_DIR"
  if [[ -d "$CURRENT_RESULTS_DIR" ]]; then
    cp -R "$CURRENT_RESULTS_DIR" "$RESULT_DIR"
    rm -rf "$CURRENT_RESULTS_DIR"
  else
    mkdir -p "$RESULT_DIR"
  fi

  TIMESTAMP="$(date +"%Y-%m-%d_%H-%M-%S")"
  {
    echo "run.timestamp=$TIMESTAMP"
    echo "browser=$browser"
  } > "$RESULT_DIR/environment.properties"
done

mkdir -p "$ROOT_RESULTS_DIR"
for browser in "${BROWSERS[@]}"; do
  RESULT_DIR="$WEB_OUTPUT_DIR/allure-results-$browser"
  if [[ -d "$RESULT_DIR" ]]; then
    find "$RESULT_DIR" -mindepth 1 -maxdepth 1 ! -name "environment.properties" -exec cp -R {} "$ROOT_RESULTS_DIR"/ \;
  fi

  if [[ -f "$RESULT_DIR/environment.properties" ]]; then
    cp "$RESULT_DIR/environment.properties" "$ROOT_RESULTS_DIR/environment.$browser.properties"
  fi
done

echo
echo "===================== SUMMARY ====================="
echo "Clean step exit code : $CLEAN_EXIT"
echo "Build step exit code : $BUILD_EXIT"
for browser in "${BROWSERS[@]}"; do
  echo "Test  $browser exit code : ${TEST_EXITS[$browser]}"
done
echo "Results merged into  : $ROOT_RESULTS_DIR"
echo "==================================================="
echo

for browser in "${BROWSERS[@]}"; do
  if [[ "${TEST_EXITS[$browser]}" -ne 0 ]]; then
    exit "${TEST_EXITS[$browser]}"
  fi
done

exit 0
