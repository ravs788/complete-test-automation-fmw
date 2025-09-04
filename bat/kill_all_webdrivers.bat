@echo off

REM Kill all Chrome driver instances
taskkill /F /IM chromedriver.exe /T

REM Kill all Gecko (Firefox) driver instances
taskkill /F /IM geckodriver.exe /T

REM Kill all Edge driver instances
taskkill /F /IM msedgedriver.exe /T

REM Kill legacy Edge driver instances if present
taskkill /F /IM edgedriver.exe /T

REM Ignore errors if process is not found

echo All browser driver processes have been terminated (if present).
