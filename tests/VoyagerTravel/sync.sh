#!/usr/bin/env bash
# Copies the Voyager Travel model and seeder from the generator, the single source for them:
#
#   tests/VoyagerTravel/sync.sh [generator]
#
# The generator defaults to ../voyager-travel-generator next to this repository. The files are copied as-is,
# except that the seed is anchored on a fixed day instead of the day it runs, so tests see the same dates.
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
generator="$(realpath "${1:-$here/../../../voyager-travel-generator}")"
source="$generator/overlay/src/VoyagerTravel"

if [[ ! -d "$source" ]]; then
  echo "No Voyager Travel app at $source." >&2
  exit 1
fi

rm -rf "$here/Models" "$here/Data"
mkdir -p "$here/Data"
cp -R "$source/Models" "$here/Models"
cp "$source/Data/ApplicationDbContext.cs" "$source/Data/ApplicationUser.cs" "$here/Data/"
cp -R "$source/Data/Seeding" "$here/Data/Seeding"

seeder="$here/Data/Seeding/DatabaseSeeder.cs"
anchor='new SeedData(DateOnly.FromDateTime(DateTime.UtcNow))'
if ! grep -qF "$anchor" "$seeder"; then
  echo "The seeder no longer contains '$anchor'; update the anchor in $0." >&2
  exit 1
fi
perl -i -pe 's/\Qnew SeedData(DateOnly.FromDateTime(DateTime.UtcNow))\E/new SeedData(new DateOnly(2026, 10, 18))/' "$seeder"

echo "Copied from $generator ($(git -C "$generator" rev-parse --short HEAD))"
