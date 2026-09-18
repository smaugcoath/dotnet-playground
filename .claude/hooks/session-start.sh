#!/bin/bash
# Installs the .NET SDK in Claude Code cloud sessions, where it is not preinstalled.
# Local sessions exit immediately; nothing here runs on a developer machine.
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

if command -v dotnet >/dev/null 2>&1 && dotnet --list-sdks | grep -q '^10\.'; then
  exit 0
fi

export DEBIAN_FRONTEND=noninteractive
apt-get update -qq
apt-get install -y -qq dotnet-sdk-10.0
dotnet --list-sdks
