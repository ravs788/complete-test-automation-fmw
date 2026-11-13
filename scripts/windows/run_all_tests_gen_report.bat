@echo off
REM Run every Playwright UI + API test and collect Allure results in allure-results\.
call "%~dp0_run_playwright_tests.bat" AllTests ""
exit /b %ERRORLEVEL%
