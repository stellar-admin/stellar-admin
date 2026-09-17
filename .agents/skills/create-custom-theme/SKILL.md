---
name: create-custom-theme
description: Create an independent StellarAdmin theme from design exploration or a Claude Design handoff, or extend an existing custom theme using its maintained specification. Use for custom-theme planning, handoff extraction, implementation, and coverage; not for regenerating upstream shadcn themes.
---

# Create a custom StellarAdmin theme

Read the [custom-theme workflow](../../../docs/design/custom-theme-workflow.md) and start at the stage matching the user's request. It contains the Claude Design prompts, export contract, specification structure, implementation stages, and review matrix. Do not duplicate that guidance here or run later stages beyond the authorized scope.

For an existing theme, read `docs/design/themes/<name>.md` and its linked maintained CSS first. Infer new component treatment from those recipes; do not require a fresh design export or repeat an accepted foundation review. Keep consequential decisions in that specification so future work does not depend on conversation history.

Before implementation, read the relevant repository guides and inspect status in each affected independent repository. Preserve the component's real DOM, states, keyboard operation, and structural behavior. Keep visual values in the custom theme. Explain any proposed shared-CSS change and its compatibility checks before applying it, without inventing a new approval requirement for already authorized work.

Use real DocsSamples with the theme query parameter for review. Verify composite layouts, state combinations, and light/dark desktop/mobile rendering using the workflow's matrix. Maintain explicit coverage; a passing inventory check is not evidence of visual fidelity. Keep exports and documentation aligned with the authorized scope and preserve existing edits.

Use native-scrollbar-visible captures for overflow review; the browser helper hides scrollbars by default. Follow the workflow's recurring-defect checks and related-component comparisons. Record rendering fixes separately from aesthetic preferences, and revise existing assertions when the user changes the intended design rather than preserving tests of a rejected appearance.

For surface changes, follow the workflow's section-border and semantic-role checks: compare touching and separated card sections, and optional Empty borders against related surfaces. For state changes, inspect filled-menu indicators and selected-plus-focused controls in the compiled bundle. Keep theme-specific choices in the maintained specification and add future-control mappings from its accepted primitives.

For grouped controls, follow the workflow’s corner and border continuity checks: compare standalone, spaced and joined items, both orientations, and single-item groups. Compare selected and resting borders beside related controls, and distinguish individual button-group focus from input-group focus ownership. Allow transitions to settle before comparing computed colors; exercise keyboard navigation when checking `:focus-visible`.

Check utility composition as well as default appearance: border shorthands can erase a semantic color that a later width utility needs. Review checkbox and radio menu indicators together, including placement and unchecked visibility. For native browser popups, reproduce the reported browser, real display scale, fonts and screen position; computed styles and headless captures cannot disprove a desktop paint defect. Keep empirical workarounds and their verified limits explicit in the theme specification.

If the task also adds an upstream component, use the [porting skill](../port-shadcn-component/SKILL.md) for its implementation and publication-artifact workflow. Use the [prototype skill](../prototype-component/SKILL.md) only for a genuinely new component design requiring visual exploration; theme creation alone does not require a new component API or sandbox.

Before removing a temporary handoff, preserve its reusable decisions and exact values in maintained sources. After authorized cleanup, remove stale live references. Report actual validation and remaining uncertainties; do not claim deterministic design inference or authorize commits/publishing through this skill.
