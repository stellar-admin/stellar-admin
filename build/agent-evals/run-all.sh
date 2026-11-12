#!/usr/bin/env bash
# Runs and grades <count> runs of each variant, interleaved (razor sa-docs sa-skills, razor ...), then prints the summary.
# Usage: run-all.sh <run-root> <count> [variants]    variants default to "razor sa-docs sa-skills"
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"

root="${1:?usage: run-all.sh <run-root> <count> [variants]}"
count="${2:?usage: run-all.sh <run-root> <count> [variants]}"
variants="${3:-razor sa-docs sa-skills}"

for _ in $(seq 1 "$count"); do
  for variant in $variants; do
    echo "== $variant: running"
    id="$("$here/run.sh" "$root" "$variant" | tail -1)"
    echo "== $variant: grading $id"
    "$here/grade.sh" "$root" "$id"
  done
done

node "$here/summarize.mjs" "$root" | tee "$root/summary.md"
