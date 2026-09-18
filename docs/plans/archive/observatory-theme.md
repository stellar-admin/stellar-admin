# Observatory theme

## Code audit — 2026-09-18

Current status: **completed**. Observatory CSS, specification and `verify-observatory.mjs` are present.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: completed locally. Updated: 2026-09-12. No commits, pushes or publishing performed.

Jerrie authorized end-to-end implementation from `observatory_handoff/`, selected compact density, and requested autonomous best-effort decisions with special-review notes. The handoff was removed with user authorization on 2026-09-12 after preserving reusable guidance in the maintained specification and CSS. The maintained authority is the [Observatory specification](../../design/themes/observatory.md) and its linked CSS.

## Delivered

- Independent Observatory bundle with exact source foundations, `.dark` support, compact table density, component/state recipes, and explicit coverage across 55 component families. No ThemeGenerator registration, upstream theme import, shared CSS edits or new Tag Helper APIs.
- Bundle build registration, OSS and Pro sample selection, website demo picker, consumer documentation and theming skill guidance.
- Regenerated all 412 website demos from their source to include the new theme. Exported the new bundle and updated the generated site stylesheet fingerprint. No component source snippets changed. SVG attribute ordering changed during export; source icon geometry was not changed.
- Permanent `util/visual-regression/verify-observatory.mjs` checks and Observatory inclusion in the existing segmented-control comparison. The optional `OBSERVATORY_PRO_URL` enables grid containment checks; `OBSERVATORY_FONT_CSS` accepts embedded local font CSS for repeatable captures.

## Decisions for special review

1. Preserved the source colours despite inaccurate contrast claims in the export. Example measured ratios: light control border on white 1.54:1, dark control border on the input surface 1.47:1, and light warning text on its tint 3.67:1. Micro-labels remain 9px as designed. Review these if the product needs stronger contrast/readability targets.
2. Dark inputs use the intended dedicated `#121a24` token, which is lighter than the card, despite contradictory prose. Corrected the source's later light alias so it does not overwrite the dedicated dark value on `<html class="dark">`.
3. Used the actual source's 2px line-tab underline, not its prose description of 3px. Removed trailing tab margin to avoid a scrollbar caused only by trailing whitespace. Long navigation still scrolls.
4. Preserved library behavior: sheet sides, sidebar variants/breakpoints, and scrollable tables. Did not turn all mobile tables into card lists or automatically change right sheets to bottom sheets. The sheet wrapper receives no duplicate padding; its footer stays reachable with a sticky bottom position.
5. Inferred unspecified families from the same source primitives, including OTP, questionnaire, messages, attachments and empty states. Extra-small actions are 22px, small 26px, default 32px and large 40px. Theme-local card width bounds and wrapping grid footers fix narrow-screen Pro overflow while retaining table scrolling.
6. Found an existing alert shorthand defect: `Alert/_Shorthand` produces empty title/description text in both Observatory and unchanged Nova. Explicit alert child tags render. This independent Tag Helper defect was not changed by the theme work.

## Verification performed

- `npm run build:css`: all 12 bundles built successfully; coverage passed for 55 components × 12 themes. Observatory is nonempty (194,125 bytes), has no undefined theme-private variable references, and matches its website-exported bundle byte for byte.
- `dotnet build stellar-admin/docs/DocsSamples/DocsSamples.csproj`: passed, including the library. `dotnet build stellar-admin-pro/docs/DocsSamplesPro/DocsSamplesPro.csproj -m:1`: passed. Used installed SDK 10.0.400 from the workspace rather than the OSS 10.0.100 pin; no pins or user configuration changed. Existing nullable/XML warnings and NU1900 vulnerability-feed access warnings remain.
- DocsSamplesGenerator built successfully with `--no-restore -m:1` after an initial `dotnet run` reported a build failure without compiler errors. `dotnet run --project stellar-admin-pro/docs/DocsSamplesGenerator --no-build` then exported all 412 demos successfully. All demo theme allowlists include Observatory and all locally referenced demo assets exist.
- Captured all 55 families in Chromium at 1280px and 390px, light/dark: 220 captures with the exact Plex fonts loaded and native scrollbars visible. No page-level horizontal overflow. Initial captures with partial font loading were superseded using temporary embedded versions of the five specified Plex font faces.
- `verify-observatory.mjs`: passed all four mode/viewport combinations for pressed focus, grouped read-only/invalid focus, checkbox/radio menu selection and feedback, native toggle/slider/OTP keyboard operation, dialog and small/media alert-dialog containment, reachable dialog footers, all four sheet sides and Escape dismissal, sidebar collapse, progress containment, and reduced motion. The Pro grid containment/scrolling check also passed.
- Existing segmented-control comparison, restricted to Observatory for this run: passed both modes and widths, including identical tab/segment appearance, keyboard/change behavior, disabled groups, labels, model binding, validation and reset.
- Pro grid and exported demo integration: passed light/dark desktop/mobile with exact Plex fonts, compact rows, no page overflow, and successful storage-driven switching to the Observatory bundle. Narrow tables retain native horizontal scrolling.
- Website `pnpm lint`, `pnpm types:check`, and `pnpm build`: passed. Formatter/type/syntax checks and `git diff --check` passed for the affected files/repos. Website checks ran with access to pnpm's cache because its launcher could not open its database in the sandbox.

