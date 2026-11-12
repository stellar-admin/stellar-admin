#!/usr/bin/env bash
# Prepares a run root: StellarAdmin packed into a local feed, and the browser tool the grader uses.
# Usage: setup.sh <run-root>    (outside this repository)
set -euo pipefail
source "$(dirname "$0")/lib.sh"

root="${1:?usage: setup.sh <run-root>}"
mkdir -p "$root"
root="$(cd "$root" && pwd)"
check_root "$root"

rm -rf "$root/feed"
"$evals/pack-feed.sh" "$root/feed"

mkdir -p "$root/tools"
( cd "$root/tools" && [ -f package.json ] || npm init -y > /dev/null )
npm install --prefix "$root/tools" --no-audit --no-fund --silent "chrome-devtools-mcp@$mcp_version"

echo "Set up $root"
