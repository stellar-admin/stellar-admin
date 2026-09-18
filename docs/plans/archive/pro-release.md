# Pro release (StellarAdmin.Pro / StellarAdmin.Pro.Identity)

## Code audit — 2026-09-18

Current status: **superseded**. The separate commercial Pro product/repository assumed here has been replaced by MIT-licensed `StellarAdmin.Dashboard`, `.Identity` and `.EntityFrameworkCore` in this checkout. The current release workflow builds/packs them but its publication allowlist remains Core and TagHelpers. Publishing Dashboard packages would need a new release decision; commercial licensing, a Pro package feed and cross-repo App-token CI are retired assumptions.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** parked. **Indexed:** 2026-09-05. Future work, not authorized by inclusion in this index.

Status: **FUTURE** (parked 2026-08-20) — split out of
[release-pipeline.md](release-pipeline.md). The shared pipeline already builds, packs
and validates the pro packages on every release run with lockstep versions, so the
mechanics are proven; what remains is everything commercial.

Decisions already made (see release-pipeline.md for history):

- Pro packages go to the **same feed** as the OSS package; access is governed by
  licensing, not a separate feed.
- Pro/workspace repos get **tags only**, no GitHub Release; pro nupkgs for testing come
  from the run's `packages-<version>` artifact.
- Package metadata is DONE (`src/Directory.Build.props`, per-csproj Description/tags,
  `LICENSE.txt`, readme, icon) and both packages pass validation.

## Tasks

1. **License finalization**: fill in `[JURISDICTION]` in `LICENSE.txt` §12 and get
   legal review before the first sale. Terms as written: per-application license,
   perpetual fallback, unlicensed use = localhost only (dev/eval).
2. **Licensing mechanism** — the real gate for publishing: license key issuance
   (sales flow) + runtime enforcement matching the license text (no valid key →
   localhost only). Design from scratch when this plan activates.
3. **Publish the pro packages**: add `StellarAdmin.Pro StellarAdmin.Pro.Identity` to
   `PUBLISH_PACKAGES` in `.github/workflows/release.yml`. That's the whole change.
4. **Pro smoke test**: `build/smoke-test.sh` only exercises the OSS package; add a pro
   variant (install `StellarAdmin.Pro.*` from the folder feed, wire `AddPro()` +
   `AddIdentity<...>`, hit a shell/Identity page).
5. **Pro repo PR/push CI** (optional until then): checks itself out into
   `stellar-admin-pro/` and OSS `master` into `stellar-admin/` (the release workflow's
   layout trick), builds the pro projects. Needs the App token secrets on the pro repo.
   Coordinate with the PR VRT work in [pr-vrt.md](pr-vrt.md) (done for the OSS repo).
   Pro docs/demos publishing lives in `docs/DocsSamplesPro` (the overlay app holding the
   DataGrid demos since the 2026-08-20 DocsSamples move to OSS); when pro docs are built,
   wire DocsSamplesGenerator to render from it as a second host, and add pro-repo PR VRT
   over the overlay app (private repo, so an App-token checkout of the OSS repo is fine).
6. **Workspace `global.json`** (optional): pin the SDK at the workspace root like the
   OSS repo does, for root-invoked builds.
