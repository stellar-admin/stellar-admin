#!/usr/bin/env bash
# Builds and runs a task app, signs in with the given roles ("-" for anonymous), fetches each path and prints its status.
# Usage: probe.sh <app-dir> <port> <roles|-> <path>...   Response bodies go to <app-dir>/../probe-out/<task>/<n>.html
set -u
app="$1"; port="$2"; roles="$3"; shift 3
out="$(dirname "$app")/probe-out/$(basename "$app")"
mkdir -p "$out"; rm -f "$out"/*
export DOTNET_SYSTEM_NET_DISABLEIPV6=1
( cd "$app" && rm -f voyager.db* && dotnet build -v quiet -p:AllowMissingPrunePackageData=true > "$out/build.log" 2>&1 ) || { echo "BUILD FAILED"; tail -20 "$out/build.log"; exit 1; }
( cd "$app" && exec dotnet run --no-build --urls "http://127.0.0.1:$port" > "$out/run.log" 2>&1 ) &
pid=$!
for i in $(seq 1 60); do curl -s -o /dev/null "http://127.0.0.1:$port/" && break; sleep 0.5; done
jar="$out/jar"
if [ "$roles" != "-" ]; then curl -s -c "$jar" -o /dev/null "http://127.0.0.1:$port/dev-login?name=sam&roles=$roles"; fi
n=0
for path in "$@"; do
  n=$((n+1))
  code=$(curl -s -b "$jar" -o "$out/$n.html" -w "%{http_code} -> %{redirect_url}" "http://127.0.0.1:$port$path")
  echo "$path  $code  ($out/$n.html)"
done
kill $pid 2>/dev/null; pkill -P $pid 2>/dev/null; sleep 1
fuser -k "$port/tcp" >/dev/null 2>&1
grep -iE "fail|exception" "$out/run.log" | head -5
