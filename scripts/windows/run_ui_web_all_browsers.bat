@echo off
REM ============================================================================
REM Run the Playwright web UI tests once per browser and merge the Allure results
REM into allure-results\ (one environment.<browser>.properties per browser).
REM Browsers: %BROWSERS% (default "chromium firefox webkit"). Install Playwright's
REM browsers first: pwsh Tests\bin\Debug\net10.0\playwright.ps1 install
REM ============================================================================

setlocal EnableDelayedExpansion
cd /d "%~dp0..\.."

if "%BROWSERS%"=="" set "BROWSERS=chromium firefox webkit"
set "MERGED_DIR=%TEMP%\allure-merged-%RANDOM%"
mkdir "%MERGED_DIR%"
set OVERALL_EXIT=0

for %%b in (%BROWSERS%) do (
    echo ================================================================
    echo Running web UI tests on %%b...
    echo ================================================================
    set "BROWSER=%%b"
    call "%~dp0_run_playwright_tests.bat" WebTests_%%b "FullyQualifiedName!~Tests.Api."
    if errorlevel 1 (
        set OVERALL_EXIT=1
        echo %%b run failed
    )
    if exist allure-results\environment.properties move /Y allure-results\environment.properties allure-results\environment.%%b.properties >nul
    xcopy /E /I /Y allure-results\* "%MERGED_DIR%\" >nul
)

if exist allure-results rmdir /S /Q allure-results
xcopy /E /I /Y "%MERGED_DIR%\*" allure-results\ >nul
rmdir /S /Q "%MERGED_DIR%"
echo(
echo Merged results for: %BROWSERS% -^> allure-results  (allure serve allure-results)
endlocal & exit /b %OVERALL_EXIT%
