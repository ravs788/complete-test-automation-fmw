@echo off
REM ============================================================================
REM Shared by the run_*_gen_report.bat scripts.
REM Builds Tests\Tests.csproj, runs it with an optional dotnet test filter,
REM stamps run.timestamp/browser into environment.properties, and copies the
REM Allure results (plus categories.json) to allure-results\.
REM
REM Usage: call scripts\windows\_run_playwright_tests.bat <label> ["filter"]
REM Env:   BROWSER=chromium|chrome|msedge|firefox|webkit, HEADLESS=true|false
REM ============================================================================

setlocal EnableDelayedExpansion
cd /d "%~dp0..\.."

set "LABEL=%~1"
set "FILTER=%~2"

set "LOCAL_DOTNET=tools\dotnet\dotnet.exe"
if exist "%LOCAL_DOTNET%" (set "DOTNET_CMD=%LOCAL_DOTNET%") else (set "DOTNET_CMD=dotnet")

set "PROJECT=Tests\Tests.csproj"
set "RESULTS_DIR=Tests\bin\Debug\net10.0\allure-results"

echo Removing previous Allure results...
if exist "%RESULTS_DIR%" rmdir /S /Q "%RESULTS_DIR%"
if exist allure-results rmdir /S /Q allure-results

echo Building %PROJECT%...
%DOTNET_CMD% build "%PROJECT%"
set BUILD_EXIT=%ERRORLEVEL%

echo Running %LABEL% tests...
if "%FILTER%"=="" (
    %DOTNET_CMD% test "%PROJECT%" --no-build --logger "trx;LogFileName=%LABEL%.trx"
) else (
    %DOTNET_CMD% test "%PROJECT%" --no-build --filter "%FILTER%" --logger "trx;LogFileName=%LABEL%.trx"
)
set TEST_EXIT=%ERRORLEVEL%

for /f "tokens=1" %%a in ("%date:/=-%") do set TODAY=%%a
for /f "tokens=1" %%a in ("%time: =0%") do set NOW=%%a
set TS=!TODAY!_!NOW!
set TS=!TS::=-!
if not exist "%RESULTS_DIR%" mkdir "%RESULTS_DIR%"
if "%BROWSER%"=="" (set "BROWSER_NAME=testsettings.json") else (set "BROWSER_NAME=%BROWSER%")
(
    echo run.timestamp=!TS!
    echo browser=!BROWSER_NAME!
) > "%RESULTS_DIR%\environment.properties"

mkdir allure-results
xcopy /E /I /Y "%RESULTS_DIR%\*" allure-results\ >nul
set COPY_EXIT=%ERRORLEVEL%
copy /Y allure-categories.json allure-results\categories.json >nul

echo(
echo ===================== SUMMARY =====================
echo Build  exit code : %BUILD_EXIT%
echo Test   exit code : %TEST_EXIT%
echo Copy   exit code : %COPY_EXIT%
echo Run Timestamp    : !TS!
echo Results          : allure-results  (allure serve allure-results)
echo ===================================================
endlocal & exit /b %TEST_EXIT%
