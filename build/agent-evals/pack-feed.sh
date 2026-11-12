#!/usr/bin/env bash
# Packs StellarAdmin from this checkout into a local NuGet feed, as version 0.0.1-eval.
# Usage: pack-feed.sh <feed-dir>
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
feed="${1:?usage: pack-feed.sh <feed-dir>}"
version="0.0.1-eval"

export DOTNET_SYSTEM_NET_DISABLEIPV6=1

mkdir -p "$feed"
for project in StellarAdmin.UI StellarAdmin StellarAdmin.EntityFrameworkCore; do
  dotnet pack "$repo/src/$project" -c Release -o "$feed" -p:Version=$version -p:AllowMissingPrunePackageData=true -v quiet
done
