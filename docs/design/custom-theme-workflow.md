# Creating and maintaining a custom theme

Use this workflow for a StellarAdmin theme with its own visual identity, independent of shadcn's generated themes. Start at the stage relevant to the current request: exploration, handoff review, implementation, or extension. A request for a design prompt does not authorize implementation, and an existing theme correction does not require restarting exploration.

The [create-custom-theme skill](../../.agents/skills/create-custom-theme/SKILL.md) routes agents here. Theme-specific decisions belong in `docs/design/themes/<name>.md`; [Ledger](themes/ledger.md) is the first worked example, not a palette or density requirement for future themes. Follow the [development guide](../development.md) and affected [repo guides](../repos/) for commands and environment details.

## 1. Explore the identity in Claude Design

Jerrie explores the direction and chooses the visual identity. Start with a short brief describing the audience, typical tasks, desired density, and distinguishing qualities. Use a representative business page plus a component showcase: a dashboard alone can conceal inconsistent control states and composite spacing. Explore a few meaningful alternatives, then select one direction before producing the handoff.

Copy and adapt this prompt:

```text
Design a custom theme named [NAME] for StellarAdmin, an ASP.NET Core business UI component library. This is an independent visual identity, not a reskin inherited from a shadcn theme.

Audience and tasks: [WHO USES IT AND WHAT THEY DO]
Visual qualities: [DESIRED CHARACTER, DENSITY, AND REFERENCES]
Constraints: [BRAND COLOURS, FONTS, ACCESSIBILITY OR DEVICE NEEDS]

Explore the visual direction using a realistic business page and a component showcase. Include light and dark modes and desktop and narrow-screen layouts. Define semantic colour roles, typography, spacing, corner radii, borders, elevation, and motion. Explain the visual hierarchy and your deliberate tradeoffs.

Show representative buttons, fields, input groups with text/icon/button addons, checkbox/radio/switch controls, tabs, a table, a menu with checked items, and a dialog. Include an example with longer text. Show rest, hover, keyboard focus, pressed, selected, disabled, and invalid states where they make sense. Distinguish transient interaction feedback from persistent selection.

For StellarAdmin dropdown checkbox and radio items, use the check/dot indicator for selection without a persistent coloured row background; retain hover and keyboard-focus feedback. Show default tabs beside a matching segmented control, plus line tabs on their own and inside a page header. Make inner borders, selected surfaces, and label-to-underline spacing easy to compare.

After the foundation is selected, demonstrate the same rules in an accordion, a small dialog with media, a sheet with footer actions, and sidebar inset/floating layouts. Preserve component semantics: a theme must not depend on replacing controls with noninteractive imitations or changing their public APIs. Clearly label any behavior that the prototype only simulates.

Treat page-specific layout choices as examples, not global component requirements. List anything not designed explicitly and suggest which existing primitives it should compose from. Do not invent a separate visual language for every component.
```

Do not demand every library component before implementation can start. The goal is a coherent foundation plus enough complex examples to reveal how the design composes. Missing designs can be inferred once the direction is agreed.

## 2. Export a reviewable handoff

After choosing the direction, ask Claude Design for this export:

```text
Export a self-contained design handoff for the selected direction:

- README.md: intent, chosen direction, deliberate decisions, unresolved questions, and how to run the examples.
- theme.css: semantic light/dark tokens and readable component styles, including state rules and any theme-private tokens. Avoid unexplained magic values.
- showcase.html: representative primitives, composite controls, and labeled interaction states.
- page-desktop.html and page-mobile.html: realistic composition examples, with viewport dimensions documented.
- screenshots/: illustrative light/dark captures with actual viewport, device scale factor, and state recorded. Ensure file extensions match their encoding.
- assets/ if needed: required local assets, their provenance, and usage terms; otherwise document external font names, weights, and loading URLs.

Include exact fonts, sizes, weights, line heights, spacing, radii, borders, shadows, and motion durations. Explain differences between light and dark mode. Identify simulated interactions, external dependencies, and component states not covered. Keep source HTML/CSS readable; screenshots alone are insufficient.
```

