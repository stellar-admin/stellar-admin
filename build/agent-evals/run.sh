#!/usr/bin/env bash
# One agent run: a fresh copy of the task's starting app for the variant, then \`claude -p\` on the task's prompt
# in a sandbox. With PREPARE_ONLY=1 it stops before the agent, to check the starting app.
# Usage: run.sh <run-root> <razor|sa-docs|sa-skills> [task]    The task is a directory here with a task.env
# (build-admin by default). Prints the run id; the variant and task are recorded in <run-root>/runs.tsv.
#   razor: Razor Pages, no StellarAdmin.
#   sa-docs: StellarAdmin wired in at /admin with no resources, its docs named in CLAUDE.md.
#   sa-skills: sa-docs plus the stellar-admin and stellar-admin-setup skills in the project.
set -euo pipefail
source "$(dirname "$0")/lib.sh"

root="$(cd "${1:?usage: run.sh <run-root> <razor|sa-docs|sa-skills>}" && pwd)"
variant="${2:?usage: run.sh <run-root> <razor|sa-docs|sa-skills>}"
task="${3:-build-admin}"
check_root "$root"
load_task "$task"
case "$variant" in razor|sa-docs|sa-skills) ;; *) echo "variant must be razor, sa-docs or sa-skills" >&2; exit 1 ;; esac
[ -d "$root/feed" ] || { echo "run setup.sh first" >&2; exit 1; }
if [ -z "${PREPARE_ONLY:-}" ]; then sandbox_credentials="$(credentials)"; fi
model="${AGENT_MODEL:-sonnet}"
limit="${AGENT_TIMEOUT:-90m}"

# Opaque ids, so neither the agent nor the grader learns the variant from a path
id="run-$(od -An -N3 -tx1 /dev/urandom | tr -d ' \n')"
dir="$root/$id"
ws="$dir/workspace"
app="$ws/app"
mkdir -p "$ws"
printf '%s\t%s\t%s\t%s\n' "$id" "$variant" "$(date -Iseconds)" "$task" >> "$root/runs.tsv"

cp "$repo/global.json" "$ws/"
sources='<add key="nuget.org" value="https://api.nuget.org/v3/index.json" />'
if [ "$variant" != razor ]; then
  cp -r "$root/feed" "$ws/feed"
  sources="<add key=\"local\" value=\"feed\" />
    $sources"
fi
# A run-local package folder: the global cache would keep serving an earlier run's 0.0.1-eval packages
cat > "$ws/nuget.config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="packages" />
  </config>
  <packageSources>
    <clear />
    $sources
  </packageSources>
</configuration>
XML

dotnet new webapp -n "$app_name" -o "$app" --no-https > /dev/null
cp -r "$evals/$fixture/base/." "$app/"
if [ "$fixture" = voyager-travel ]; then copy_voyager_travel_model "$app"; fi
if [ "$variant" != razor ]; then cp -r "$evals/$fixture/package/." "$app/"; fi
if [ "$variant" = sa-skills ]; then
  mkdir -p "$app/.claude/skills"
  cp -r "$repo/skills/stellar-admin" "$repo/skills/stellar-admin-setup" "$app/.claude/skills/"
fi
( cd "$app" && dotnet new gitignore > /dev/null && printf '\n# SQLite\n%s*\n' "$database" >> .gitignore )
sandbox "$ws" "$app" dotnet build -v quiet > "$dir/prepare-build.log" 2>&1 || { echo "the starting app doesn't build: $dir/prepare-build.log" >&2; exit 1; }
git -C "$app" init -q
git -C "$app" -c core.autocrlf=false add -A
git -C "$app" -c user.name=Voyager -c user.email=dev@voyager.test commit -qm "Voyager"
if [ -n "${PREPARE_ONLY:-}" ]; then echo "$id"; exit 0; fi

started="$(date +%s)"
set +e
sandbox "$ws" "$app" timeout "$limit" claude -p "$(cat "$evals/$task/prompt.md")" \
  --model "$model" --output-format stream-json --verbose --dangerously-skip-permissions \
  > "$dir/agent.jsonl" 2> "$dir/agent.err"
code=$?
set -e
ended="$(date +%s)"

git -C "$app" -c core.autocrlf=false add -A
git -C "$app" diff --cached --numstat > "$dir/numstat.txt"
skills="$(cd "$app" && find . -name SKILL.md -not -path './.git/*' | sort | tr '\n' ' ')"
cat > "$dir/meta.json" <<JSON
{ "id": "$id", "variant": "$variant", "task": "$task", "model": "$model", "exitCode": $code, "seconds": $((ended - started)), "skills": "$skills" }
JSON
echo "$id"
