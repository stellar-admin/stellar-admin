# Theme selector for docs website demos

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

Status: **COMPLETE 2026-08-17** — all three phases executed, CDP-verified, and
committed. Phase 2 initially shipped a styled native `<select>`; Jerrie asked for the
shadcn select, so the selector now uses the CLI-generated base-ui select
(`src/components/ui/select.tsx`, `items` map on the root for capitalized labels, `sm`
trigger). Phase 3 resolved the height clipping observed after live theme swaps.

Let the reader switch any exported demo between the 8 StellarAdmin themes (luma, lyra,
maia, mira, nova, rhea, sera, vega) from a selector on the docs website's `Demo`
component. Builds directly on the dark-mode sync shipped 2026-08-17: the selector only
writes a localStorage key, and the script the generator injects into every exported
demo page reacts to it — the `storage` event propagates the change live to iframes and
full-preview tabs because everything is same-origin.

## Design

- **State**: `localStorage["demo-theme"]`, values = theme names, absent/invalid → nova.
  Global (all demos share the choice), persistent, never in the URL.
- **Demo pages** (generator-injected script, extension of the existing theme-sync
  script): resolve the theme before first paint and point the theme `<link>` at
  `/demo/tag-helpers/assets/stellar-admin.<theme>.css`; on a `storage` event, swap by
  loading the new stylesheet first and removing the old one on its `load` event (no
  unstyled flash). Theme names are validated against the list embedded at export time.
- **Theme bundles** are exported under **stable names** (`stellar-admin.<theme>.css`,
  no fingerprint) so the runtime URL scheme is trivial. Vercel serves `public/` with
  etag revalidation by default (vercel.json sets no cache headers), so stable names
  stay correct across deploys; only immutable-cache optimization is lost, which was
  never configured anyway. `site.css` / `stellar-admin.js` keep their fingerprinted
  scrape-based flow.
- **Theme list source**: the generator derives it from the OSS repo's
  `Client/css/themes/*.css` on disk (same repo-relative dependency the DocsSamples
  project reference already encodes; mirrors how `build:css` derives its list). The
  website selector hardcodes the 8 names — themes change rarely, and a mismatch only
  shows a dead option.
- **Orthogonal to dark mode**: every bundle contains light + dark variables; the
  `.dark` class mechanism is untouched. No per-theme fonts exist (no theme defines
  `--font-sans`), so the single Geist link keeps working.

## Phase 1 — Generator: export all themes + runtime switching in demo pages

All in `stellar-admin-pro/docs/DocsSamplesGenerator/Generator.cs`:

1. New download step: enumerate the theme names from
   `<repoRoot>/../stellar-admin/src/StellarAdmin.TagHelpers/Client/css/themes/*.css`,
   fetch each `/_content/StellarAdmin.TagHelpers/stellar-admin.<theme>.css`, write to
   `assets/stellar-admin.<theme>.css`.
2. `FixDemoContent`: rewrite the theme `<link>` (currently rewritten to the
   fingerprinted nova name by the generic asset regex) to a stable-named link marked
   `data-sa-theme` (a marker attribute rather than an id, since a swap briefly has two
   theme links in flight), followed by an inline `saThemeInit()` call so a non-default
   theme applies during parsing — the `_CleanLayout` full-page demos have no opacity
   fade to mask a late swap. Exclude theme bundles from the fingerprinted scrape so no
   orphan copy ships.
3. Extend the injected script with the theme half (embedded valid-theme list, initial
   pre-paint href fix, load-then-remove swap on `storage` events for `demo-theme`).
4. Regenerate the export.

**Verify** (headless chromium over CDP against a static server on `website/public`,
same harness as the dark-mode check): fresh load honors a stored `demo-theme`; a
`storage` write from another same-origin context swaps a live page without unstyled
flash; invalid stored value falls back to nova; dark class still applies on a non-nova
theme. Spot-check all 8 bundles exist in `assets/`.

**Checkpoint: stop for Jerrie's review before Phase 2.**

## Phase 2 — Website: `useDemoTheme` hook + selector UI

All in `website/`:

1. `useDemoTheme()` hook: reads `localStorage["demo-theme"]` (default nova), setter
   writes it and dispatches a custom window event; subscribes to that event plus the
   `storage` event so multiple `Demo` blocks on one page stay in sync. SSR-safe
   (localStorage only touched in effects/callbacks).
2. Theme select component (shadcn CLI select to match the existing base-ui kit, or a
   styled native select if the generated one is heavyweight for a toolbar — decide in
   review of the actual markup).
3. `Demo` gets a slim toolbar row (right-aligned selector) above the preview for both
   `preview` and `fullscreen` modes; adjust the wrapper's first/last-child border
   styling for the extra row. `DemoPreview`/`DemoLaunch` themselves stay untouched —
   the iframe reacts via the storage event, and a launched tab reads the key itself.

**Verify** in `vite dev` in a real browser: switching the selector restyles the iframe
live and in a separately opened full preview; choice persists across page navigation
and reload; two demos on one page keep their selectors in sync; site dark-mode toggle
still syncs independently.

**Checkpoint: stop for Jerrie's review before Phase 3.**

## Phase 3 — DemoPreview height via ResizeObserver (deliberately last)

Replace the one-shot `load`-event height measurement in
`website/src/components/demo-preview.tsx` with a `ResizeObserver` on the iframe's
body, so height tracks theme swaps (radius/spacing changes) and interactive demos
(accordions, collapsibles) without a reload. Keep the `load` listener only to attach
the observer to each new document.

**Verify**: switch themes and open/close an accordion demo — iframe height follows
without scrollbars or dead space.

## Deferred / out of scope

- Shareable theme links (`?theme=vega` query param layer) — skipped for now.
- Generator-emitted theme list for the website selector (only needed if hardcoding
  ever bites).