These filenames are a suggested export shape, not an ingestion requirement. The agent should inspect what was actually supplied and work with equivalent files. Prefer runnable source over screenshot-derived guesses. Report missing information only when it materially blocks extraction; do not require a second export merely to match these names.

## 3. Extract a durable specification before broad implementation

The agent inventories the handoff, opens its visual examples, and reads its CSS. Resolve contradictions by distinguishing deliberate design decisions from accidental prototype behavior; document consequential inferences. If a key decision remains unresolved, present a concrete comparison for review while continuing independent work. Do not treat a screenshot as proof of interactive behavior.

Create `docs/design/themes/<name>.md` with the following content:

| Section | Information to preserve |
| --- | --- |
| Authority and intent | What makes this theme distinctive, approved direction, source precedence, and the maintained CSS path. |
| Foundations | Semantic palette roles, light/dark strategy, font loading, typography, spacing, radii, borders, elevation, motion, and density. |
| State rules | Rest/hover/focus/pressed versus checked/selected/open, disabled/busy, validation, and meaningful combinations. |
| Composition recipes | How fields, groups, menus, overlays, tables, and navigation combine primitives; which element owns borders, padding, focus, and clipping. |
| Inference guidance | Nearest-primitives mapping, intentional exceptions, what belongs to page layout, and what must preserve the existing component contract. |
| Extension and verification | Where to add rules, how to record coverage, representative sample routes, and checks for future components. |

Preserve exact source values in the maintained theme CSS; link to them from the specification rather than maintaining competing palette/shadow copies. Until the CSS exists, retain those exact values in the extraction record. Do not remove the export while any necessary information exists only there. The final authority is the maintained specification plus CSS and real component examples, not temporary exports, chat history, or sandbox screenshots.

Inspect Git status separately in each affected repo, preserve existing edits, and establish the feature branches or worktrees appropriate to the user's authorization before implementation. Record cross-repo scope and verification in an indexed product plan when the task spans sessions. Only involve the separate website repository when the authorized task needs website exports or documentation.

## 4. Implement and review the foundation

Implement a small representative set against real StellarAdmin markup: raised/flat actions, standalone and grouped fields, selection controls, a surface, and an overlay usually expose the main decisions. Prefer existing DocsSamples pages with `?theme=<name>&mode=light|dark` once theme selection is wired. Do not create a dedicated gallery by default. A temporary prototype is useful only when it answers an unresolved design question.

Review the rendered foundation with Jerrie before broad expansion when the direction has not yet been accepted. Present concrete screenshots or runnable examples and identify the decisions being reviewed. Reuse approval already given in the session; do not ask again for every component or routine inference. Keep changes reviewable and explain discrepancies between the external design and the actual library rendering.

Include related components in the same review when their visual contract is shared: default tabs and segmented controls, standalone line tabs and page-header navigation, and checkbox and radio menu items. Surface consequential choices such as joined dividers versus individually bordered selected items early. Concourse's accepted Ledger-like tab treatment is a theme preference, not a mandatory appearance for future themes. If the user requests reviewing one component before adapting its partner, preserve that order, record the temporary mismatch, and run the existing appearance comparison after the approved port; do not weaken the comparison to make the intermediate design pass.

### Keep visual identity separate from structure

Read the component's Tag Helpers, shared CSS, web component, slots, and state attributes before writing theme rules. Preserve semantics, keyboard operation, sizes, variants, orientations, and responsive behavior. Upstream themes are useful references for these contracts; their colours and visual styling are not the custom theme's authority.

The `shadcn.` prefix is reserved for upstream-derived themes; independent theme names must not use it. Theme rules belong in `Client/css/themes/<name>.css`, under `@layer components.theme`. Never add a hand-authored theme to ThemeGenerator's upstream source list or import an upstream theme to conceal missing coverage. Follow the OSS guide for bundle registration and the coverage manifest. Consumers continue to select a single stylesheet, with no theme-specific Tag Helper API.

