# Cross-repo release pipeline

## Code audit — 2026-09-18

Current status: **superseded**. The workspace-hosted multi-repo pipeline is superseded by `.github/workflows/release.yml` and `build/README.md` in this checkout. Current publication is restricted to Core/TagHelpers.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **CLOSED 2026-08-20.** The pipeline itself is done and verified end-to-end
(Phases 0-3 + pro metadata; `0.0.1-preview.2` published to GitHub Packages, three repos
tagged, draft prerelease on OSS, package consumed successfully). Remaining work split
into [oss-release.md](oss-release.md) (also CLOSED — nuget.org go-live, was Phase 4) and
[pro-release.md](pro-release.md) (FUTURE — licensing etc., was Phase 5). This file stays
as the record of the pipeline's decisions and history.

Three of those decisions were **superseded by the nuget.org go-live on 2026-08-20** (see
[oss-release.md](oss-release.md) §4) and are kept below only as history: D7 (GitHub
Packages was staging; the push was dropped rather than kept as a mirror once nuget.org
went live), D10 (releases are no longer drafts — the release step runs after the push, so
the version is installable when the release appears), and open question 3 (`--generate-notes`
dropped; release bodies are deliberately empty). The `nuget-org` environment named in D8
exists but has **no** required reviewer: protection rules need a public repo or a paid
plan, and a lockstep version-bump guard replaced that gate.

Goal: a single release pipeline that versions and publishes every StellarAdmin package
in lockstep. v1 releases only `StellarAdmin.TagHelpers` (OSS); pro packages come later
but the pipeline is shaped for them from day one. The OSS `ci.yml` keeps its push/PR
build and loses its release half.

**Nothing is pushed to nuget.org until Phase 4.** Until then the pipeline must still be
testable end to end: packages are validated and smoke-tested from a local folder feed in
the build job, and "publishing" targets the org's GitHub Packages registry — the same
`dotnet nuget push` with a different `--source`, so going live is a source/auth swap.

Repos (all under the `stellar-admin` GitHub org): `workspace` (private umbrella, this
repo), `stellar-admin` (public OSS), `stellar-admin-pro` (private).

## Why the workspace repo hosts the release

- `StellarAdmin.Pro.csproj` / `StellarAdmin.Pro.Identity.csproj` reference the OSS
  project as `..\..\..\stellar-admin\src\...` — a sibling-directory `ProjectReference`.
  Any build of pro needs both repos checked out side by side under those exact names.
- The workspace `.gitignore` already ignores `/stellar-admin/` and `/stellar-admin-pro/`,
  so CI checkouts into those paths reproduce the local layout and `StellarAdmin.slnx`.
- Packing OSS and pro with the same `-p:Version` turns the `ProjectReference` into a
  `PackageReference` on that same version automatically — lockstep versioning for free.

## Decisions (proposed; confirm before Phase 1)