## Review artifacts and limits

Focused captures live locally in `stellar-admin/util/visual-regression/snapshots/observatory/`; the broader 220-capture inventory and contact sheets are in `/tmp/observatory-review/`. These are verification artifacts, not the maintained design authority. Font files were cached only in `/tmp`; applications still load fonts from their layouts.

Rendering was checked in Chromium, not Firefox/Safari, and this was not an exhaustive accessibility audit or a claim of pixel identity for every inferred component. App-specific mobile compositions and unspecified components remain the most useful aesthetic review targets.

## Repository state and handoff

All child repos were clean before work; the workspace initially contained only the untracked handoff. Changes remain uncommitted on the existing `master` branches: workspace specification/plan/guide; OSS theme/registration/samples/coverage/verification; Pro sample layout; website selector/docs/412 regenerated demos/generated assets; consumer theming skill. Existing source edits were not discarded. No commits or publishing are authorized by this record.

The verification servers started on ports 5206, 5207 and 5208 were stopped at completion. Jerrie's port 5205 was left alone. No implementation work remains; the six items above are follow-up review notes, not an implicit queue of authorized new work.

## Sidebar scrollbar review — 2026-09-12

Jerrie requested removal of the visible sidebar scrollbar. Updated only Observatory's sidebar-content scrollbar styling, preserving native overflow and scrolling. Rebuilt all theme bundles and refreshed the exported Observatory stylesheet. Chromium with native scrollbar suppression disabled confirmed hidden sidebar chrome, working scroll position changes, and automatic scrolling to the last focused navigation link in light/dark modes at 1280px and 390px. Inspected the desktop capture. The maintained specification now records this appearance preference.

## Initial native select investigation — 2026-09-12 (superseded by resolution below)

Jerrie's Chrome screenshots show a missing bottom edge on the Observatory Year options popup, while Ice shows the edge. At this stage it remained an unresolved visual defect; the closed select's computed border does not establish that the popup paints correctly. Compared theme rules and captured the real Year popup with IBM Plex Sans loaded, downward placement, fractional positioning, and CSS zoom factors 1.25/1.5/1.75/2/2.25. The inspected local headless Chromium captures retain the bottom edge. Device-scale emulation also affects popup placement/capture, so those runs do not establish equivalence to Jerrie's Chrome display. No speculative CSS change was made. Requested Chrome page zoom and display scaling to reproduce the failing environment. Further work must verify the actual open popup against Ice before declaring this fixed.

### Select popup resolution — 2026-09-12

The reported environment is Chrome at 100% zoom on a 1.6× display; Concourse also fails while Ice and the other themes work. Reproduced with an isolated headed Chromium window on the actual Hyprland desktop and `grim` captures, using embedded IBM Plex Sans and Source Sans 3. The defect depends on popup positioning as well as font metrics: Observatory's old 13px size lost its bottom edge at the 150px test offset, and Concourse's 13.5px size failed with its actual font. Headless captures at simulated scale painted the border and were insufficient evidence.

Changed native-select text only to 12.5px in Observatory and 14px in Concourse, retaining existing native behavior, heights and focus styling. CSS builds passed and both exported website stylesheets were refreshed. The new desktop `verify-native-select-popup.mjs` checks light-mode painted borders at two positions, captures both modes and verifies native keyboard selection. All eight final cases passed; inspected dark popup captures show complete bottom edges. Its pixel detector distinguished the old Observatory failure (about 15% bottom-edge coverage) and Concourse failure (0%) from the corrected captures (100%). Browser helper options now allow isolated headed capture while retaining existing headless defaults. Updated both maintained specifications with the workaround and its platform-specific verification limits. No desktop settings were changed; all temporary test browsers were closed.


## Final appearance review and documentation cleanup — 2026-09-12

Fixed the Empty bordered example by replacing the theme's `border: 0` reset with `border-width: 0`, retaining semantic border color when an app utility restores the edge. Verified bordered and default states in Chromium in both modes at desktop/mobile widths; added permanent coverage to `verify-observatory.mjs` and refreshed the exported stylesheet.

At Jerrie's request, Observatory dropdown radio items now reuse the existing SVG checkmark, and both checkbox/radio indicators are on the right in Observatory and Ice. Preserved hidden unchecked indicators and interaction behavior. Updated both theme specifications and their verification scripts, rebuilt the theme bundles and refreshed both website assets. Sixteen targeted browser cases passed across the two themes, two modes, two widths and two menu types; inspected representative captures. These were targeted checks, not another run of the entire integration suite.

Jerrie accepted Observatory's appearance and requested durable extension guidance and handoff removal. Updated the Observatory specification with asset/font provenance, future-control recipes, approved menu placement, utility-border composition and verification entry points. Updated the custom-theme skill and workflow with these reusable lessons and the limits of native-popup testing. Removed the temporary handoff after reviewing its README, source, asset notes and capture manifest against maintained sources. The CSS header's original extraction path is historical provenance, not a dependency or a direction to consult the deleted folder. No runtime code changed during this documentation cleanup.