Rules directly in `@layer components` outrank the nested theme layer regardless of selector specificity. Before changing `components.css`, explain the specific blocked customization, why a theme-local rule cannot handle it, the proposed shared change, and how existing themes will be checked. This explanation is required; a new permission checkpoint is not required when the existing task authorization covers the work. Prefer a narrow variable hook whose fallback preserves the existing value. Keep the custom value in the theme file and compare other themes after the change. Do not move theme-specific styling into shared defaults or escalate specificity blindly.

## 5. Expand by component family and verify

Apply approved primitives to related families, inspecting real examples as each family is completed. Record consequential accepted decisions in the theme specification immediately, rather than leaving them solely in a chronological fix log. Do not encode a one-off page layout as a universal theme rule.

Recurring menu-selection correction (Ledger and Concourse): dropdown checkbox and radio items indicate checked state with a checkmark or dot, without a persistent coloured row background. Preserve transient hover/focus feedback and visible keyboard focus on both checked and unchecked rows. Do not infer menu-row fills from tabs, toggles, segmented controls, or sidebar selection. Apply this convention to future themes unless the user explicitly requests a different treatment; inspect both checkbox and radio examples with focus moved away from the checked row.

Observatory review established a preference for trailing checkmarks in both dropdown checkbox and radio rows; Ice was aligned as well. Use that as the starting point for new theme designs unless the user selects another treatment. Keep standalone radio controls distinct from menu radio items. Reuse the indicator SVG already emitted by the Tag Helper, reserve space on the correct side, and verify unchecked indicators remain hidden. This preference supersedes Observatory's source design of leading checks and radio dots.

Meridian review also aligned Ice and Observatory horizontal line tabs: the list itself has no full-width bottom border; only the active tab has an accent underline. Use this as the starting preference for new custom themes, including standalone and page-header composition. Preserve established themes outside the requested scope and keep vertical orientation decisions explicit.

When translating a handoff, inspect the final cascade as well as the opening tokens. A late root alias can overwrite a dark-mode token, as Meridian's input surface did. Verify actual input and card colours in both modes. Check font roles on elements that use them, loading those faces explicitly for a dedicated font test; an unused font remaining unloaded is not evidence of a broken font setup. Do not carry a platform-specific workaround from another theme without reproducing its need.

Use this review matrix to catch the kinds of defects encountered while building Ledger and Concourse:

| Area | What to inspect in the browser |
| --- | --- |
| States | Keyboard focus, pressed + focused, checked versus hovered, invalid + focused, disabled and model-bound controls; visible indicators and reduced motion. |
| Composite controls | Equal text edge insets, doubled addon padding, read-only inner surfaces, nested groups, joined end corners, and both orientations. |
| Text layout | Actual visible title/body gaps, wrapped labels and descriptions, helper text, font loading, and alignment including transparent borders. |
| Surfaces | One owner for outer border/radius/clipping, circular pseudo-element borders, table width/caption placement, and nested shadows. |
| Overlays | Small/normal dialogs with and without media; sheets on all four sides; close-button placement; viewport containment; reachable footer actions; legitimate long-content scrolling. |
| Navigation | Expanded/collapsed, inset/floating, narrow-screen behavior, header integration, and scrolling with native scrollbars visible to the capture tool. |

For overflow review, launch `util/visual-regression/browser.mjs` with `{ hideScrollbars: false }`; its default hides the progress/header scrollbar defects found during Concourse review. Measure the affected container's `scrollHeight/clientHeight` and `scrollWidth/clientWidth`, not only document overflow. Keep intended scrolling usable instead of hiding overflow to pass a screenshot. In particular:

