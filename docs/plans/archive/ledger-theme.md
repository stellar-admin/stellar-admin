# Ledger theme

## Code audit — 2026-09-18

Current status: **completed**. `src/StellarAdmin.TagHelpers/Client/css/themes/ledger.css`, its bundle registration, `util/theme-coverage/coverage.json`, and `util/visual-regression/verify-ledger.mjs` are present. The PR VRT workflow invokes the Ledger check; sample and website theme selectors include Ledger. The old foundational-checkpoint index status was stale. The maintained design is `docs/design/themes/ledger.md`.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: implemented and verified. Last updated: 2026-09-11.

## Scope and authorization

Implement the custom Ledger theme, infer missing designs from its intent, and establish reproducible maintenance for future components. The user approved the foundational visual review and authorized full expansion. Commits across all repositories authorized on 2026-09-11; publishing is not authorized. All five repositories are on `feat/ledger-theme`.

## Design and implementation

See [Ledger specification](../../design/themes/ledger.md). The original design export was deleted at the user’s request; the specification and maintained CSS now own the design contract. Ledger is hand-authored and independent of ThemeGenerator's upstream sources. Existing tag helper APIs and interaction semantics remain the component contract.

- Full Ledger light/dark CSS covers the shipped visual component families. Shared-only components have explicit rationale in the coverage manifest.
- The build and CI enforce inventory and coverage for 54 component families across nine themes. Seven failure-case tests verify the checker. The porting skill records the workflow for extending custom themes.
- Review ordinary DocsSamples component pages using `?theme=ledger&mode=light` or `?theme=ledger&mode=dark`; dedicated Ledger pages were deleted at the user’s request. `stellar-admin/sandbox/html/ledger.html` remains the approved standalone foundation snapshot.
- Pro samples support theme/mode queries. The data grid uses the existing card/table structures with Ledger styling.
- Website picker, installation/theming documentation, consumer theming skill, and all 406 exported demos are updated. Fonts are loaded by sample/exported layouts; consumers supply Lexend and JetBrains Mono in their own layout.

## Verification

- CSS build passed outside the sandbox; all nine bundles contain CSS. Inside the sandbox Tailwind had exited successfully while producing empty bundles. The build script now rejects empty/missing outputs.
- OSS and Pro DocsSamples builds passed using `/usr/share/dotnet/dotnet` SDK 10.0.111, compatible with the OSS 10.0.100 pin. Existing nullable/XML documentation warnings remain. The demo generator exported 406 demos successfully, with one existing nullable warning.
- Theme coverage validation passed; all seven checker tests passed.
- Chromium captured 61 sample pages at desktop/mobile widths in each mode, including overlay scenarios. Ignored output directories: `stellar-admin/util/visual-regression/snapshots/ledger-full-light` and `ledger-full-dark`. Representative controls, navigation, messages, questionnaire, overlays, and Pro data grid were visually inspected. Captures are review artifacts, not a claim of exhaustive visual or accessibility approval.
- The permanent `verify-ledger.mjs` check passed in both modes and runs in the PR workflow: slider keyboard changes, switch state/geometry, OTP entry, group focus, additive pressed/focus shadows, radius overrides, and reduced motion. Body contrast is 15.92:1 light / 15.26:1 dark; primary button text is 6.59:1 light / 7.52:1 dark. This is focused contrast verification, not an entire component-suite accessibility audit.
- Shared CSS hooks cover button transitions, textarea minimum height, page title typography, and active tab surface. Compared all standard computed properties of 432 representative controls across the eight upstream-derived themes in light/dark mode against the previously exported Git HEAD bundles: no differences. The earlier foundational Nova screenshot comparison was byte-identical.
- Website `pnpm lint`, `pnpm types:check`, and `pnpm build` passed. Browser interaction confirmed all nine picker options; choosing Ledger persisted the preference and switched the embedded demos to the Ledger stylesheet and Lexend font.
- New Ledger CSS/scripts, generator C# formatting, and Git whitespace checks passed. The shared `components.css` already fails whole-file formatter checks at Git HEAD; it was not mass-reformatted.
- `dotnet run --project src/SkillsGenerator -- --check` reports existing drift in `components-index.md`, `components/form-row.md`, and `components/form-section.md`. The generator, those component sources, curated example selection, and those samples are unchanged by this task. The unrelated reference drift is left for separate maintenance.

## Repository handoff

| Repository | Changes |
| --- | --- |
| Workspace | Maintained design specification, plan/index, OSS guide, component-porting skill. |
| OSS | Ledger CSS/bundle registration, shared hooks, coverage checker/manifest/tests, theme selection, browser verification and CI integration. |
| Pro | Sample theme selection and exported demo font support. |
| Website | Theme picker, human-facing docs, regenerated demos and CSS assets. |
| Consumer skills | Ledger selection, font setup, and custom-theme guidance. |

