@echo off
REM ============================================================================
REM Script: run_web_tests_gen_report.bat
REM Purpose:
REM   1. Kill stray WebDriver processes
REM   2. Clean and build the UI/Web test project
REM   3. Execute the Web UI tests (Allure JSON files produced in bin folder)
REM   4. Inject a timestamp into environment.properties so it appears in Allure
REM   5. Copy the results from
REM        UI\Web\bin\Debug\net9.0\allure-results\
REM      to a top-level folder called
REM        allure-results
REM ============================================================================

setlocal EnableDelayedExpansion

REM --------------------------------------------------------------------------
REM 0. Kill any orphaned browser driver processes
REM --------------------------------------------------------------------------
call bat\kill_all_webdrivers.bat

REM --------------------------------------------------------------------------
REM 1. Clean UI.Web project
REM --------------------------------------------------------------------------
echo Cleaning UI.Web test project...
dotnet clean UI/Web/UI.Web.Tests.csproj
set CLEAN_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM Additional clean-up: remove previous Allure result folders
REM --------------------------------------------------------------------------
if exist UI\Web\bin\Debug\net9.0\allure-results (
    echo Removing previous UI Web allure-results folder...
    rmdir /S /Q UI\Web\bin\Debug\net9.0\allure-results
)
if exist allure-results (
    echo Removing previous root allure-results folder...
    rmdir /S /Q allure-results
)

REM --------------------------------------------------------------------------
REM 2. Build UI.Web test project
REM --------------------------------------------------------------------------
echo Building UI.Web test project...
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore
set BUILD_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 3. Run UI.Web tests (generates Allure result *.json files)
REM --------------------------------------------------------------------------
echo Running UI.Web tests...
dotnet test UI/Web/UI.Web.Tests.csproj ^
  --no-build ^
  --logger "trx;LogFileName=WebTests.trx" ^
  --test-adapter-path:. ^
  /p:AllureResultsDirectory=UI\Web\bin\Debug\net9.0\allure-results
set TEST_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 4. Add timestamp to Allure results
REM --------------------------------------------------------------------------
echo Adding timestamp to Allure results...
for /f "tokens=1" %%a in ("%date:/=-%") do set TODAY=%%a
for /f "tokens=1" %%a in ("%time: =0%") do set NOW=%%a
set TS=!TODAY!_!NOW!
set TS=!TS::=-!
echo run.timestamp=!TS!> UI\Web\bin\Debug\net9.0\allure-results\environment.properties
set TIMESTAMP=!TS!

REM --------------------------------------------------------------------------
REM 5. Copy Allure results to top-level allure-results directory
REM --------------------------------------------------------------------------
echo Preparing target allure-results directory...
mkdir allure-results
xcopy /E /I /Y UI\Web\bin\Debug\net9.0\allure-results\* allure-results\
set COPY_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM Summary
REM --------------------------------------------------------------------------
echo(
echo ===================== SUMMARY =====================
echo Clean  step exit code : %CLEAN_EXIT%
echo Build  step exit code : %BUILD_EXIT%
echo Test   step exit code : %TEST_EXIT%
echo Copy   step exit code : %COPY_EXIT%
echo Run Timestamp        : %TIMESTAMP%
echo ===================================================
echo(
endlocal
