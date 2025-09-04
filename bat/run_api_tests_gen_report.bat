@echo off

setlocal

REM Clean API test project
dotnet clean API/API.Tests.csproj
set CLEAN_EXIT=%ERRORLEVEL%

REM Build API test project
dotnet build API/API.Tests.csproj --no-restore
set BUILD_EXIT=%ERRORLEVEL%

REM Run tests and generate allure results
dotnet test API/API.Tests.csproj --no-build --logger "trx;LogFileName=TestResults.trx" --results-directory API/bin/Debug/net9.0/allure-results
set TEST_EXIT=%ERRORLEVEL%

REM Generate static Allure report
if exist API\bin\Debug\net9.0\allure-results (
    allure generate API\bin\Debug\net9.0\allure-results --clean -o API\bin\Debug\net9.0\allure-report
    set ALLURE_EXIT=%ERRORLEVEL%
) else (
    echo Allure results directory does not exist, skipping report generation.
    set ALLURE_EXIT=1
)

REM (Optional) Serve the Allure report; comment out if you do not want to launch a web server
allure serve API\bin\Debug\net9.0\allure-results

REM Print summary of each step
echo Clean result:  %CLEAN_EXIT%
echo Build result:  %BUILD_EXIT%
echo Test result:   %TEST_EXIT%
echo Allure generate result: %ALLURE_EXIT%
echo Report preview server (if started) is running. Press Ctrl+C to stop.

endlocal

echo Batch script complete.
