# PR visual regression tests — plan

**Index status:** needs-reconciliation. **Indexed:** 2026-09-05. Historical execution/review notes need reconciliation against current code before resuming. This migration does not infer outstanding approval or completion.

Status: **APPROVED** (2026-08-20) — Jerrie signed off on Decision 1 (Option A move),
Decision 2 (ephemeral base-vs-head, no committed baselines), and Decision 3 (advisory
sticky comment). Overlay-app shape (content links vs. two generator hosts) left as an
implementation call in Phase 1. Caveat from Jerrie: the docs generator doesn't just render
pages — it also reads the razor pages' *source* (for the `<include>` mdx exports), so every
source-path constant in DocsSamplesGenerator must follow the pages to the OSS repo.
Comparison method decided 2026-08-20: **pixel-based only** (Decision 4); the
style-snapshot layer is parked, to be revived only if pixel diffing proves problematic.

**EXECUTED locally 2026-08-20** (Phases 1-3; all working trees left uncommitted for
Jerrie's review — nothing committed or pushed). Execution notes:

- Overlay-app shape (the Phase 1 implementation call): **standalone thin overlay**
  `stellar-admin-pro/docs/DocsSamplesPro` (DataGrid pages + GridDisplayTemplates +
  duplicated thin infra: Example* tag helpers, layouts, Booking StaticData; port 5210),
  not MSBuild content links — cross-repo linked Razor compilation is fragile, and since
  no pro demo is published yet the generator keeps hosting the single OSS DocsSamples app.
  Two-host generator wiring waits for pro-release.
- Phase 1 checkpoint met: OSS repo builds/runs DocsSamples standalone; generator output
  verified byte-identical modulo the three pre-existing per-run noises (SVG attribute
  order, anti-forgery tokens, generated element-ID suffixes) — none of which affect
  pixels, further validating Decision 4. Website tree restored to HEAD afterwards.
- SkillsGenerator's source-path constants followed the pages to the OSS repo too (same
  caveat class as DocsSamplesGenerator). Pre-existing, unrelated: SkillsGenerator does
  not build (NU1010 — `Microsoft.CodeAnalysis.CSharp` has no central `PackageVersion`).
- Phase 2: vrt.mjs rewritten to pixel capture/compare (pixelmatch + pngjs via a minimal
  `util/visual-regression/package.json`); `--browser`/`CHROME_PATH` binary resolution;
  exit codes 0/1/2; report.md + summary.json + per-page highlighted diff images.
  `selftest.mjs` implements the standing self-test (radio-fix case + a determinism check
  that a clean re-capture produces zero diffs). **Self-test verified locally 2026-08-20**
  (two full runs, exit 0; patch restored cleanly both times).
- Determinism finding: two independent clean captures were **byte-identical across all
  107 screenshots** (same machine, separate browser processes). Consequence: pixels that
  pixelmatch classifies as anti-aliasing are real changes, not rasterization noise — so
  compare counts them in a separate **AA-only** column (hairline changes) instead of
  discarding them. This mattered immediately: the reversed radio fix produced only 4
  gated pixels on the Radio page, and on the Field page and open-Popover scenario the
  broken stroke ring was 100% AA-classified (0 gated / 72 AA-only pixels) — under plain
  AA exclusion those two pages' regressions were invisible. The AA-excluded count stays
  the headline severity number per Decision 4; the plan's accepted "AA tolerance could
  swallow hairline changes" trade-off is thereby retired.
- Phase 3: `vrt.yml` (advisory, fork-safe, no secrets, paths-filtered, merge-base
  worktree) + `vrt-comment.yml` (workflow_run sticky comment — required because fork PRs
  get a read-only GITHUB_TOKEN in vrt.yml itself; consumes the artifact as data and
  verifies the artifact's PR number against the run's head SHA).