| # | Decision | Choice |
|---|---|---|
| D1 | Source acquisition | Multiple `actions/checkout` steps with `repository:` + `path:` (not submodules — keeps the "independent repos" convention). |
| D2 | Trigger | `workflow_dispatch` on `workspace` with inputs `version` (semver, required), `oss_ref` / `pro_ref` (default `master`), `publish` (bool, default false = dry run). Not a tag push: the pipeline knows the SHAs it built and creates the tags itself. |
| D3 | Version source | The `version` input, passed as `-p:Version` / `-p:PackageVersion` to every pack. No MinVer/NBGV (single-repo tools). |
| D4 | Lockstep | Every release bumps every package, changed or not (ASP.NET Core / Avalonia style). |
| D5 | Cross-repo token | GitHub App installed on the org (`actions/create-github-app-token@v2`), `contents: read+write` on the three repos. Fallback: fine-grained PAT as org secret (expires, tied to Jerrie's account). |
| D6 | Where users see releases | GitHub Release + `vX.Y.Z` tag created on the **OSS** repo at the built SHA, packages attached. Pro repo gets the same tag at its SHA (no release object yet). Workspace gets a tag too as the release record. |
| D7 | Publish target | v1 = GitHub Packages (`https://nuget.pkg.github.com/stellar-admin/index.json`) using `github.token` with `packages: write`; private, deletable, free for the org. nuget.org (OIDC via `NuGet/login@v1` + trusted publisher for `stellar-admin/workspace` / `release.yml`) is deferred to Phase 4. |
| D9 | Package verification | Every build runs `dotnet validate package local` (Meziantou.Framework.NuGetPackageValidation.Tool) over `artifacts/*.nupkg`, then a smoke test that consumes the packed OSS package from `artifacts/` as a folder feed in a throwaway webapp and asserts the page and the `_content` bundle are served. Same script runs locally. |
| D10 | Releases while off nuget.org | The OSS GitHub Release is created as a **draft** (nobody can install the version yet); un-drafted in Phase 4. Tags are created normally. |
| D8 | Job split | `build` job (checkout, build, test, pack, upload artifact) → `publish-oss` job (needs `build`, `environment: nuget-org` with required reviewer). Pro publish is a later sibling job to a private feed. |

## Phase 0 — One-time setup (manual, outside git)

1. Create the GitHub App (org-level, permissions: Contents read/write, Metadata read),
   install on `workspace`, `stellar-admin`, `stellar-admin-pro`; store
   `RELEASE_APP_ID` and `RELEASE_APP_PRIVATE_KEY` as org secrets (names used by
   `release.yml`). Needed for Phase 1's first CI run already: the OSS repo is still
   **private**, so even its checkout needs the token.
2. Create GitHub environment `packages` on `workspace` (required reviewer: Jerrie) — the
   manual gate in front of any push, even to the staging registry.
3. Locally: a PAT with `read:packages` in `~/.nuget/NuGet.Config` (or a `nuget.config`
   in a scratch app) so Jerrie can install from the GitHub Packages registry to verify.

(nuget.org trusted publisher + `nuget-org` environment moved to Phase 4.)

Checkpoint: nothing to review in code; just confirm secrets exist.

## Phase 1 — `release.yml` dry run (workspace repo) — DONE

Files: `.github/workflows/release.yml`, `build/smoke-test.sh`,
`.config/dotnet-tools.json`, `.gitignore` (`/artifacts/`). Verified locally 2026-08-19:
build + pack `--no-build` of all three packages at `0.0.1-smoke.3`, validator green,
smoke test green (tag helpers render, `_content` css/js served). Not yet run in CI (needs
Phase 0 secrets).

Findings that shaped it:
- Version and `ContinuousIntegrationBuild=true` are passed to `dotnet build` and pack
  runs `--no-build`; otherwise the assembly's informational version stays 1.0.0 while
  the nupkg says `$VERSION`, and the validator flags non-deterministic paths (rule 112).
- Validator rule ids 52 (project/repo URL reachable) and 119 (SourceLink sources
  reachable) are excluded until the OSS repo is public — remove the exclusions then.
- `AllowMissingPrunePackageData=true` is passed defensively (NETSDK1226 locally).
- The smoke test must run the app with `--no-launch-profile`; the template's
  launchSettings otherwise overrides the URL.

Add `.github/workflows/release.yml`:

- `on: workflow_dispatch` with the D2 inputs.
- `build` job:
  - `actions/checkout` (self); checkout OSS → `path: stellar-admin` at `oss_ref`
    (public, `github.token` is enough); checkout pro → `path: stellar-admin-pro` at
    `pro_ref` using the App token.
  - Record `OSS_SHA` / `PRO_SHA` as job outputs (`git -C stellar-admin rev-parse HEAD`).
  - `setup-node@v4` (20.x); `setup-dotnet@v5` with
    `global-json-file: stellar-admin/global.json` (it's no longer at the checkout root).
  - `dotnet build StellarAdmin.slnx -c Release` (add `-p:AllowMissingPrunePackageData=true`
    only if CI hits NETSDK1226 like the local machine does).
  - `dotnet pack` for `stellar-admin/src/StellarAdmin.TagHelpers` **and** the two pro
    projects into `./artifacts` with `-p:Version`, `-p:PackageVersion`, symbols/snupkg
    (same flags as today's `ci.yml`). Packing pro now is a free smoke test that the
    cross-package dependency version resolves; nothing pro is pushed.
  - Validate: `dotnet tool run`/`dotnet validate package local artifacts/*.nupkg`
    (Meziantou.Framework.NuGetPackageValidation.Tool, pinned in a workspace
    `.config/dotnet-tools.json`). Fails the job on missing README/icon/license, symbols,
    non-deterministic build, etc. Pro packages will fail metadata checks until Phase 5 —
    validate only the OSS package until then, or exclude the known rules.
  - Smoke test (`build/smoke-test.sh <version> <artifacts-dir>` in the workspace, so it
    also runs locally against a local `dotnet pack`): `dotnet new webapp` in a temp dir,
    `nuget.config` with `artifacts/` as a package source (plus nuget.org for the rest),
    `dotnet add package StellarAdmin.TagHelpers --version <version>`, add
    `AddStellarAdmin()` + a couple of tag helpers to `Index.cshtml`, `dotnet run` in the
    background, then `curl` the page and the `_content/StellarAdmin.TagHelpers/...`
    bundle and assert 200 + expected markup. This is the check that the RCL's static web
    assets and tag helpers actually work from the *package*, not the project reference.
  - Upload `artifacts/` as a workflow artifact.
- No publish job yet.

Verify: run with `publish=false`, inspect the artifact — three `.nupkg`, the
`StellarAdmin.Pro.nupkg` nuspec depends on `StellarAdmin.TagHelpers` `>= <version>`,
validate and smoke steps green. Also run `smoke-test.sh` locally once.

Checkpoint: review the workflow file, the smoke script + artifact.

## Phase 2 — Publish to GitHub Packages + tag OSS — DONE (2026-08-19, `0.0.1-preview.2`)

`publish-oss` job added to `release.yml`. Deviations from the sketch below: tags are
created explicitly via `gh api` *before* the release (a draft release does not create its
tag until published); the push uses `--no-symbols` (GitHub Packages rejects `.snupkg`;
symbols ship with nuget.org in Phase 4). Not yet run.


- Add `publish-oss` job: `needs: build`, `if: inputs.publish`, `environment: packages`,
  `permissions: packages: write, contents: read`. Download artifact, push
  `StellarAdmin.TagHelpers.*.nupkg` (and `.snupkg`; GitHub Packages accepts but ignores
  symbols) to `https://nuget.pkg.github.com/stellar-admin/index.json` with
  `--api-key ${{ github.token }} --skip-duplicate`. Filter to the OSS package by name; do
  not glob everything. The `--source` is the only thing Phase 4 changes.
- Then, with the App token: `gh release create -R stellar-admin/stellar-admin v<version>
  --target <OSS_SHA> --generate-notes --draft [--prerelease if version has a hyphen]`
  and upload the OSS packages as assets. Tag `stellar-admin-pro` at `PRO_SHA` and
  `workspace` at `github.sha` with the same `v<version>` (plain tags via `gh api` /
  `git push origin <sha>:refs/tags/v<version>`).
- Rerun safety: `--skip-duplicate` on push; `gh release create` must tolerate an existing
  release/tag (check first, `--clobber` on upload). Failed/abandoned versions can be
  deleted from GitHub Packages (unlike nuget.org).

Verify end to end with a `-preview` version: run with `publish=true`, then locally
install `StellarAdmin.TagHelpers <version>` from the GitHub Packages source into a scratch
app (or rerun `smoke-test.sh` with the registry as source) and confirm the draft release
+ three tags exist. Verified 2026-08-19: consumed `0.0.1-preview.2` from GitHub Packages.

Gotcha: `dotnet add package --source` takes a URL or folder path, **not** a NuGet.Config
source name — `--source github` resolves to `./github` (NU1301). Either omit `--source`
(restore uses every configured source, with its stored credentials) or pass the full URL
`https://nuget.pkg.github.com/stellar-admin/index.json`.

Checkpoint: review before Phase 4.

## Phase 3 — Strip release from OSS `ci.yml` — DONE (OSS commit 60e7b15)


Do this **before or together with** Phase 2 going live: releases created with an App/PAT
token *do* trigger `release: published` workflows (unlike `GITHUB_TOKEN`), so the old
job would double-publish.

- Remove the `release` trigger, `IS_RELEASE`/`TAG_VERSION` env, and the pack/upload/
  login/push steps; drop `packages: write` / `id-token: write` permissions; rename the
  workflow to "Build and Test". Keep push/PR on `master`.
- Optionally remove the old trusted publisher policy for `stellar-admin/stellar-admin`
  on nuget.org once the workspace one has succeeded.

Checkpoint: one small diff in the OSS repo.

## Phase 4 — Go live on nuget.org

Moved to [oss-release.md](oss-release.md) §4.

## Phase 5 — Pro readiness

Metadata work DONE 2026-08-19/20 (recorded in [pro-release.md](pro-release.md), which
also carries the remaining tasks): `stellar-admin-pro/src/Directory.Build.props` (a
deliberate copy of the OSS shape, not a cross-repo import: each repo must build
standalone), `LICENSE.txt` (per-application, perpetual fallback, localhost-only without
a key), `readme.md`, icon, XML docs, per-csproj `Description`/`PackageTags`;
`release.yml` validates all three packages (52/119 stay excluded for pro: the repo is
private) and pushes the `PUBLISH_PACKAGES` list (OSS only for now). Nice-to-haves
(version-bump guard, Dependabot) moved to [oss-release.md](oss-release.md).

## Open questions — resolved 2026-08-19

1. Prerelease convention: any SemVer prerelease suffix (`-alpha.N`, `-beta.N`,
   `-preview.N`, `-rc.N`); a hyphen is what marks a prerelease in the workflow. Only
   caveat: NuGet sorts labels alphabetically (`alpha < beta < preview < rc`), so don't
   publish a "later" build under an alphabetically earlier label for the same version.
2. Record of built commits: tags on all three repos only (plus the run's step summary);
   no committed SHA manifest.
3. Release notes: `--generate-notes` for now. It lists merged PRs, and the OSS repo is
   committed to directly, so the body is just the "Full Changelog" link; revisit (PRs or a
   `--notes-file`) before the first public release.
4. Test versions (`0.0.1-preview.*`): packages stay on GitHub Packages; Jerrie deletes GitHub
   releases himself if he wants to.
5. Pro/workspace repos get **tags only**, no GitHub Release (confirmed 2026-08-20). For
   grabbing pro nupkgs after a build, the run's `packages-<version>` artifact suffices
   (30-day retention is fine for immediate testing).
