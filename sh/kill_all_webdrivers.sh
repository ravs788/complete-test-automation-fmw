#!/usr/bin/env bash

# Kill orphaned WebDriver processes if present.
# This script is intentionally best-effort: missing processes are not failures.

set +e

kill_by_name() {
  local process_name="$1"
  if pgrep -x "$process_name" >/dev/null 2>&1; then
    pkill -x "$process_name"
  fi
}

kill_by_name "chromedriver"
kill_by_name "geckodriver"
kill_by_name "msedgedriver"
kill_by_name "edgedriver"

echo "All browser driver processes have been terminated (if present)."
