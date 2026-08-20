@echo off
REM ============================================================================
REM Script: run_ui_web_all_browsers.bat
REM Purpose:
REM   • Run UI/Web tests on multiple browsers (Chrome, Firefox, Edge)
REM   • Produce separate Allure result sets per browser with
REM       run.timestamp and browser name in environment.properties
REM   • Merge all results into a single root-level allure-results folder
REM ============================================================================

setlocal EnableDelayedExpansion

REM --------------------------------------------------------------------------
REM 0. Kill any orphaned browser driver processes
REM --------------------------------------------------------------------------
call bat\kill_all_webdrivers.bat

REM --------------------------------------------------------------------------
REM 1. Define browsers to test
REM --------------------------------------------------------------------------
set BROWSERS=chrome firefox edge

REM --------------------------------------------------------------------------
REM 2. Global clean-up of previous run folders
REM --------------------------------------------------------------------------
for %%b in (%BROWSERS%) do (
    if exist UI\Web\bin\Debug\net10.0\allure-results-%%b rmdir /S /Q UI\Web\bin\Debug\net10.0\allure-results-%%b
)
if exist allure-results rmdir /S /Q allure-results
if exist allure-report rmdir /S /Q allure-report

REM --------------------------------------------------------------------------
REM [Dotnet Resolution] Prefer local bundled dotnet, fallback to global
REM --------------------------------------------------------------------------
set "LOCAL_DOTNET=tools\dotnet\dotnet.exe"
if exist "%LOCAL_DOTNET%" (
    set "DOTNET_CMD=%LOCAL_DOTNET%"
) else (
    set "DOTNET_CMD=dotnet"
)

REM --------------------------------------------------------------------------
REM 3. Clean & build the UI/Web test project once
REM --------------------------------------------------------------------------
echo Cleaning UI.Web test project...
%DOTNET_CMD% clean UI/Web/UI.Web.Tests.csproj
set CLEAN_EXIT=%ERRORLEVEL%

echo Building UI.Web test project...
%DOTNET_CMD% build UI/Web/UI.Web.Tests.csproj --no-restore
set BUILD_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 4. Execute the test suite for each browser
REM --------------------------------------------------------------------------
for %%b in (%BROWSERS%) do (
    echo ================================================================
    echo Running UI.Web tests on %%b...
    echo ================================================================
    set BROWSER=%%b
    set RESULT_DIR=UI\Web\bin\Debug\net10.0\allure-results-%%b    

    REM Run tests for current browser
    %DOTNET_CMD% test UI/Web/UI.Web.Tests.csproj ^
      --no-build ^
      --logger "trx;LogFileName=WebTests_%%b.trx" ^
      --test-adapter-path:. ^
      /p:BROWSER=%%b
    if errorlevel 1 echo %%b run failed

    REM Copy Allure results to per-browser directory
    if exist UI\Web\bin\Debug\net10.0\allure-results rmdir /S /Q !RESULT_DIR!
    if exist UI\Web\bin\Debug\net10.0\allure-results xcopy /E /I /Y UI\Web\bin\Debug\net10.0\allure-results !RESULT_DIR! >nul
    if exist UI\Web\bin\Debug\net10.0\allure-results rmdir /S /Q UI\Web\bin\Debug\net10.0\allure-results

    REM Add timestamp & browser info to environment.properties
    for /f "tokens=1" %%d in ("%date:/=-%") do set TODAY=%%d
    for /f "tokens=1" %%t in ("%time: =0%") do set NOW=%%t
    set TS=!TODAY!_!NOW!
    set TS=!TS::=-!
    (
        echo run.timestamp=!TS!
        echo browser=%%b
    )> !RESULT_DIR!\environment.properties
)

REM --------------------------------------------------------------------------
REM 5. Merge all browser result sets (workaround: mass-copy, risk overwrite on uuid clash!)
REM --------------------------------------------------------------------------
rmdir /S /Q allure-results
mkdir allure-results
for %%b in (%BROWSERS%) do (
    xcopy /E /I /Y /EXCLUDE:bat\exclude_envprops.txt UI\Web\bin\Debug\net10.0\allure-results-%%b\* allure-results\ >nul
    if exist UI\Web\bin\Debug\net10.0\allure-results-%%b\environment.properties (
        copy /Y UI\Web\bin\Debug\net10.0\allure-results-%%b\environment.properties allure-results\environment.%%b.properties >nul
    )
)


REM --------------------------------------------------------------------------
REM Summary
REM --------------------------------------------------------------------------
echo(
echo ===================== SUMMARY =====================
echo Clean step exit code : %CLEAN_EXIT%
echo Build step exit code : %BUILD_EXIT%
echo Results merged into  : allure-results
echo ===================================================
echo(
endlocal
