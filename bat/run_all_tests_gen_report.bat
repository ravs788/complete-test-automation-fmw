@echo off
REM ============================================================================
REM Script: run_all_tests_gen_report.bat
REM Purpose:
REM   • Run both API and UI/Web test suites and create a merged Allure report
REM   • Common workflow:
REM       0. Kill orphaned WebDriver processes
REM       1. Clean projects and old Allure result folders
REM       2. Build projects
REM       3. Execute tests (single AllureResultsDirectory each → no duplicates)
REM       4. Inject run.timestamp into each environment.properties
REM       5. Merge results to root-level allure-results
REM       6. Print summary of exit codes and timestamp
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
REM 0. Kill orphaned WebDriver processes
REM --------------------------------------------------------------------------
call bat\kill_all_webdrivers.bat

REM --------------------------------------------------------------------------
REM 1. Clean projects and previous Allure folders
REM --------------------------------------------------------------------------
echo Cleaning test projects...
%DOTNET_CMD% clean API/API.Tests.csproj
set CLEAN_API_EXIT=%ERRORLEVEL%
%DOTNET_CMD% clean UI/Web/UI.Web.Tests.csproj
set CLEAN_WEB_EXIT=%ERRORLEVEL%

echo Removing previous Allure result folders...
if exist API\bin\Debug\net9.0\allure-results (
    rmdir /S /Q API\bin\Debug\net9.0\allure-results
)
if exist UI\Web\bin\Debug\net9.0\allure-results (
    rmdir /S /Q UI\Web\bin\Debug\net9.0\allure-results
)
if exist allure-results (
    rmdir /S /Q allure-results
)

REM --------------------------------------------------------------------------
REM 2. Build projects
REM --------------------------------------------------------------------------
echo Building projects...
%DOTNET_CMD% build API/API.Tests.csproj --no-restore
set BUILD_API_EXIT=%ERRORLEVEL%
%DOTNET_CMD% build UI/Web/UI.Web.Tests.csproj --no-restore
set BUILD_WEB_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 3. Run tests
REM --------------------------------------------------------------------------
echo Running API tests...
%DOTNET_CMD% test API/API.Tests.csproj ^
  --no-build ^
  --logger "trx;LogFileName=APITests.trx" ^
  /p:AllureResultsDirectory=API\bin\Debug\net9.0\allure-results
set TEST_API_EXIT=%ERRORLEVEL%

echo Running UI.Web tests...
%DOTNET_CMD% test UI/Web/UI.Web.Tests.csproj ^
  --no-build ^
  --logger "trx;LogFileName=WebTests.trx" ^
  --test-adapter-path:. ^
  /p:AllureResultsDirectory=UI\Web\bin\Debug\net9.0\allure-results
set TEST_WEB_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM 4. Add timestamp to both result folders
REM --------------------------------------------------------------------------
echo Adding timestamp to Allure results...
for /f "tokens=1" %%a in ("%date:/=-%") do set TODAY=%%a
for /f "tokens=1" %%a in ("%time: =0%") do set NOW=%%a
set TS=!TODAY!_!NOW!
set TS=!TS::=-!
echo run.timestamp=!TS!> API\bin\Debug\net9.0\allure-results\environment.properties
echo run.timestamp=!TS!> UI\Web\bin\Debug\net9.0\allure-results\environment.properties
set TIMESTAMP=!TS!

REM --------------------------------------------------------------------------
REM 5. Merge results to root allure-results
REM --------------------------------------------------------------------------
echo Merging Allure result sets...
mkdir allure-results
xcopy /E /I /Y API\bin\Debug\net9.0\allure-results\* allure-results\ >nul
xcopy /E /I /Y UI\Web\bin\Debug\net9.0\allure-results\* allure-results\ >nul
set COPY_EXIT=%ERRORLEVEL%

REM --------------------------------------------------------------------------
REM Summary
REM --------------------------------------------------------------------------
echo(
echo ===================== SUMMARY =====================
echo Clean  API exit code : %CLEAN_API_EXIT%
echo Clean  WEB exit code : %CLEAN_WEB_EXIT%
echo Build  API exit code : %BUILD_API_EXIT%
echo Build  WEB exit code : %BUILD_WEB_EXIT%
echo Test   API exit code : %TEST_API_EXIT%
echo Test   WEB exit code : %TEST_WEB_EXIT%
echo Merge  step exit code: %COPY_EXIT%
echo Run Timestamp        : %TIMESTAMP%
echo ===================================================
echo(
endlocal
