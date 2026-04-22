#!/usr/bin/env bash

set -euo pipefail

if [ "$#" -eq 0 ]; then
  exit 0
fi

dotnet format server/AlCopilot.slnx --no-restore --include "$@"
