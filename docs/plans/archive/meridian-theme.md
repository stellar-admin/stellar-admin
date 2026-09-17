# Meridian theme implementation

Status: completed — implemented, verified and accepted by Jerrie on 2026-09-12. Repositories: workspace, OSS, Pro samples, website, consumer skills. Jerrie authorized closeout documentation and commits across all five repositories. Initial worktrees were clean except the untracked Meridian handoff in the workspace. Pushes and publication remain outside scope.

## Scope and decisions

Implemented the independent [Meridian theme specification](../../design/themes/meridian.md) and its maintained CSS, bundle registration, explicit coverage for all 55 existing component families, sample selection/fonts, website picker, 412 regenerated demos and consumer theming guidance. The handoff is retained unchanged. No shared component CSS or public component API changed. Existing selector/state mappings informed implementation; Meridian imports no other theme and stays outside ThemeGenerator.

Jerrie authorized autonomous implementation and best-effort inference, explicitly selecting trailing dropdown checkmarks. Both checkbox and radio menu items reuse the emitted check SVG to the right of text, show it only when checked, and retain neutral hover/focus without persistent selected fill. Missing designs compose Meridian's existing primitives. Preserve library sidebar breakpoints, sheet sides and table scrolling; prototype table-to-card conversion and automatic sheet repositioning remain application choices. Default table spacing follows the supplied 11px rhythm; no density preference API was introduced.

Rendering correction: the handoff's late root alias overwrites its intended raised dark input surface. Limit that alias to light mode so dark fields render `#23201a`. A regression now checks that dark fields differ from card surfaces and light fields share them. Other deliberate inferences and contrast measurements are maintained in the specification.

## Verification

- `npm run build:css`: passed for all 13 nonempty theme bundles. `node util/theme-coverage/check.mjs`: 55 component families × 13 themes. Meridian has no undefined private tokens; its exported stylesheet matches the library bundle byte for byte.
- OSS library and both sample apps built successfully using installed .NET SDK 10.0.400 from the workspace (the nested OSS pin is 10.0.100). Existing nullable/XML documentation warnings and NU1900 vulnerability-feed access warnings remain. A sandbox-only Pro build returned no diagnostic before failing; the normal elevated build and sample run succeeded. Initial sandbox socket restrictions required elevated execution for local servers/browser checks.
- `MERIDIAN_FONT_CSS=/tmp/meridian-fonts-inline.css MERIDIAN_PRO_URL=http://localhost:5207 node stellar-admin/util/visual-regression/verify-meridian.mjs http://localhost:5206`: passed in light/dark at 1280px/390px, plus reduced motion and Pro grid checks. Covered trailing checked-only indicators, checked/unchecked resting and hover surfaces, focused/pressed actions, grouped invalid focus, native input operation, OTP, slider keyboard operation, optional Empty borders, typography, dark input surfaces, long header scrolling, progress, small/media/sticky-footer dialogs, all sheet sides with Escape dismissal, and sidebar containment.
- Temporary full-page audit rendered 53 sample routes in four mode/width combinations (212 pages). Compared default tabs and segmented controls including selected/unselected geometry, colours, type and shadows; they match in every Meridian case. Inspected representative screenshots across fields, actions, cards, navigation, overlays, attachments, messages, questionnaire, avatars and Pro grid. Screenshots are in ignored `stellar-admin/util/visual-regression/snapshots/meridian/` and `/tmp/meridian-audit/`. Unused font faces were naturally not loaded on pages without that text role; dedicated checks explicitly loaded and verified all three families.
- The temporary audit's final cleanup used an incorrect browser method after all page checks completed; maintained regression scripts and subsequent focused audits use the correct cleanup. It did not invalidate recorded page results.
- Full-page mobile overflow in AlertDialog, Badge, Dialog, Item, Popover, Separator, Sheet, Table and Tooltip was reproduced in unchanged Observatory and Nova with native scrollbars visible. Fixed-width/nowrap sample compositions remain outside this theme change. Some other narrow sample groups extend beyond their padded example region; no universal clipping was added to hide legitimate overflow.
- `dotnet run --project stellar-admin-pro/docs/DocsSamplesGenerator`: generated 412 demo partials. Compared every generated HTML document with its prior version, accounting only for the new theme-list member/font link, attribute ordering, generated IDs and antiforgery values; no unexplained changes remain. Existing SVG attribute values/path content were compared, not discarded by a broad SVG normalization. No pre-existing website edits existed.
- Exported OSS card and dropdown demos verified in Chromium with persisted `demo-theme=meridian`, dark mode, three loaded font families and interactive menus. Pro grid verified in its running samples app; it is not among the current website exports.
- Website `pnpm lint`, `pnpm types:check`, and `pnpm build`: passed. Consumer theming frontmatter/content and cross-repo theme lists reviewed. All affected repositories pass `git diff --check`.

## Review notes and remaining limits

No implementation or visual-review work remains pending; Jerrie accepted the final theme. The source palette's contrast claims are overstated: light control borders measure about 1.78:1, subtle text on the page 4.38:1, and warning text on its tint 3.87:1. Preserve the accepted appearance; do not call it accessibility-certified. Native select popup painting at fractional desktop scale, RTL and forced-colors behavior were not verified.

## Final working tree

Workspace: maintained specification, archived plan/index, OSS guide; original handoff stays untracked. OSS: new CSS and regression script, coverage manifest/guide, bundle registration, sample layout/picker and segmented comparison theme list. Pro: font/theme selection in the sample layout. Website: theming page, picker list, 412 generated HTML demos and Meridian stylesheet. Consumer skills: handwritten theming guide. Generated screenshots, build logs and font downloads remain outside tracked source.

Temporary verification servers on 5206, 5207 and 5211 are stopped at closeout. Jerrie's port 5205 was not touched. Restart an already-running DocsSamples app to pick up the compiled Razor theme list, then select Meridian or use `?theme=meridian&mode=light|dark` on any ordinary component route.

## Accepted review and closeout

Removed the full-width bottom border from horizontal line tabs in Meridian, Ice and Observatory, retaining each active-tab underline. Rebuilt all 13 bundles and refreshed only the three affected exported stylesheets. Browser checks passed for all 12 theme/mode/viewport combinations (light/dark, 1280px/390px), asserting a borderless tab list and visible 2px active indicator; representative screenshots were inspected. Ice used fallback fonts in this focused border check; Meridian and Observatory used embedded fonts. Theme coverage and whitespace checks passed.

The maintained Meridian specification now includes recipes and an extension checklist for future controls, independently of the temporary handoff. The custom-theme workflow records the approved line-tab convention, late root-alias cascade checks and font-role verification lessons. Consumer setup guidance already includes Meridian's three font families and stylesheet. The original handoff is retained locally and excluded from commits; screenshots, downloaded fonts and temporary audit scripts remain local verification artifacts.

Committed product changes: OSS `e5f3172`, Pro samples `84de75c`, website `3f957846`, consumer skills `a1ab968`. The workspace closeout commit contains this record and the maintained specifications/workflow. No push or publication was performed.
