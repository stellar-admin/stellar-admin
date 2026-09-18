# Concourse theme

## Code audit — 2026-09-18

Current status: **completed**. Concourse CSS and specification, segmented/tab recipes and `verify-concourse.mjs` are present.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: completed — implemented and verified locally; not committed or published. Last updated: 2026-09-11.

## Scope and authorization

Jerrie supplied `concourse_handoff/`, asked to start adding the theme, and authorized continuation after the foundation preview. The maintained design authority is the [Concourse specification](../../design/themes/concourse.md) plus [theme CSS](../../../stellar-admin/src/StellarAdmin.TagHelpers/Client/css/themes/concourse.css). Follow the [custom-theme workflow](../../design/custom-theme-workflow.md) for future extensions. No commits, pushes, or publishing were performed during implementation. Handoff deletion was subsequently authorized and completed; see the cleanup note below.

All five repositories remain on their existing branches with uncommitted changes. Initial workspace status contained only untracked `concourse_handoff/`; OSS, Pro, website, and consumer skills were clean. User handoff files were preserved during implementation. Work was performed locally without sub-agents or additional worktrees.

## Implementation

- Added an independent Concourse bundle, with the supplied palette, font families, sizing scales, focus, shadows, and colour-based action feedback. Promoted the temporary foundation source to `Client/css/themes/concourse.css` and removed the superseded agent-created preview files.
- Added rules or documented shared/composed coverage for all 55 current component families. No upstream fallback imports or new component APIs. Registered the bundle as a `ClientOutput` and a custom theme in the coverage manifest.
- Added narrow shared hooks for toggle hover background and tab indicator colour; defaults preserve the existing themes. Kept checked menu state separate from neutral transient hover/focus by correcting selector specificity.
- Wired DocsSamples and DocsSamplesPro theme selection and font links, the website demo picker, and consumer theming guidance. Concourse is selected with `?theme=concourse&mode=light|dark` or a single `stellar-admin.concourse.css` link.
- Fixed narrow-screen demo composition: constrained Item and Spinner wrappers, wrapped Attachment thumbnails, and let Concourse pagination controls wrap without hiding actions.
- Regenerated 412 website demos, theme assets, and source includes. Export changes include Concourse/font references, the sample fixes, regenerated CSS/JS fingerprints, and generator SVG attribute ordering. Reviewed those changes without discarding generated differences through normalization.
- Regenerated consumer references. Attachment reflects the wrapping fix; AlertDialog also picked up an existing source/reference discrepancy (the source already used semantic result-surface classes). The generator check now passes.
- Added `verify-concourse.mjs` for focus, persistent selection, grouped validation, native input, sheet containment/dismissal, sidebar widths, and reduced motion. Included Concourse in the existing segmented-control cross-theme check.

## Verification

- Explicit `npm run build` and `npm run build:css` passed with nonempty outputs for all ten bundles. The initial sandboxed build failed to write CSS; it was recovered with the same build outside process restrictions. No user-wide configuration or dependency versions changed.
- OSS DocsSamples build passed with 10 existing nullable-property warnings; Pro DocsSamplesPro build passed with seven existing documentation warnings. Commands ran from the workspace with SDK 10.0.400, bypassing the nested OSS 10.0.100 pin without changing it.
- Coverage check passed: 55 component families × ten themes. Coverage failure-case tests passed. Inventory coverage is not a visual or accessibility certification.
- Captured 69 real DocsSamples partials in light/dark at 1280px and 390px, 900px high, DPR 1 (276 combinations). All captures reported loaded Source Sans 3. Corrected and recaptured eight mobile overflow cases from four samples; final combined results report zero page-level horizontal overflow. Settled sheet captures avoid recording the middle of the existing 500ms entrance animation.
- Inspected the desktop overview for all 69 partials, plus focused full-size light/dark and mobile captures for controls, menus, cards, input groups, sidebars, dialogs, sheets, pagination, and corrected samples. Captures are ignored under `stellar-admin/util/visual-regression/snapshots/concourse/`.
- Primary button rest/hover/press geometry, typography, colours, shadows, and movement match the runnable handoff in the same browser in both modes. Fresh source screenshots and `source-comparison.json` accompany the captures. The supplied image files contain JPEG data despite `.png` names and unreliable viewport metadata, so they were not used as pixel baselines.
- `verify-concourse.mjs` passed in both modes at desktop/mobile widths. One sidebar font-provider request timed out; the bounded wait reported it and continued functional checks. The separate visual sweep loaded fonts for every capture. An early sheet assertion measured the entrance animation before completion; the check now waits for animation completion.
- `util/segmented-control/check.mjs` passed across all ten themes, including native keyboard navigation, disabled behavior, event counts, model binding, validation, and reset.
- Existing-theme tab/toggle computed styles were identical before/after the two shared hooks across nine themes × two modes × two components (36 combinations), captured after stable builds in the same environment.
- Website lint, type checking, and production build passed. Rebuilt production output after demo regeneration to include the new stylesheet and exports. pnpm checks required local database access outside the sandbox.
- Browser integration checks confirmed that the exported website demo and Pro DataGrid load Concourse, dark mode, and the Source Sans 3 stack. Inspected their settled captures.
- SkillsGenerator regeneration and `--check` passed; the restore emitted a vulnerability-feed connectivity warning (NU1900). No dependency changes were made.
- Final documentation links, private token references, generated asset references, and per-repo `git diff --check` were checked before handoff.

