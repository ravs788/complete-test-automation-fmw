@echo off

setlocal

REM Clean up any previous common allure-results directory
if exist allure-results (
    rmdir /s /q allure-results
)
mkdir allure-results

REM Clean and build both test projects
dotnet clean API/API.Tests.csproj
dotnet clean UI/Web/UI.Web.Tests.csproj

dotnet build API/API.Tests.csproj --no-restore
dotnet build UI/Web/UI.Web.Tests.csproj --no-restore

REM Run both sets of tests, outputting allure results to a shared directory
dotnet test API/API.Tests.csproj --no-build --logger "trx;LogFileName=APITests.trx" --results-directory allure-results
dotnet test UI/Web/UI.Web.Tests.csproj --no-build --logger "trx;LogFileName=WebTests.trx" --results-directory allure-results

REM Generate static Allure report from merged results
allure generate allure-results --clean -o allure-report

REM (Optional) Serve the Allure report; comment the line below if you do not want to launch a web server
allure serve allure-results

echo Batch script complete. All tests across API and UI.Web have been run, and a common Allure report generated.
endlocal