- Check bordered progress tracks at 0%, partial, and 100%: an inline inherited height can include the outer border and exceed the track's interior.
- Check page-header tabs with ordinary and long labels: positioned underlines can extend beyond the navigation box even when all text fits. Preserve horizontal scrolling for genuinely long navigation.
- Inspect the outside edge of inset selection rules at higher capture scale: a transparent border can expose the background as a sliver outside the rule. Compare both border geometry and paint with the handoff.

- When the approved design hides sidebar scrollbar chrome, retain native overflow and verify wheel/touch scrolling and automatic scrolling to off-screen keyboard focus. Hiding the scrollbar is a theme preference, not permission to disable scrolling.

### Section borders and semantic border roles

Aurora, Meridian and Observatory exposed doubled card dividers when a header immediately preceded a footer: each section contributed a 1px border, producing a 2px join. Compare header/footer, header/content/footer, headerless, small and image-card compositions. Assign one owner to each touching divider in the theme; do not change valid demo markup to mask a composition defect. The maintained [card-divider regression](../../util/visual-regression/verify-card-dividers.mjs) checks the affected themes in both modes and widths. Preserve consumer utility overrides and keep unrelated themes outside the requested scope.

Aurora's optional Empty border initially used its stronger control-border token, making a passive surface much darker than a bordered table. The accepted correction uses the ordinary `--border` token. Choose border roles by purpose: passive surfaces and separators can be quieter than interactive controls. Compare real related samples, including color, width and style, in light and dark modes. This is Aurora's accepted hierarchy, not a mandate to replace another theme's deliberate border treatment. Update assertions that encoded the superseded appearance, and still verify that removing an optional border utility restores the borderless default.

### Grouped control corners and border continuity

Parallax review exposed square standalone/spaced toggles while joined end items were rounded. Audit the base control radius together with group overrides; correct end-cap rules alone do not establish a consistent control family. Compare standalone controls, spaced groups, joined horizontal and vertical groups, and a single-item group. Joined interiors should keep their intended seams while exposed corners follow the theme’s owner radius. Preserve deliberate consumer class/utility overrides rather than treating their demo as the theme default.

Compare selected and unselected controls side by side. Parallax’s selected toggles incorrectly switched from a strong control border to the quieter surface-border token, and button-group text addons used the same faint token beside strongly bordered buttons/inputs. Choose a border role for the composed edge even when one constituent is static text. In Parallax, selected toggles and button-group text addons now use the strong control border; selection retains its tint. This is the accepted Parallax recipe, not an instruction to restyle other themes automatically.

Keep focus ownership explicit: a button group can contain individually interactive controls, whereas an input group owns a combined field border and ring. Matching resting borders does not imply coloring every button-group member when one child receives focus. Verify selected-plus-focused and invalid states after changing resting border rules.

Wait for transitions to finish before comparing computed colors; page load and font readiness alone do not guarantee settled styles. Parallax’s immediate post-navigation checks sampled intermediate border colors. Use keyboard input to establish `:focus-visible` before checking its ring; programmatic focus alone may not activate it. Compare the exact reported sample/variant and inspect a capture alongside computed assertions.

### Utility composition and native popup rendering

Test optional border utilities in addition to borderless defaults. Observatory's Empty example exposed `border: 0` resetting border color to `currentColor`; a later `border` utility restored the width/style and revealed a dark text-colored edge. Use `border-width: 0` when only width should be suppressed, preserving the semantic border color. Load the real sample stylesheet as well as the library stylesheet in isolated checks, since utilities may be supplied by the consuming app. Compare the painted border to the theme token in both modes, and verify the default remains borderless.

An open native select popup can be a separate browser/desktop surface. Its painted border is not established by the closed select's computed style, and it may be absent from a CDP page screenshot. When a user reports a defect, distinguish browser zoom, CSS zoom, emulated device scale, and actual display scaling. Reproduce with the reported fonts, browser and desktop scale; compare a working theme in the same environment and test more than one popup screen position. Use an isolated headed browser and desktop capture where necessary without changing the user's display settings.