The user authorized committing the completed changes across all five repositories on `feat/ledger-theme` on 2026-09-11; nothing is pushed or published. Earlier uncommitted-status notes below record their respective verification checkpoints. Local verification servers are stopped at handoff. Jerrie's port 5205 was not touched. No Ledger implementation work remains; the separate reference drift and pre-existing whole-file formatting issue above remain outside this task.

## Sidebar scrollbar follow-up

Ledger omitted the `no-scrollbar` utility used by every upstream theme on `.sa-sidebar-content`. Added it while preserving overflow scrolling, rebuilt the nine bundles, and refreshed the website Ledger asset. The screenshot browser previously hid native scrollbars globally, concealing this difference; focused Ledger verification now disables that browser flag and checks hidden scrollbar styling, zero gutter, and working scroll position in light and dark modes. The full focused browser check passed after this fix.

## Input-group demo follow-up

Corrected Ledger grouped read-only controls to retain the transparent inner surface, gave inputs in block-aligned groups explicit minimum height and vertical padding to prevent flex collapse, and restored horizontal/vertical button-group end corners using the same structural override approach as upstream themes and the configurable Ledger radius. Rebuilt all bundles and refreshed the website Ledger CSS. Focused browser checks passed in light/dark modes, including new assertions for read-only backgrounds, stacked spacing, and both group orientations at default/custom radii. Visually inspected the Buttons, Text, Label, and ButtonGroup demos. Git whitespace checks passed; changes remain uncommitted.

The input-group spacing follow-up removes doubled padding at inline addon/field boundaries: the default internal gap is now 8px instead of 24px, with 12px outer padding retained. Rebuilt the bundles and refreshed the website Ledger asset.

Refined the spacing after edge-alignment review: all grouped fields and addons now use 6px horizontal outer padding and a default 6px internal gap. This supersedes the preceding 12px/8px values. The three Buttons examples now have identical measured leading text insets (7px including the border), verified by a permanent browser assertion in both modes. Focused checks passed; inspected the updated Buttons screenshot and refreshed the website CSS.

## Switch positioning follow-up

Removed Ledger’s duplicate vertical transform: shared CSS already centers the thumb through the independent translate property. Checked-state styling now uses a general sibling selector so the hidden model-binding input does not break thumb positioning. Rebuilt bundles and refreshed the website asset. Inspected the full switch page; permanent browser checks now measure vertical centering, containment, and 3px end insets across all seven demo controls in both on/off states and both color modes, including small, disabled, model-bound, and customized controls. All focused checks and Git whitespace checks passed.

## Nested button-group spacing follow-up

Ledger now adds an 8px gap when a button group contains nested groups, matching the upstream grouping behavior. The outer grouping container has no shadow; each joined subgroup retains its own surface. The With Select demo keeps select/input joined while separating the arrow button. Rebuilt bundles and refreshed the website Ledger asset. Browser checks pass in light/dark modes, including a permanent assertion for an 8px inter-group gap and zero inner gap.

### Avatar border follow-up

Rounded Ledger's avatar `::after` border to inherit the avatar shape; the shared border previously remained square around circular images, both standalone and grouped. Rebuilt all theme bundles and refreshed the website Ledger asset. Focused browser verification now asserts circular border geometry for all four intro avatars in light and dark modes; all checks passed, and the light-mode screenshot was visually inspected.

### Table layout follow-up

Restored Ledger table and container full-width sizing, relative positioning, horizontal overflow scrolling, and bottom captions. Removed the automatic container border/shadow so the explicit Border demo wrapper supplies a single frame, and removed the footer's redundant last-row border. Ledger retains its card surface, muted header/footer, typography, and cell spacing. Rebuilt all nine bundles, refreshed the website Ledger asset, and visually inspected Intro and Border screenshots. Focused light/dark browser checks passed, including new assertions for table/container width agreement, caption placement, no duplicate container border, and scrolling within a 280px container. Changes remain uncommitted in the OSS repository (theme and verification), website (generated Ledger CSS), and workspace (this record); existing edits were preserved.

### Bordered table corner follow-up

Removed the independent Ledger table-container radius and added overflow clipping to the Border demo's rounded outer wrapper. The wrapper now controls the border and surface corners together, eliminating the pale gap caused by a 6px inner radius inside a 4px outer radius. Rebuilt CSS and DocsSamples; focused browser checks passed in light/dark modes, including assertions that the container has no independent radius and the border wrapper clips its contents. Inspected the corrected screenshot. Regenerated website output and retained the updated Border demo body, snippet, and Ledger bundle; preserved the pre-generation document heads and all unrelated demo exports because regeneration also picked up the current sample layout's Ledger default. OSS, website, and workspace changes remain uncommitted; existing edits were preserved.

### Small alert dialog media alignment follow-up

Restored centered grid layout for Ledger's small alert dialog header, with 6px between title and description and an additional 8px below the media icon. The website Ledger bundle was refreshed. Focused browser verification now checks centered media/text and rendered gaps at 1280px and 390px in light and dark modes; the full focused suite passed and the corrected dialog screenshot was reviewed. CSS bundles rebuilt successfully and whitespace checks passed. Changes remain uncommitted in the OSS, website, and workspace repos on `feat/ledger-theme`, preserving the existing theme work.

