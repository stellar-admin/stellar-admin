#!/usr/bin/env bash
# Smoke-tests the three active packed StellarAdmin packages as a consumer:
# scaffolds a throwaway Razor Pages app, installs the package from a folder feed, renders
# a page with tag helpers, and asserts the page and the RCL static web assets are served.
#
# Usage: build/smoke-test.sh <version> <artifacts-dir> [port]
#
# Runs both locally (against `dotnet pack -o <artifacts-dir>`) and in the release
# workflow. Requires the .NET SDK, curl and network access to nuget.org for the
# framework packages the scaffolded app pulls in.
set -euo pipefail

VERSION="${1:?usage: smoke-test.sh <version> <artifacts-dir> [port]}"
ARTIFACTS="$(cd "${2:?usage: smoke-test.sh <version> <artifacts-dir> [port]}" && pwd)"
PORT="${3:-5299}"
PACKAGES=(StellarAdmin.Core StellarAdmin.TagHelpers StellarAdmin.Dashboard)

WORK="$(mktemp -d)"
APP="$WORK/SmokeApp"
# Resolve the SDK from the product pin even inside the throwaway consumer directory.
cp "$(dirname "${BASH_SOURCE[0]}")/../global.json" "$WORK/global.json"
# Never satisfy the tested version from a previous run in the global package cache.
export NUGET_PACKAGES="$WORK/packages"
SERVER_PID=""

cleanup() {
  if [[ -n "$SERVER_PID" ]] && kill -0 "$SERVER_PID" 2>/dev/null; then
    kill "$SERVER_PID" 2>/dev/null || true
    wait "$SERVER_PID" 2>/dev/null || true
  fi
  rm -rf "$WORK"
}
trap cleanup EXIT

fail() { echo "SMOKE FAIL: $*" >&2; exit 1; }
step() { echo; echo "==> $*"; }

for PACKAGE in "${PACKAGES[@]}"; do
  [[ -f "$ARTIFACTS/$PACKAGE.$VERSION.nupkg" ]] \
    || fail "$ARTIFACTS/$PACKAGE.$VERSION.nupkg not found"
done

if curl -s -o /dev/null --connect-timeout 1 "http://127.0.0.1:$PORT/"; then
  fail "port $PORT is already in use"
fi

step "Scaffolding consumer app in $APP"
dotnet new webapp --name SmokeApp --output "$APP" --no-restore

# Package sources: the folder feed for the package under test, nuget.org for everything
# else. Isolated from the user's global NuGet.Config so only these two sources apply.
cat > "$APP/nuget.config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="artifacts" value="$ARTIFACTS" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="artifacts"><package pattern="StellarAdmin.*" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
XML

for PACKAGE in "${PACKAGES[@]}"; do
  [[ "$PACKAGE" == StellarAdmin.Core ]] && continue
  step "Adding $PACKAGE $VERSION from the folder feed"
  dotnet add "$APP" package "$PACKAGE" --version "$VERSION" --no-restore
done

# Wire the package up exactly as the readme's quick start describes.
cat > "$APP/Program.cs" <<'CS'
using StellarAdmin;
using StellarAdmin.Icons;
using StellarAdmin.TagHelpers;
using StellarAdmin.Dashboard;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddStellarAdmin()
    .ConfigureForms(forms => forms.SectionLayout = FormSectionLayout.Stacked)
    .AddTagHelpers();
builder.Services.AddStellarAdmin().AddDashboard();

var app = builder.Build();
if (app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StellarAdminFormsOptions>>()
    .Value.SectionLayout != FormSectionLayout.Stacked)
    throw new InvalidOperationException("Form configuration was not applied.");
var icons = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<IconOptions>>().Value;
if (typeof(IconOptions).Assembly.GetName().Name != "StellarAdmin.Core"
    || !icons.TryGetIcon("check", out var checkIcon)
    || checkIcon!.Shapes.Count == 0)
    throw new InvalidOperationException("Core icon definitions were not packaged correctly.");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
CS

cat >> "$APP/Pages/_ViewImports.cshtml" <<'RAZOR'
@using StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.TagHelpers
@addTagHelper *, StellarAdmin.Dashboard
RAZOR

