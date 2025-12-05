#!/bin/bash

# Kill all Chrome driver instances
pkill -f chromedriver

# Kill all Gecko (Firefox) driver instances
pkill -f geckodriver

# Kill all Edge driver instances
pkill -f msedgedriver

# Kill legacy Edge driver instances if present
pkill -f edgedriver

# Ignore errors if process is not found

echo "All browser driver processes have been terminated (if present)."
