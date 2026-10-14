#!/usr/bin/env bash
# Creates a run directory with local StellarAdmin packages and one copy of the Voyager app per task.
# Usage: prepare.sh <run-dir>    (the run dir must be outside this repository)
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../../.." && pwd)"
run="${1:?usage: prepare.sh <run-dir>}"
version="0.0.1-eval"

mkdir -p "$run"
run="$(cd "$run" && pwd)"
case "$run/" in "$repo/"*) echo "run dir must be outside the repository" >&2; exit 1 ;; esac

export DOTNET_SYSTEM_NET_DISABLEIPV6=1

for project in StellarAdmin.UI StellarAdmin StellarAdmin.EntityFrameworkCore; do
  dotnet pack "$repo/src/$project" -c Release -o "$run/feed" -p:Version=$version -p:AllowMissingPrunePackageData=true -v quiet
done

cp "$repo/global.json" "$run/"
cat > "$run/nuget.config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local" value="feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
XML

# Task 1 starts without StellarAdmin; tasks 2-6 start with it installed
for task in 1 2 3 4 5 6; do
  app="$run/task-$task"
  rm -rf "$app"
  dotnet new webapp -n Voyager -o "$app" --no-https > /dev/null
  cp -r "$here/fixtures/fresh/." "$app/"
  if [ "$task" != 1 ]; then
    cp -r "$here/fixtures/installed/." "$app/"
  fi
  dotnet build "$app" -v quiet > /dev/null
done

echo "Prepared $run"