- **Trial PR verified 2026-08-20**: PR #1 (revert of the radio fix) ran end-to-end on
  GitHub — Visual Regression succeeded in 5m18s (within the 4-6 min estimate), the
  sticky comment posted via workflow_run, and the report flagged exactly the predicted
  5 screenshots with pixel-for-pixel identical counts to the local self-test (Radio
  desktop/mobile 4+212 AA-only; Field desktop/mobile and Popover__open 0+72 AA-only) —
  google-chrome on ubuntu-latest reproduced the local chromium diff exactly. Artifacts:
  vrt-report ~0.9 MB, vrt-screenshots ~18 MB. The comment **update** path was also
  verified live: pushing a no-op src change re-ran the job in 5m11s and the sticky
  comment updated in place (same comment id, "Visual changes: none", still the PR's
  only comment). Finding from the trial: GitHub evaluates the `paths:` filter against
  the PR's *net* changed files — a PR whose commits cancel to a zero diff skips the
  workflow entirely and leaves the previous comment stale. Rare and semantically
  harmless (a zero-diff PR can't change visuals), accepted as-is. Remaining: Jerrie
  reviews the comment format on the PR, then closes it unmerged; the fork-PR
  (read-only token) path gets exercised by the first real community PR.

Expands item 3 of [oss-release.md](archive/oss-release.md). Three questions to settle:

1. The best page source (DocsSamples) lives in the pro repo, but the check must run on
   OSS-repo PRs. Split DocsSamples into OSS and pro parts?
2. Pro samples (the data grid) are currently not published anywhere — what happens to them?
3. When a contributor's legitimate fix changes rendering (e.g. the radio stroke fix), must
   they regenerate baselines in their PR?

## Facts that drive the decisions

- **DocsSamples is ~98% OSS.** Of ~50 page folders, only `DataGrid/` (plus
  `Shared/GridDisplayTemplates`) uses pro tag helpers. The pro footprint is one
  `ProjectReference`, one `@using StellarAdmin.Pro.TagHelpers`, and `AddPro()` in
  `Program.cs`. AppHeader/Sidebar/PageHeader/PageContainer/slots are OSS components.
- **DataGrid is not published**: no `DemoPartials` entry in DocsSamplesGenerator, no
  `data-grid.mdx` on the website. Pro docs don't exist yet (pro-release is FUTURE).
- **Fork PRs get no secrets.** Once the OSS repo is public, a PR-triggered workflow cannot
  use the release pipeline's GitHub App token to check out the private pro repo for
  contributor PRs. `pull_request_target` (secrets + untrusted code) is a known security
  footgun — not an option. Any design that needs a pro checkout excludes community PRs,
  which are the whole point of the check.
- **Captures are environment-sensitive.** vrt.mjs diffs computed styles + bounding rects;
  those depend on chromium version and font rasterization. A baseline captured locally will
  not match a capture on `ubuntu-latest`. Baselines are therefore only meaningful if
  captured in the same environment that compares — i.e. in CI.
- **No baselines exist today.** `util/visual-regression/snapshots/` is gitignored; the tool
  was built for ad-hoc before/after runs during refactors. One full run is ~772K gzipped
  JSON (the pass/fail data) + ~8.5M PNGs (human review only).
- vrt.mjs launches the `chromium` binary; GitHub runners ship Google Chrome instead —
  needs a binary-path flag. Known limit: hover/focus states are not swept; overlay
  open-states are covered by the 7 click scenarios.

## Decision 1 — where the VRT pages live

### Option A (recommended): move DocsSamples to the OSS repo, thin pro overlay stays in pro

Move `docs/DocsSamples` (minus the DataGrid pages and pro wiring) back into
`stellar-admin/docs/DocsSamples`. It was born there ("Moves docs to Pro" relocated it); its
Tailwind entry already imports OSS `theme-tokens.css` by relative path.

Pro side keeps a small overlay app (e.g. `stellar-admin-pro/docs/DocsSamplesPro`) holding
the DataGrid pages + `GridDisplayTemplates` + `AddPro()`. It can include the OSS pages via
MSBuild content links (`<Content Include="../../../stellar-admin/docs/DocsSamples/Pages/**">`)
or simply reference the shared demo tag helpers, so the docs generator still renders
everything from one host. DocsSamplesGenerator stays in the pro repo (it writes into the
closed website repo; it's a maintainer-only publishing tool) and points at the overlay app.

- OSS PRs run VRT with zero secrets; contributors can run the samples app + vrt.mjs locally.
- The public repo gains a runnable component gallery — good for contributors regardless of VRT.
- Sample content (Voyager Travel demos) is already public on the website; nothing leaks.
- Cost: a repo-move commit pair, generator path updates, and the overlay app plumbing.

### Option B: keep DocsSamples in pro; build an OSS-only page source

Duplicate ~50 components' worth of curated samples into ComponentPlayground or a new
gallery. Permanent drift between the VRT pages and the real docs samples — the
"bite us later" path. Rejected.

### Option C: no PR gate; VRT runs in pro/workspace CI or manually before release

Cheapest, but contributors get no signal, and the maintainer finds breakage after merge.
Only acceptable as a stopgap if 1.0 must ship before Phase 1 lands.

### Option D: App-token VRT that skips fork PRs (or runs post-review via label)

Works for Jerrie's own PRs only; the contributor scenario runs late (after a maintainer
labels the PR) or never. Adds `pull_request_target` risk surface if done wrong. Rejected as
the primary design (a label-triggered run could be layered on later for other purposes).

### Option E: frozen HTML fixtures (reuse the website demo export) served against rebuilt CSS

Static HTML can't reflect tag-helper C# emission changes — the most common PR type.
Rejected.

## Decision 2 — baseline strategy

### Recommended: no committed baselines — ephemeral base-vs-head comparison per PR

The PR job does both captures itself, in one runner:

1. Check out the PR head with full history; `git worktree add ../base <merge-base with master>`.
2. Build + run DocsSamples from base; `vrt.mjs capture` → `snapshots/base`.
3. Build + run DocsSamples from head; `vrt.mjs capture` → `snapshots/head`.
4. `vrt.mjs compare` → report.

Why this wins:

- **The baseline lifecycle disappears.** Nothing to commit, nothing to regenerate, no
  "who re-baselines and when" process, no 772K-per-regen churn in git history.
- **Determinism is free.** Both captures share one runner, chromium build, and font state —
  even font-loading flakiness cancels out. Chromium upgrades on the runner image can never
  invalidate anything.
- **Contributors do nothing special.** Answering question 3: no, a contributor never
  generates visuals. Their PR produces a diff report; a human judges it (see Decision 3).
- Semantics match a PR gate: it answers "what did *this PR* change visually", which is the
  reviewable question — not "does master match a blessed state from three weeks ago".
- Cost: double build + double capture per run (~4-6 min estimated). Acceptable; mitigate
  with a paths filter (`src/**`, `docs/DocsSamples/**`) so doc-only PRs skip it.
- Caveat: if a PR edits sample pages themselves, content changes surface as diffs too.
  That's informative rather than wrong — the report just needs to say which pages diff.

### Alternative: committed CI-generated baselines

Commit `*.json.gz` (never PNGs) to the OSS repo; PRs capture head only and compare.
Requires a re-baseline mechanism that runs *in CI* (a `workflow_dispatch` "update
baselines" job that commits to the PR branch, or an artifact the author downloads and
commits) because local captures don't match runner captures. Halves PR runtime but buys a
permanent process burden, baseline churn, and runner-image-update invalidations. Choose
only if the double build proves too slow in practice.

### Alternative: external service (Percy / Chromatic / Argos / Lost Pixel)

Purpose-built approval UIs, but paid or rate-limited tiers and a third-party dependency
in a repo that currently has none. Not recommended while the bespoke tool covers the
need; Argos's free OSS tier could ingest our screenshots later if the advisory-comment
review flow proves too manual.

## Decision 3 — what happens when diffs are found (enforcement)

Any legitimate visual fix (the radio stroke fix) *should* produce diffs, so a hard-fail
red X trains people to ignore the check. Recommended:

- The job **passes** whether or not diffs exist; it posts/updates a sticky PR comment:
  "Visual changes: none" or "N pages changed" with the per-page summary,
  and uploads an artifact containing the full diff report + base/head screenshot pairs.
- The reviewer (Jerrie) eyeballs the screenshots during review — intentional changes are
  approved by merging, accidental ones are caught by the comment making them visible.
- Optional hardening later: a required check satisfied by either "no diffs" or a
  maintainer-applied `visual-change-approved` label. Start advisory; add the label gate
  only if silent regressions actually slip through.

## Decision 4 — comparison method: pixel diff, not style snapshots

Decided 2026-08-20. vrt.mjs's original style-snapshot comparison (curated computed-style
properties + bounding rects) was designed for ad-hoc before/after runs where pixel
comparison would drown in cross-run rasterization noise. Two findings changed the
calculus:

