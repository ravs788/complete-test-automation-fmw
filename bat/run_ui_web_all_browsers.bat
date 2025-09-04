@echo off
setlocal

REM Clean up old allure results from prior multi-browser run
if exist allure-results-chrome rmdir /s /q allure-results-chrome
if exist allure-results-firefox rmdir /s /q allure-results-firefox
if exist allure-results-edge rmdir /s /q allure-results-edge
if exist allure-results rmdir /s /q allure-results
if exist allure-report rmdir /s /q allure-report

REM Run tests on Chrome
echo Running UI.Web tests on Chrome...
set BROWSER=chrome
dotnet test UI/Web/UI.Web.Tests.csproj --logger "trx;LogFileName=WebTests_Chrome.trx" --results-directory allure-results-chrome
if errorlevel 1 echo Chrome run failed

REM Run tests on Firefox
echo Running UI.Web tests on Firefox...
set BROWSER=firefox
dotnet test UI/Web/UI.Web.Tests.csproj --logger "trx;LogFileName=WebTests_Firefox.trx" --results-directory allure-results-firefox
if errorlevel 1 echo Firefox run failed

REM Run tests on Edge
echo Running UI.Web tests on Edge...
set BROWSER=edge
dotnet test UI/Web/UI.Web.Tests.csproj --logger "trx;LogFileName=WebTests_Edge.trx" --results-directory allure-results-edge
if errorlevel 1 echo Edge run failed

REM Merge all Allure results into one folder for final report
mkdir allure-results
xcopy /Y /Q /S allure-results-chrome\* allure-results\
xcopy /Y /Q /S allure-results-firefox\* allure-results\
xcopy /Y /Q /S allure-results-edge\* allure-results\

REM Generate and (optionally) serve the Allure report
allure generate allure-results --clean -o allure-report
allure serve allure-results

echo Batch script complete. All UI.Web tests have been run on Chrome, Firefox, and Edge, and results merged into a single Allure report.

endlocal
