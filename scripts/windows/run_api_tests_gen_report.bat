@echo off
REM Run the Restful Booker API tests (Playwright APIRequestContext) and collect Allure results.
call "%~dp0_run_playwright_tests.bat" ApiTests "FullyQualifiedName~Tests.Api."
exit /b %ERRORLEVEL%