cat > "$APP/Pages/Shared/_Layout.cshtml" <<'RAZOR'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Smoke</title>
    <link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.shadcn.nova.css" />
    <link rel="stylesheet" href="/_content/StellarAdmin.Dashboard/stellar-admin-dashboard.css" />
    <script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js"></script>
</head>
<body>
    @RenderBody()
</body>
</html>
RAZOR

cat > "$APP/Pages/Index.cshtml" <<'RAZOR'
@page
@model IndexModel
<sa-alert id="smoke-alert">
    <sa-alert-title>Smoke test alert</sa-alert-title>
    <sa-alert-description>Rendered from the packed StellarAdmin.TagHelpers.</sa-alert-description>
</sa-alert>
<sa-button id="smoke-button">Smoke button</sa-button>
<sa-data-grid items='@(new[] { new { Name = "Smoke row" } })'>
    <sa-data-grid-column title="Name" field="Name" />
</sa-data-grid>
RAZOR

step "Restoring and building"
dotnet restore "$APP"
dotnet build "$APP" --no-restore -c Release

# Confirm the restore really resolved the package under test at the requested version.
for PACKAGE in "${PACKAGES[@]}"; do
  grep -q "\"$PACKAGE/$VERSION\"" "$APP/obj/project.assets.json" \
    || fail "project.assets.json does not reference $PACKAGE/$VERSION"
done

step "Starting app on port $PORT"
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://127.0.0.1:$PORT" \
  dotnet run --project "$APP" --no-build --no-launch-profile -c Release \
    --urls "http://127.0.0.1:$PORT" >"$WORK/server.log" 2>&1 &
SERVER_PID=$!

for _ in $(seq 1 60); do
  if curl -fsS -o /dev/null "http://127.0.0.1:$PORT/" 2>/dev/null; then break; fi
  kill -0 "$SERVER_PID" 2>/dev/null || { cat "$WORK/server.log"; fail "app exited early"; }
  sleep 1
done
curl -fsS -o /dev/null "http://127.0.0.1:$PORT/" || { cat "$WORK/server.log"; fail "app did not start"; }

step "Asserting rendered page"
PAGE="$(curl -fsS "http://127.0.0.1:$PORT/")"
echo "$PAGE" | grep -q 'id="smoke-alert"' || fail "alert tag helper did not render"
echo "$PAGE" | grep -q 'role="alert"' || fail "alert markup missing role=\"alert\" (tag helper not applied?)"
echo "$PAGE" | grep -q '<sa-alert' && fail "raw <sa-alert> left in output (tag helpers not registered)"
echo "$PAGE" | grep -q 'id="smoke-button"' || fail "button tag helper did not render"
echo "$PAGE" | grep -q '<sa-button' && fail "raw <sa-button> left in output (tag helpers not registered)"

echo "$PAGE" | grep -q 'Smoke row' || fail "data grid did not render its row"
echo "$PAGE" | grep -q '<sa-data-grid' && fail "raw data grid tags left in output"

step "Asserting static web assets from the package"
assert_asset() {
  local path="$1" needle="$2"
  local file="$WORK/asset"
  curl -fsS -o "$file" "http://127.0.0.1:$PORT$path" || fail "$path not served"
  [[ -s "$file" ]] || fail "$path is empty"
  if ! grep -q -- "$needle" "$file"; then
    echo "--- response headers for $path:" >&2
    curl -sSI "http://127.0.0.1:$PORT$path" >&2 || true
    echo "--- first bytes:" >&2
    head -c 300 "$file" >&2; echo >&2
    fail "$path does not contain '$needle'"
  fi
  echo "ok $path ($(wc -c <"$file") bytes)"
}
assert_asset /_content/StellarAdmin.TagHelpers/stellar-admin.shadcn.nova.css "--color-"
assert_asset /_content/StellarAdmin.TagHelpers/stellar-admin.js "function"
assert_asset /_content/StellarAdmin.Dashboard/stellar-admin-dashboard.css "sa-resource-form"
assert_asset /_content/StellarAdmin.Dashboard/htmx.min.js "htmx"

echo
echo "SMOKE OK: all three packages at $VERSION restore; tag helpers and data grid render with static assets"