## Final repository changes and limits

Workspace: design specification, this archived implementation record, plan index, and OSS guide, plus the original user handoff (subsequently removed). OSS: new theme and focused check, coverage/registration, two shared CSS hooks, sample picker/fonts, three responsive sample fixes, and verification guidance. Pro: DocsSamplesPro layout only. Website: theming documentation, picker, 412 regenerated demo pages and related exported assets/includes. Consumer skills: theming guidance and two regenerated references.

The referenced `Concourse Design System.dc.html` was not supplied; its calendar/combobox/command-palette designs concern components not added by this theme task. Future extensions compose the maintained recipes without requiring that missing source file. No implementation work remains for the current theme scope. This was representative visual and behavioral verification, not exhaustive pixel matching, accessibility certification, or validation of every possible state combination.

Only agent-owned verification servers were used (DocsSamples 5206, Pro 5207, website 5210), and they were stopped after verification. Jerrie's port 5205 was not touched. Future component support starts from the maintained specification and CSS.

## Progress correction after review

User review exposed native scrollbar arrows on the progress track. The indicator inherited the 6px outer height while the bordered track had only 4px of content height. Added a theme-local maximum height to fit the interior, rebuilt CSS, and refreshed only the exported Concourse stylesheet. No shared CSS or markup change was needed. The earlier general screenshot sweep hid scrollbars and missed this defect.

Added `verify-concourse-progress.mjs` with native scrollbars visible. It reproduced overflow before the fix and passed afterward for all four progress examples in light/dark at 1280px/390px, checking overflow and proportional widths including 0%/100%. Inspected corrected light desktop and dark mobile captures. CSS build and theme coverage passed. Changes remain uncommitted.

## Dropdown selection correction after review

The user requested the same indicator-only dropdown selection treatment previously established for Ledger. Removed Concourse's persistent checked-row fill for checkbox and radio items; hover/focus feedback and check/dot indicators remain. Updated the maintained specification and shared custom-theme workflow so future themes do not infer menu selection backgrounds from tabs or toggles. Replaced the prior regression assertions that enforced selected fill with checkbox/radio checks for equal resting backgrounds, matching hover feedback, and visible keyboard focus.

Rebuilt CSS and refreshed the exported Concourse stylesheet. Updated Concourse browser checks passed light/dark at 1280px/390px and reduced motion; one sidebar font request timed out and the bounded functional check continued. Inspected light checkbox and dark radio captures. Coverage, formatting, and affected-repo whitespace checks passed. Changes remain uncommitted.

## Page-header navigation correction after review

The line-tab underline extended 1px beyond the page-header navigation scroll container, producing an unwanted vertical scrollbar. Added 2px bottom margin to Concourse line-tab lists only inside page-header navigation. Shared CSS and standalone tabs remain unchanged. Rebuilt and refreshed the exported Concourse bundle.

Added regression checks for ordinary header navigation without overflow and deliberately long labels with usable horizontal scrolling but no vertical overflow. The updated browser suite passed in light/dark at 1280px/390px and reduced motion; one sidebar font-provider timeout was reported while functional checks continued. Inspected desktop/mobile captures with native scrollbars visible. CSS build, coverage, formatting, and whitespace checks passed. Changes remain uncommitted.

## Tabs design revision awaiting user review

The user requested a smaller line-tab text/underline gap and Ledger-like inner borders for default tabs, explicitly deferring segmented controls until the tabs are accepted. Reduced horizontal line-tab bottom padding from 10px to 3px. Default tabs now use 2px track padding, 3px gaps, rounded inner items, and a card-coloured selected item with a border and shallow shadow. Removed tab divider/edge-only corner rules while preserving the segmented-control rules. Refreshed the exported Concourse stylesheet for review.

CSS build and formatting passed. Browser checks covered default, line, icon, vertical tabs and header navigation in light/dark at 1280px/390px with visible scrollbars; all fit their viewports and header navigation had no vertical overflow. Inspected light/dark and vertical captures. The tabs/segmented appearance-equality check is intentionally not satisfied during this requested design review; its assertion remains intact. Next step: user review of tabs, then port the accepted appearance to segmented controls. No commits or publication performed.

## Approved tabs treatment applied to segmented controls

The user approved the revised tabs and authorized matching segmented controls. Shared Concourse recipes now give both components 2px track padding, 3px gaps, rounded inner items, and a card-coloured selected surface with its own border and shallow shadow. Removed segmented divider and edge-only corner rules. Updated the maintained specification and exported Concourse stylesheet.

The existing appearance comparison passed across all ten themes in light/dark at desktop/mobile widths, restoring the Concourse tabs/segmented equality contract. Native keyboard/focus, change events, labels, disabled/independent groups, form binding, validation, and reset checks passed. Inspected Concourse segmented text/icon captures; desktop/mobile containment passed in both modes. CSS build, coverage, formatting, and whitespace checks passed. No remaining design-review dependency for this change; all work remains uncommitted.

## Handoff cleanup

Jerrie authorized removing the temporary Concourse and Ice handoff folders before committing. Preserved Concourse font-loading details in the specification, replaced handoff links and the obsolete missing-export prerequisite with maintained extension guidance, and deleted both folders. The specifications, theme CSS, real samples, and verification tools remain the sources for future component support. No product code changed in this cleanup.
