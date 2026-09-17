# Parallax implementation

Status: completed — implementation, local verification and user-reviewed refinements. Updated: 2026-09-13. User authorized handoff removal and commits across all five repositories. No push or publication requested.

## Scope and implementation

Affected repositories: workspace (specification/plan), OSS (independent theme/bundle/sample integration/coverage/verification), Pro (sample font/theme selection), website (selector/docs/generated exports), consumer skills (theming guidance). All child repos were clean before implementation. Workspace initially contained the untracked `handoff-parallax/` input; it was removed after user authorization, with reusable decisions preserved in maintained sources.

Design authority: [Parallax specification](../../design/themes/parallax.md) and its maintained CSS. Source palette/geometry extracted; real-component recipes adapted from existing themes without imports. No shared CSS or component API changes. Coverage records 55 component families across 15 themes, including explicit shared-only rationale. Parallax stays outside ThemeGenerator.

Both samples and the website picker expose Parallax. Layouts load Space Grotesk 400/500/600 and JetBrains Mono 400/500. Regenerated all 412 website demos to add the theme and fonts; refreshed the final compiled Parallax asset afterward. Generated SVG attribute-order churn is retained; no pre-existing edits were discarded. Website and consumer theming guidance document fonts, dimensions and accent roles.

## Verification

- Explicit CSS build passed; compiled and exported Parallax bundles are identical, 201,493 bytes. Theme coverage passed. OSS sample build passed with existing nullable warnings; Pro sample build passed with existing XML-documentation warnings.
- Local SDK is 10.0.400; commands ran from the workspace rather than the child 10.0.100 pin. The Pro build failed silently during sandboxed project-reference evaluation and passed outside the sandbox. pnpm could not open its local database inside the sandbox; website validation passed outside it.
- `verify-parallax.mjs` passed at 1280px/390px in both modes: focus/pressed combinations, input surface roles, optional Empty borders, grouped validation and read-only fields, both menu indicator types including bold menus, keyboard toggles/sliders/switches/OTP, reduced motion, header/progress overflow, dialog/media/small variants, all four sheet sides and sidebar states. Dedicated follow-up checks passed the Pro grid containment/scrolling matrix.
- Full-page audit rendered all 59 sample routes at both widths/modes (236 renders), with every route loading the correct tokens. Inspected representative full-page Card, InputGroup, Tabs and FormSection screenshots. Fonts were downloaded from the handoff's specified Google Fonts URL and embedded temporarily for deterministic captures; required font faces were explicitly loaded.
- Final focused checks passed in all four mode/viewport combinations: adjacent and separated card dividers, rounded addon outer corners with square inner seams, disabled checked indicator visibility, exported demo theme selection, actual font loading and dark-mode application. Added Parallax to the maintained card-divider check and retained addon/disabled-state regressions in its theme verifier.
- Website lint, formatting, type checking and production build passed. Rebuilt production output after generating demos and copying the final stylesheet. Theme verifier syntax and affected-repository whitespace checks passed.
- Measured selected light contrast pairs: primary label 4.58:1, control border on white 3.12:1, warning text on tint 4.80:1, subtle text on sunken surface 4.64:1. Dark control border on recessed input is 4.11:1. These are targeted measurements, not exhaustive accessibility certification.

## Known limits and follow-up

Ten full sample pages overflow at 390px: AlertDialog, Badge, Dialog, Item, LinkButton, Popover, Separator, Sheet, Table and Tooltip. The same cases also overflow with Aurora because of fixed-width/nowrap demo compositions; Parallax font metrics and padding change some widths. Isolated Parallax dialogs/sheets, header navigation, OTP and the Pro grid passed containment checks. No unrelated demo-layout changes were made.

User review may refine aesthetic choices, including the slightly strengthened contrast colors. RTL, forced-colors mode, native select popup painting at fractional desktop scaling, and exhaustive assistive-technology behavior remain unverified. Screenshots and temporary font files are local verification artifacts; the maintained specification/CSS are the durable authority.

## Repository handoff

Commit scope spans all five repositories. OSS: theme, bundle registration, layouts/selector, coverage and verification. Pro: sample layout only. Website: theming docs, picker, 412 generated HTML demos and new theme bundle. Skills: theming guide only. Workspace: specification, guide and this indexed plan. Verification servers started on ports 5206 and 5207 were stopped; Jerrie's port 5205 was untouched.

## Toggle refinement — 2026-09-13

User review identified square standalone/spaced toggles and a muted selected border. Toggles now use the theme’s 6px owner radius, joined groups retain square inner seams and rounded outside corners, and single-item joined groups round all corners. Selection uses the strong control border plus the existing accent tint; focus and invalid colors remain intact. Updated the maintained specification, rebuilt CSS and refreshed only the exported Parallax stylesheet. Existing implementation edits were preserved across workspace, OSS and website.

Focused browser checks passed Toggle and ToggleGroup at 1280px/390px in light/dark; inspected desktop light and mobile dark captures. Checks exclude the Part Classes example’s intentional consumer styling overrides. CSS build/coverage and affected-repository whitespace checks passed.

The existing Parallax regression suite also passed all four mode/viewport combinations and reduced motion, including native toggle keyboard operation and selected hover/focus behavior.

## Button-group text border refinement — 2026-09-13

Aligned text addons with the strong control-border token used by adjacent outline buttons and inputs. Preserved individual keyboard-focus indication. Updated the specification and refreshed the compiled website stylesheet. CSS build/coverage and whitespace checks passed. The real With Text sample passed resting border comparisons and keyboard-focus checks in light/dark at 1280px/390px; inspected its light desktop capture. Changes were kept with the implementation across workspace, OSS and website.

## Maintenance documentation — 2026-09-13

Following user acceptance, expanded the maintained Parallax specification with future-control mappings and focused review routes. Updated the custom-theme development skill and workflow with grouped corner coverage, strong versus quiet border roles, button-group versus input-group focus ownership, deliberate consumer overrides, transition settling and keyboard focus-visible verification. These general checks preserve theme-specific decisions rather than applying Parallax styling to existing themes. Documentation-only follow-up; checked local links, skill discovery and whitespace.
