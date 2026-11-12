#!/usr/bin/env bash
# Grades one run: builds and starts its app in a sandbox, runs the deterministic checks, then an Opus grader
# works through the checklist and rubric in a headless browser. Results go to <run>/grade/.
# Usage: grade.sh <run-root> <run-id> [port]
set -euo pipefail
source "$(dirname "$0")/lib.sh"

root="$(cd "${1:?usage: grade.sh <run-root> <run-id> [port]}" && pwd)"
id="${2:?usage: grade.sh <run-root> <run-id> [port]}"
port="${3:-5310}"
dir="$root/$id"
ws="$dir/workspace"
app="$ws/app"
out="$dir/grade"
url="http://127.0.0.1:$port"
sandbox_credentials="$(credentials)"
grader_model="${GRADER_MODEL:-opus}"

rm -rf "$out"
mkdir -p "$out"
rm -f "$app"/voyager.db*

if ! sandbox "$ws" "$app" dotnet build -v quiet > "$out/build.log" 2>&1; then
  echo '{ "build": false, "checks": [] }' > "$out/checks.json"
  echo "build failed: $out/build.log"
  exit 0
fi

sandbox "$ws" "$app" env ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --no-launch-profile --urls "$url" > "$out/app.log" 2>&1 &
app_pid=$!
trap 'kill $app_pid 2>/dev/null || true' EXIT
for _ in $(seq 1 120); do curl -s -o /dev/null "$url/" && break; sleep 0.5; done

node "$evals/voyager/checks/checks.mjs" "$url" > "$out/checks.json"

sed "s|{{BASE_URL}}|$url|g" "$evals/build-admin/checklist.md" > "$out/checklist.md"
cp "$evals/build-admin/rubric.md" "$out/rubric.md"
cat > "$out/mcp.json" <<JSON
{ "mcpServers": { "browser": {
  "command": "$(tool_dir node)/node",
  "args": ["$root/tools/node_modules/chrome-devtools-mcp/out/Default/gen/src/bin/chrome-devtools-mcp.js",
    "--headless", "--isolated", "--executablePath", "/usr/bin/chromium", "--viewport", "1440x900",
    "--chromeArg=--no-sandbox", "--no-usage-statistics"]
} } }
JSON

prompt="You are grading a web app's admin area. Work through checklist.md in this directory with the browser tools, then score rubric.md, taking the screenshots it lists and saving them in $out under the names it gives. Judge only what you see in the browser. When done, write $out/grade.json with this shape: {\"checks\": [{\"id\": \"L1\", \"result\": \"pass|partial|fail\", \"note\": \"...\"}], \"rubric\": [{\"id\": \"R1\", \"score\": 1, \"note\": \"...\"}], \"summary\": \"...\"}, with every check and rubric item."
sandbox_ro=("$root/tools")
set +e
sandbox "$out" "$out" timeout 60m claude -p "$prompt" \
  --model "$grader_model" --output-format json --dangerously-skip-permissions \
  --strict-mcp-config --mcp-config "$out/mcp.json" > "$out/grader.json" 2> "$out/grader.err"
set -e
[ -s "$out/grade.json" ] && echo "graded: $out/grade.json" || echo "the grader wrote no grade.json: $out/grader.err"
