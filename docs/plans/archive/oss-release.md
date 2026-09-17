# OSS release (StellarAdmin.TagHelpers 1.0)

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-21.** `StellarAdmin.TagHelpers 0.0.1-preview.6` is live on
nuget.org, published by the pipeline and install-tested by Jerrie. Every section is done.
Split out of [release-pipeline.md](release-pipeline.md), whose pipeline work (Phases 0-3
+ pro metadata) is DONE and verified end-to-end; pro-specific release work moved to
[pro-release.md](../pro-release.md).

Goal: ship `StellarAdmin.TagHelpers` publicly on nuget.org.

The title says 1.0, but what went out is a prerelease, and deliberately so: this plan was
about the *route* to nuget.org, and that route is now proven end-to-end. Cutting 1.0 is
one more dispatch of the same workflow with a `1.0.0` version, gated on the API being
considered stable rather than on anything here.

## 1. Component fixes before release

**DONE 2026-08-20.** The radio artifact was the lucide circle indicator's
stroke rendering (invisible upstream via the Radix cascade, visible with a
peer-sibling native input); fixed structurally with `stroke-none` in
components.css. Switches confirmed fine. Bonus: themes synced with upstream
shadcn, and the choice-card pattern (field-label wrapping a field) now works
end-to-end with native inputs (`has-[:checked]` generator translation) and is
documented with samples on the Radio and Checkbox pages.

## 2. Builder API naming: `AddUI()` vs `AddTagHelpers()`

**DONE 2026-08-20 — renamed to `AddTagHelpers()`.** Matches the package name and the
package-named registration convention (`AddPro()`, `AddIdentity()`); "UI" failed as a
tier discriminator since Pro is UI too. `StellarAdminUI{Builder,Options,MenuOptions,
Extensions}` became `StellarAdminTagHelpers*`, with all docs/readme/skill/website
references swept.

## 3. Visual regression tests on PRs (OSS and pro)

**DONE 2026-08-20** — full plan + execution notes in [pr-vrt.md](../pr-vrt.md); committed,
pushed, and verified end-to-end on trial PR #1 (close it unmerged after reviewing the
comment format). Summary of the decisions:

- DocsSamples moves back to the OSS repo (it is ~98% OSS content); a thin pro overlay app
  keeps the DataGrid pages. DocsSamplesGenerator stays in pro and follows the new paths —
  including its razor *source* reads for the `<include>` mdx exports.
- No committed baselines: the PR job captures base (merge-base worktree) and head on the
  same runner and compares — the baseline lifecycle and cross-environment determinism
  problems disappear; contributors never regenerate visuals.
- Comparison is **pixel diff only** (pixelmatch, AA tolerance); vrt.mjs's style-snapshot
  layer is parked after the radio stroke fix exposed its allowlist blind spot.
- Enforcement is advisory: the job always passes and posts a sticky PR comment + artifact
  with diff images; the reviewer judges intentional vs. accidental.

## 4. Go live on nuget.org (was release-pipeline Phase 4)

1. **DONE 2026-08-20** — OSS repo is public; `release.yml` now validates the OSS
   package with no exclusions (52/119 stay for the pro packages).
2. **DONE 2026-08-20** — trusted publishing policy created on nuget.org for repo
   `stellar-admin/workspace` / workflow `release.yml`, and `NUGET_USER` set as a
   repo secret on `workspace`. There was nothing to copy: the old secret went away
   with `60e7b15` when the release half of the OSS CI workflow was removed. The value
   is just the nuget.org username the policy belongs to.
3. **DONE 2026-08-20, without the reviewer gate.** The `nuget-org` environment exists,
   but GitHub rejected the required-reviewer rule: protection rules need a public repo
   or a Team/Enterprise plan, and `workspace` is private under the free org plan.
   Decision (Jerrie, 2026-08-20): accept it rather than pay or go public. He is the
   only org member and repo collaborator, so a required reviewer would only be him
   approving his own dispatch; the real guards are `publish` defaulting to false and
   the version-bump guard below. Revisit if the org ever gains members.
4. **DONE 2026-08-20** — `publish` runs in `nuget-org` with `id-token: write`, logs in
   via `NuGet/login@v1` using `secrets.NUGET_USER`, and pushes to
   `https://api.nuget.org/v3/index.json` with the login's temporary key. The GitHub
   Packages push was **dropped**, not kept as a mirror: it was staging *until* nuget.org
   went live, it carried the same single package, and two feeds means two ways for a
   publish to half-succeed. The `packages` environment is now unused. If the pro tier
   ever needs a private feed, `pro-release.md` decides that on its own terms.
5. **DONE 2026-08-20** — `--no-symbols` and `--draft` are gone, so snupkgs ride along to
   nuget.org and the release is published outright (it runs after the push, so the
   version is installable by the time anyone sees the release). The four leftover
   `v0.0.1-preview.*` draft releases were **deleted** rather than published — those
   versions never reached nuget.org, so a visible release would advertise something
   nobody can install. Their tags were kept on all three repos: they record what those
   runs built, and the version-bump guard reads them.
6. **N/A 2026-08-21** — there was no old trusted publisher policy for
   `stellar-admin/stellar-admin` to remove (Jerrie confirmed on nuget.org). The
   plan assumed one from the era when the OSS repo published its own packages; the
   only policy that ever existed points at `stellar-admin/workspace`, so nothing
   else can mint a publishing key.
7. **DONE 2026-08-20 — dropped.** `--generate-notes` lists merged PRs and the OSS repo
   is committed to directly, so it produced nothing but a compare link. Decision
   (Jerrie): drop it rather than move to a PR-based flow or maintain `--notes-file`.
   Releases are created with an explicitly empty body (`--notes ""`, which `gh` needs
   when running non-interactively) and exist to carry the tag and the package assets.
   Hand-written notes can be added later if a release ever warrants them.

**DONE 2026-08-20 — version-bump guard** (was a nice-to-have, pulled in as the
replacement for the reviewer gate): with `publish` checked, the build job refuses a
version that is not greater than the highest existing `v*` tag on `workspace`, before
spending a build on it. Comparison is SemVer precedence, not `sort -V` alone — `sort -V`
ranks `1.0.0` *below* `1.0.0-rc.1`, which would have rejected the 1.0 release itself, so
the core and the prerelease label are compared separately. Verified against the live tag
list for: next prerelease, same version, older version, `0.0.1` over `0.0.1-preview.5`,
`preview.10` over `preview.5`, and the no-tags-yet path.

**DONE 2026-08-21 — checkpoint met.** `release.yml` was dispatched for
`0.0.1-preview.6` with `publish` checked: the version guard accepted it, the packages went
to nuget.org, all three repos were tagged, and the OSS repo carries a `v0.0.1-preview.6`
release flagged as a prerelease. Jerrie installed the package from nuget.org and tested it.

## Nice-to-haves (unplanned)

Not done, and not blocking anything — carried here so they are not lost with the plan:

- Dependabot/Renovate on the workspace repo for the pinned action versions.
