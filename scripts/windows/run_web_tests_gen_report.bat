@echo off
REM Run the Playwright web UI tests (SauceDemo + TodoMVC) and collect Allure results.
REM Pick the browser with: set BROWSER=chromium^|chrome^|msedge^|firefox^|webkit
call "%~dp0_run_playwright_tests.bat" WebTests "FullyQualifiedName!~Tests.Api."
exit /b %ERRORLEVEL%