Observatory and Concourse required an empirical native-select font-size workaround at 1.6× Wayland display scaling. Headless captures did not reproduce the missing bottom edge; the exact browser rounding cause remains unproven. Do not generalize those font sizes to other themes or promise correctness at untested scales. See `util/visual-regression/verify-native-select-popup.mjs` and its README for the desktop-specific check; record automated light-mode edge checks separately from manually inspected dark captures.

Build CSS explicitly and confirm nonempty output. Coverage checks detect inventory omissions, not visual correctness. Test rendered examples at desktop/mobile widths in both modes, and use the existing Chromium/CDP tools for focused checks and image comparisons. Capture base and candidate in the same environment with fonts loaded and fixed viewport/state; inspect the screenshots as well as computed styles. Do not run broad repeated checks once relevant checks pass unless a new change or unresolved concern warrants them. Add meaningful permanent regression checks for visual defects found during review; simple aesthetic tuning does not need a test that merely mirrors its CSS declaration.

Aurora also exposed two combined-state issues worth checking when extending themes. Bold-menu checked indicators and focus rails need ink that contrasts with the filled primary row; an accent indicator can disappear on an accent background even when neutral menus pass. A custom focus ring using a CSS variable required Tailwind's explicit `ring-[length:var(...)]` hint: without the hint, it compiled as a color rather than a width. Verify the rendered selected-and-focused control, and compare unaffected themes when introducing shared fallback hooks. Do not copy Aurora's 2px ring into other themes by default.

Base assertions on the intended behavior, not whatever the first implementation renders. Concourse initially had a check that preserved the unwanted selected menu fill; the corrected check compares checked/unchecked resting and hover states and verifies keyboard focus. When correcting a defect, reproduce it with the relevant check before the fix where practical. When changing an approved design, update its specification and any now-obsolete expectations together. Existing examples are `util/visual-regression/verify-concourse-progress.mjs`, the menu/header cases in `verify-concourse.mjs`, and `util/segmented-control/check.mjs` (paths relative to the product repository root). Reuse or adapt those checks rather than treating their theme-specific values as universal design requirements.

Update `util/theme-coverage/coverage.json` after inspecting actual support, including shared-only rationale where appropriate. Run the coverage checks and build commands documented in the OSS guide. For a new component, follow the porting workflow for docs and consumer references as authorized. For a CSS-only correction, rebuild and refresh the affected exported stylesheet without needlessly regenerating every HTML demo. Preserve pre-existing generated edits when a full export is necessary.

When a full DocsSamples page differs from a standalone component demo, compare the actual variants and trace background ownership through the sample layout and example tag helpers. Explicit utilities such as `bg-background` can cover a themed card surface while leaving parent padding exposed as a contrasting strip. Preserve the intended variant and remove unintended sample-wrapper overrides; do not change the component variant to hide the mismatch. Check headerless layouts as well as layouts with headers, and allow transparent wrappers in background regression assertions.

## 6. Close out and maintain

Before declaring the theme complete, check supported families/variants, both modes, font setup, required bundle registration, relevant consumer documentation, and any authorized website/sample integration. Report checks actually run and material gaps; do not equate inventory coverage with exhaustive visual or accessibility verification. Record Git status per affected repo and stop only your own verification processes. Follow the repository agreement for commits, publishing, and ports.

Before deleting temporary artifacts, ensure the specification contains all reusable decisions, the maintained CSS contains exact values, and real examples demonstrate the implementation. Follow the user's requested deletion/confirmation scope, then search for stale references and update live documentation. Historical plans may record that artifacts were removed but must not direct future work to them.

When a component is added later, read the theme specification, map its parts to existing recipes, implement supported variants/states, update coverage, and verify the real component. A new handoff is needed only for an intentionally new visual direction or a consequential decision that existing rules cannot resolve. AI inference is not deterministic; saved source, dependencies, decisions, and repeatable browser checks provide reproducibility.