- The radio stroke fix (`4a498de`) was invisible to the style layer — `stroke` wasn't in
  the curated property list. An allowlist-based comparison can always have the *next*
  missing property; pixels are exhaustive by construction.
- The ephemeral same-runner design (Decision 2) removes the noise argument: base and head
  render with the identical chromium build, fonts, frozen animations, and blocked
  external images, so pixel diffing is deterministic enough to gate on.

So the gate is **pixel comparison of the screenshot pairs** (pixelmatch with
anti-aliasing tolerance, % changed pixels + highlighted-region image per page). The
style-snapshot machinery is **parked, not deleted** — it stays in git history and can be
revived as a non-gating "explainer" layer if pixel diffing proves problematic in
practice (known trade-offs accepted for now: no property-level explanation of diffs; no
visibility into content clipped inside scrollable containers or stacked under overlays;
AA tolerance could in principle swallow hairline changes).

## Question 2 — pro samples publishing

Out of scope for the OSS 1.0 release. The DataGrid page moves to the pro overlay app
(Decision 1A), which becomes the natural home for pro demos when pro docs are built as
part of [pro-release.md](pro-release.md). Add a line there: pro docs/demos publishing +
pro-repo PR VRT (private repo, so an App-token checkout of the OSS repo is fine there).

## Plan of action