### Normal alert dialog header spacing follow-up

Moved the header grid layout to the base Ledger alert dialog header so its 6px title/description gap applies to normal dialogs as well as small ones. Small dialogs retain their centered alignment. Refreshed the website CSS asset and added normal-dialog rendered-gap checks at desktop/mobile widths in both color modes. The full focused browser suite and CSS build passed; the normal dialog screenshot was reviewed. OSS, website, and workspace changes remain uncommitted with existing edits preserved.

### Dropdown checked-item background follow-up

Removed the checked state from Ledger's dropdown highlight selector so checkbox and radio selections use their check indicators without a persistent background. Hover, focus, and open submenu highlighting remain. Added focused browser coverage for both demos in light and dark modes, including indicator visibility and focus feedback; enabled browser focus emulation so focus styles are exercised reliably. Rebuilt the theme bundles and refreshed the website's Ledger asset.

Verification passed: complete Ledger focused suite in light and dark modes, visual inspection of both dropdown demos, and whitespace checks in the workspace, OSS, and website repos. Existing uncommitted changes were preserved; the temporary server on port 5206 was stopped.

### Sheet overflow follow-up

Restored Ledger's fixed sheet positioning and placed its close button absolutely at the top right. The previously in-flow close button added its height above the full-height body, causing unnecessary scrolling and pushing the footer below the panel. Added browser checks for all four sides and the form footer at desktop/mobile widths in both modes, including requested heights, viewport bounds, close-button insets, and absence of overflow. The CSS build and complete focused suite passed; right and top screenshots were reviewed and the website Ledger asset refreshed. Existing edits remain preserved and uncommitted; the temporary port 5206 server was stopped.

### Accordion spacing follow-up

Reduced Ledger accordion trigger vertical padding and content bottom padding from 14px to 10px, matching Nova's compact spacing. Included the trigger's transparent border in the content inset so heading and body text align exactly. Added rendered text-gap, alignment, and bottom-spacing checks at desktop/mobile widths in both color modes. The CSS build, full focused browser suite, screenshot review, and whitespace checks passed. Refreshed the website Ledger CSS asset, preserved existing uncommitted edits, and stopped the temporary server on port 5206.

### Accordion visible text spacing correction

The previous padding match still left a 15.92px visible text gap in Ledger versus 13px in Nova because their font metrics differ. Reduced Ledger trigger bottom padding to 7px, producing a 12.92px gap while retaining 10px top padding. Expanded the regression to all three simultaneously open items, including wrapped text, at desktop/mobile widths in both modes, and tightened the permitted visible gap to 14px. The complete focused suite and CSS build passed; compared rendered Ledger/Nova measurements and reviewed the updated screenshot. Refreshed the website stylesheet, preserved existing changes, and stopped the temporary port 5206 server.

### Sidebar inset and floating header correction

Restored the missing Ledger desktop inset treatment: 8px outer gutters, the theme radius and card shadow, clipped header corners, and the collapsed-state left gutter. Mobile remains edge-to-edge. For the floating sidebar, applied matching lower corner rounding and a subtle card shadow to its direct main-content app header, retaining the contrasting header background. This is the initial suggested treatment; the user can still choose a frame around the entire main content instead.

Added focused browser checks for inset expanded/collapsed geometry and floating header corners, shadow, and background contrast at desktop/mobile widths in light/dark modes. Rebuilt all nine CSS bundles and refreshed the website Ledger asset. Inspected desktop screenshots for both variants. Existing changes in the workspace, OSS, and website repos were preserved.

### Self-contained design specification

Removed obsolete references to the deleted design export and dedicated sample pages. Promoted the reviewed component corrections into the maintained Ledger specification, documented token roles, spacing/typography, source authority, shared-layer constraints, and a concrete new-component implementation/coverage/verification workflow. Exact palette and shadow values remain canonical in the maintained CSS to avoid divergent copies. Updated the OSS guide to use normal component pages with the theme query parameter. This follow-up changes workspace documentation only; no component code or generated assets changed. Checked local specification links, obsolete references, and Git whitespace; runtime checks were not rerun for documentation changes.

### Reusable custom-theme workflow and skill

Added `docs/design/custom-theme-workflow.md` with Claude Design exploration/export prompts, a durable specification contract, staged implementation, shared-CSS explanation and compatibility requirements, a visual review matrix, and cleanup/extension guidance. Added the thin `create-custom-theme` workspace skill, its Claude discovery symlink, and links from AGENTS.md and the Ledger specification. Existing authorization and accepted design decisions carry forward; the workflow does not require repeated approvals or expand task scope. Validation covers skill metadata, local links, symlink resolution, and whitespace. A fresh agent-session discovery test has not been run. Product repositories were not changed by this addition.
