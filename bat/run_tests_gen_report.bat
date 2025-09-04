@echo off

setlocal

REM Kill orphaned driver processes first
call bat\kill_all_webdrivers.bat

REM Clean UI.Web test project
dotnet clean UI/Web/UI.Web.Tests.csproj
set CLEAN_EXIT=%ERRORLEVEL%

REM Build UI.Web test project
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore
set BUILD_EXIT=%ERRORLEVEL%

REM Run tests and generate allure results
dotnet test UI/Web/UI.Web.Tests.csproj --no-build --logger "trx;LogFileName=TestResults.trx" --results-directory UI/Web/allure-results
set TEST_EXIT=%ERRORLEVEL%

REM Generate static Allure report
if exist UI/Web/allure-results (
    allure generate UI/Web/allure-results --clean -o UI/Web/allure-report
    set ALLURE_EXIT=%ERRORLEVEL%
) else (
    echo Allure results directory does not exist, skipping report generation.
    set ALLURE_EXIT=1
)

REM (Optional) Serve the Allure report; comment the line below out if you do not want to launch a web server
allure serve UI/Web/allure-results

REM Print summary of each step
echo Clean result:  %CLEAN_EXIT%
echo Build result:  %BUILD_EXIT%
echo Test result:   %TEST_EXIT%
echo Allure generate result: %ALLURE_EXIT%
echo Report preview server (if started) is running. Press Ctrl+C to stop.

endlocal

echo Batch script complete.