**Phase 1 — DocsSamples repo move** (OSS + pro + workspace commits)
1. `git mv`-equivalent move of DocsSamples (minus DataGrid pages, GridDisplayTemplates,
   pro reference/`AddPro()`/`@using`) into `stellar-admin/docs/DocsSamples`; fix the
   theme-tokens relative import and csproj paths.
2. Create the pro overlay app hosting the DataGrid pages; wire it to the shared pages
   (content links or duplicated thin infra — implementation call).
3. Update DocsSamplesGenerator to host the overlay app; regenerate website demos; verify
   zero diffs in `website/public/demo` output.
4. Update `vrt.mjs` default pages path, skills/CLAUDE.md references, and the
   `docssamples-moved-to-pro` memory.
   Checkpoint: OSS repo builds standalone (no pro checkout); generator output unchanged.

**Phase 2 — make vrt.mjs CI-ready** (OSS commits)
1. Chrome binary resolution (`--browser` flag or `CHROME_PATH` env; keep `chromium` default).
2. **Switch the comparison to pixel diffing** (Decision 4): capture keeps its
   determinism controls (freeze CSS, font await, blocked hosts, fixed viewports,
   scenarios) but emits screenshots only — the style-snapshot capture and compare are
   removed (recoverable from git history if revived). Compare = pixelmatch over the PNG
   pairs with anti-aliasing tolerance, reporting % changed pixels per page plus a
   highlighted-diff image. Dependency call: vendor pixelmatch/pngjs or add a minimal
   package.json (the tool is currently dependency-free).
3. Report + exit codes: markdown summary (per-page changed-pixel counts, new/removed
   pages) for the PR comment; diff images + base/head pairs in the artifact; stable exit
   codes (0 = no diffs, 1 = diffs, 2 = error).
4. **Tool self-test**: capture before/after the radio-fix commit (`4a498de`) and assert
   the Radio page is flagged. Standing rule: any visual bug that reaches human eyes
   without the tool firing becomes a permanent self-test case.
5. Optional: focus-visible scenarios via `CSS.forcePseudoState` for form controls (nice to
   have; not a blocker).

Resulting invariant: any change visible in a default-state screenshot at the two
viewports produces a signal. The remaining gaps are *coverage*, not instrumentation, and
are enumerable: hover/focus states aren't swept, only the scripted open-state scenarios
run, only components with DocsSamples pages are tested, and content clipped inside
scrollable regions isn't composited into screenshots.

**Phase 3 — the OSS PR workflow** (OSS commit)
1. `vrt.yml` on `pull_request` with paths filter; no secrets. Steps: checkout (full
   depth) → worktree at merge-base → build/run/capture base → build/run/capture head →
   compare → sticky PR comment + artifact upload.
2. Trial run on a synthetic PR (revert the radio fix on a branch) to confirm the report
   catches it and reads well.
   Checkpoint: Jerrie reviews the comment/report format on the trial PR.

**Phase 4 — later / optional**
- Label-gated required check if advisory proves too soft.
- Pro-repo PR VRT over the overlay app (App token; private repo so secrets are fine).
- Committed-baseline mode if double-build runtime becomes a problem.

## Open questions for Jerrie

1. OK with DocsSamples moving back to the OSS repo (Option A), with the generator staying
   in pro?
2. Overlay-app shape preference: MSBuild content-links to the OSS pages vs. a standalone
   pro samples app that only holds pro pages (generator then renders from two hosts)?
3. Advisory-comment enforcement acceptable to start, or want the label-gated required
   check from day one?
