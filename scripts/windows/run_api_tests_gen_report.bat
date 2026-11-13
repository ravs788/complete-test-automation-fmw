@echo off
REM ============================================================================
REM Script: run_api_tests_gen_report.bat
REM Purpose:
REM   1. Clean the API test project / solution
REM   2. Execute the API tests (Allure JSON files are produced automatically
REM      inside the bin folder by the Allure adapter)
REM   3. Copy the generated Allure results from
REM        API\bin\Debug\net10.0\allure-results\
REM      to a top-level folder called
REM        allure-results
REM ============================================================================

setlocal EnableDelayedExpansion

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
REM 1. Clean API solution / project
REM --------------------------------------------------------------------------
echo Cleaning API solution / project...
%DOTNET_CMD% clean API/API.Tests.csproj
set CLEAN_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM Additional clean-up: remove previous Allure result folders
REM --------------------------------------------------------------------------
if exist API\bin\Debug\net10.0\allure-results (
    echo Removing previous API allure-results folder...
    rmdir /S /Q API\bin\Debug\net10.0\allure-results
)
if exist allure-results (
    echo Removing previous root allure-results folder...
    rmdir /S /Q allure-results
)

REM --------------------------------------------------------------------------
REM 2. Build API test project
REM --------------------------------------------------------------------------
echo Building API test project...
%DOTNET_CMD% build API/API.Tests.csproj --no-restore
set BUILD_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 3. Run API tests (this generates the Allure result *.json files)
REM    NOTE:
REM      • --no-build  → skip build because we already cleaned
REM      • --logger    → keep trx for Azure DevOps / any CI reporting
REM      • /p:AllureResultsDirectory → tell Allure adapter where to drop results
REM --------------------------------------------------------------------------
echo Running API tests...
%DOTNET_CMD% test API/API.Tests.csproj --no-build --logger "trx;LogFileName=APITests.trx" /p:AllureResultsDirectory=API\bin\Debug\net10.0\allure-results
set TEST_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 4. Add timestamp to Allure results
REM --------------------------------------------------------------------------
echo Adding timestamp to Allure results...
for /f "tokens=1" %%a in ("%date:/=-%") do set TODAY=%%a
for /f "tokens=1" %%a in ("%time: =0%") do set NOW=%%a
set TS=!TODAY!_!NOW!
set TS=!TS::=-!
echo run.timestamp=!TS!> API\bin\Debug\net10.0\allure-results\environment.properties
set TIMESTAMP=!TS!

REM --------------------------------------------------------------------------
REM 5. Copy Allure results to top-level allure-results directory
REM --------------------------------------------------------------------------
echo Preparing target allure-results directory...
if exist allure-results (
    rmdir /S /Q allure-results
)
mkdir allure-results

echo Copying Allure results...
xcopy /E /I /Y API\bin\Debug\net10.0\allure-results\* allure-results\
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
