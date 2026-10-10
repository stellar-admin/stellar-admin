# Theme token migration

Status: proposed. Branch `token-spec-concept`. Builds on the [token spec concept](token-spec-concept.md), whose prototype is in `sandbox/html/token-spec/`.

## Goal

Replace the fifteen generated theme bundles with one stylesheet driven by tokens. A theme is a short file of knob values, or nothing for the default. The tag helpers' CSS drops Tailwind. Developers get a theme builder and a consumer skill that writes a theme from a description.

Decisions already made:

- The current themes do not need to be reproduced. A small set of curated presets replaces them, and the shipped look changes (pre-1.0, breaking).
- Tailwind leaves the tag helpers' CSS entirely. The Dashboard keeps its Tailwind build for now, reading the tokens through an adapter; removing Tailwind from the Dashboard is later work.
- Supported browsers are stated: relative colour syntax and `color-mix()` set the floor (about Chrome 119, Safari 18, Firefox 128).
- The samples and docs keep a theme picker, over the curated presets.

Out of scope: removing Tailwind from the Dashboard views, and new components.

## End state

TagHelpers ships:

- `stellar-admin.css`: the one bundle. Layer order (`sa.reset, sa.tokens, sa.components, sa.theme, sa.overrides`), a small reset in place of preflight, the foundation (knobs and formulas, with the default theme's values), every component's tier 2 tokens and rules, and structure that no theme touches (anchor positioning, popovers, toast stacking, enter/exit keyframes in place of `tw-animate-css`, a few hand-written utilities such as `sr-only` if the tag helpers still need them).
- `presets/<name>.css`: about 20 lines of knobs each, in `sa.theme`. The default needs none.
- `stellar-admin.tailwind.css`: an optional `@theme inline` adapter. `--spacing: var(--sa-unit)`, `--text-*`, `--radius-*` and `--color-*` mapped onto the tokens, so Tailwind utilities follow density, type scale, shape and colour, including inside scoped `data-density` sections. Used by the Dashboard and by consumer apps that write Tailwind.

Source layout under `src/StellarAdmin.TagHelpers/Client/css/`: `reset.css`, `tokens.css`, `structure.css`, `components/<component>.css` (one per component), `presets/*.css`, `tailwind-adapter.css`. A plain build (Lightning CSS: concatenate, minify, no Tailwind) produces `wwwroot/stellar-admin.css` and the preset files. Native nesting stays as written.

Removed: `themes/*.css`, `theme.css`, `theme-tokens.css`, `shadcn-tailwind.css`, `base.css`, `scripts/build-theme-bundles.mjs`, the per-theme `wwwroot/stellar-admin.<theme>.css` bundles, `util/ThemeGenerator`, the per-theme `util/visual-regression/verify-*.mjs` scripts, `util/theme-coverage` in its current form, `docs/design/themes/*.md`, and the `@source inline()` forced-utility list. Tailwind and `tw-animate-css` leave TagHelpers' `package.json`.

## Public API

Proposed, for review before phase 1 ends:

- Knobs (tier 0) and foundation tokens (tier 1) are public and documented: the names in the concept's `tokens.css`, minus anything internal (`--_*`).
- Component tokens (tier 2) follow `--sa-<component>-<part>-<property>-<state>`. Public, but documented per component as "advanced"; a token not in the docs is internal and may change.
- Dashboard: `DashboardTheme` (fifteen members) is replaced. Proposed shape:

  ```csharp
  options.Theme(theme =>
  {
      theme.Preset = DashboardThemePreset.Default;   // or a curated preset
      theme.Stylesheet = "~/css/my-theme.css";        // optional custom knob file, loaded after the preset
      theme.IncludeSuggestedFonts = true;             // unchanged
  });
  ```

  `DashboardThemePreset` is a new enum: `Default` first, then the curated presets. The layout links `stellar-admin.css`, then the preset file, then `Stylesheet`.
- TagHelpers-only consumers link `stellar-admin.css` and optionally a preset or their own knob file. No API.

## Phases

Each phase ends with a checkpoint for review before the next starts.

### Phase 1: foundation and API

- Settle the public token list and the tier 2 naming grammar; rename prototype tokens that do not fit.
- Design pass on the default theme (it becomes the product's face): accent, neutral tint, font, radius, density, depth. Approve visually in the prototype before anything moves. Curated presets wait until every component is converted (phase 3), since conversion may add or change knobs; until then the eight prototype presets serve as stress tests.
- Move `tokens.css`, the reset and the structural CSS into the library source. Add the new build alongside the old one, so both bundles exist during migration.
- Add a temporary mapping file that sets the shadcn names (`--background`, `--primary`, …) and Tailwind's `--color-*` from the tokens, so unconverted components keep working on the new foundation.
- Port the verification tools into `util/theme-check/`: the contrast checker (CDP, system Chromium, as the concept's `contrast.mjs`) and the random-theme screenshot pass.

Checkpoint: default theme approved; public token list approved; DocsSamples renders on the new foundation with every component still on old CSS through the mapping.

Progress (2026-10-09):

- Token names reviewed against one grammar and renamed in the prototype (170 references): `pager` to `pagination`; `page` split into `page-container` and `page-header`; padding spelled `p`/`px`/`py`/…; selection states (`selected`, `current`, `on`) before the property and interaction states (`hover`, `active`) last, so tabs and sidebar items no longer say `active` for "selected"; every token has a property (`--sa-menu-separator-border`, `--sa-table-row-bg-hover`, `--sa-toast-icon-success-fg`, …). Knobs: `--sa-col-head-*` to `--sa-column-head-*`, `--sa-current-fill` to `--sa-current-page-fill`, `--sa-danger-fill` to `--sa-solid-destructive` (it clashed with `--sa-color-danger-fill`), `--sa-secondary-fill` to `--sa-filled-secondary`; `--sa-item-gap` to `--sa-space-item-gap`; `--sa-container-edge` removed (containers use `--sa-color-line`). Verified: computed styles of 12,114 elements on 36 pages (every preset, light and dark, specimen and resource index) match the committed prototype exactly.
- [Theme tokens](../design/tokens.md) drafted as the public reference: knobs with defaults and meanings, tier 1 by role, the tier 2 grammar and its vocabulary, and what is internal. Approved.
- Default theme: three candidates rendered (A soft, the previous base; B structured; C airy) and viewed in the builder. B chosen, with column heads, menu section labels and sidebar group labels in normal case and the section labels at 11.5px. It is now the knob defaults in `tokens.css`: accent `oklch(0.5 0.16 258)`, neutral hue 255 at 0.006, accent tint 0.15, surface depth 0.8, 6px radius, square tags, outlines, raised, elevation 0.7, a rule-only card footer, column heads on a band. The eight existing presets now set the old base values they relied on, so they look as before; the builder's defaults follow `tokens.css`. Verified: the new base matches candidate B exactly and every preset is unchanged (computed styles of 12,114 elements on 36 pages), the builder opens on the new defaults with an empty export, and the contrast check passes for the base and every preset in light and dark (36 pages). Chips inside a field barely showed in dark mode (sunken surface 0.29 on a field of about 0.26); in dark mode they now use a 14% ink mix over the surface (about 0.34), checked on the base, Nova, Ledger and Soft presets and in a screenshot. The contrast check did not catch this, since it measured text only, so it now also checks parts: every element with its own fill or border must separate from what is behind it by a luminance contrast of at least 1.1 (its fill or its border, whichever is stronger; a real drop shadow also counts; a fill within 1.02 of its backdrop with no border draws no edge on purpose and is skipped; the switch thumb is measured against its track). Its first run found 164 faint parts across the presets and 12 random themes, all from colours set at a fixed lightness while the surface under them changes. Fixes in `tokens.css` and `components.css`:

- the sunken surface stays 0.04 below the page (`0.96 - 0.035 × depth`; it was a fixed 0.962, so with depth the page darkened onto it and tab tracks, chips and secondary fills vanished);
- light-mode lines darken with the page (`min(fixed, page − k × strength)`), the sidebar line is a step below the sidebar, and line strength has a floor of 0.75 (the builder range follows);
- chips are an 8% ink tint over whatever they sit on (cards, highlighted rows, fields, the page), replacing the dark-mode chip override;
- soft danger tints mix a danger capped at lightness 0.58, so a light danger still tints;
- muted text darkens slightly with depth (`0.5 − 0.02 × depth`) and unselected tab text is 66% ink, so both keep 4.5:1 on the darkest bands.

The base theme's visible changes: muted text 0.52 to 0.48, the sunken surface (table head band, tab track, secondary fills) 0.962 to 0.932, chips a translucent tint, unselected tab text slightly darker. Verified with seeds 7, 11, 23 and 41: 0 text failures; faint parts down to 2 for seed 7 and about 20 per 132 pages for the others, all in extreme random themes at 1.09–1.1, plus Nova's page-size control on its footer band (1.06). Curated presets moved to phase 3, after conversion. 
- Foundation in the library: `tokens.css` is now `src/StellarAdmin.TagHelpers/Client/css/tokens.css`, and the prototype loads it from there (its copy is gone, so the two cannot drift). `reset.css` is added for the plain build; the transitional bundle keeps Tailwind's preflight instead, because DocsSamples and the Dashboard load their own Tailwind CSS first, which puts `sa.reset` above Tailwind's layers, where its element rules would beat the old components. Phase 3 must state the layer order for consumers who also use Tailwind (`@layer theme, base, sa.reset, …` or the adapter declaring it). No `structure.css` yet: the structural CSS (`anchors.css`, popovers, keyframes) is still Tailwind utilities that tag helpers emit, so it moves as components convert.
- New bundle alongside the old ones: `css/stellar-admin.css` builds `wwwroot/stellar-admin.css` (Tailwind for now): tokens, `shadcn-mapping.css`, the shared structure and the Nova theme file's component styles. `shadcn-mapping.css` (temporary) replaces `theme.css` there: it sets the shadcn variables, `--spacing`, `--text-xs`/`-sm`/`-base`, the fonts and the radius steps from the tokens, on every token scope and in `sa.tokens`, above the theme file's own values. The radius steps map onto the token roles (`-lg` control, `-xl` container, …) rather than adding to `--radius`, so pill controls do not round containers into circles. Built by `build-theme-bundles.mjs`, registered in `ClientOutput`. DocsSamples' theme picker has it as "Tokens" (`?theme=tokens`).
- Verification tools in `util/theme-check/` (README there): `contrast.mjs` (text contrast and parts' separation) and `screenshots.mjs`, for any page URLs, over the base theme, a presets folder and seeded random themes, light and dark, on `util/visual-regression/browser.mjs`. The random themes now use the renamed switch knobs; the scratchpad version still used the old names, so its random runs never varied the current-page, destructive, secondary and column-head switches.

Verified (2026-10-09): `dotnet build docs/DocsSamples` builds every bundle including `stellar-admin.css`; Tailwind keeps the relative colour syntax intact. DocsSamples renders on the new bundle in light and dark (theme showcase screenshots against Nova): the token accent, Inter, the canvas tint and every component through the mapping. On a DocsSamples primary button, a `data-density="compact"` parent shrinks it from 32px to 25.6px, a nested `.dark` recolours it and a `data-sa-scope` with its own `--sa-accent` turns it green. `contrast.mjs` on the prototype (base, 8 presets, 12 random themes, seed 7, specimen and resource index): 0 text failures of 13,104, 2 faint parts of 5,904 (Nova's tabs list on its footer band 1.06; one extreme random sidebar item 1.09), as before the port. On DocsSamples (theme showcase, Table, DataGrid, Tabs; base, 8 presets, 3 random): 5 text failures of 16,656, all the destructive badge in dark mode (4.0–4.4), and 244 faint parts of 5,328 (sidebar, kbd, data grid card, segmented control, table rows, tabs list): unconverted components read through the mapping, inputs to phase 2 batch 1. Not run: tag helper tests (no C# changed).

Checkpoint: review the default theme on the token bundle in DocsSamples, the mapping file and the checker. Reviewed: the switch was round and fields were transparent on the tinted page, both Nova's styles; batch 1 starts with Input, InputGroup and Switch.

Phase 2 progress (2026-10-09):

- How a converted component coexists with the old bundles, which stay in use until phase 3: its CSS goes in `css/components/<component>.css`, whose header lists the old selectors it replaces (`Replaces: .sa-switch, …`). The build gives `stellar-admin.css` copies of `components.css` and the Nova theme file without those rules (it parses them, drops each replaced selector from its selector list, and drops a rule once none is left), and imports every `components/*.css` file itself, in name order, so a component never relies on another's file order and out-ranks another component's part by selector instead; the fifteen old bundles are unchanged. Markup stays as it is (the `group/*`, `peer` classes and utilities the tag helpers emit), since the old bundles need it; removing it moves to phase 3, with the tag helper and test updates.
- Layer order changed to `theme, base, sa.reset, sa.tokens, sa.components, components, sa.theme, sa.overrides, utilities`. With the sa layers after Tailwind's, a converted component beat an author's utilities (DocsSamples puts `ml-auto`, `pl-0.5`, `rounded-full` on input group parts), breaking the old contract that the author's classes win. The order is declared in `tokens.css` and before the Tailwind import in the bundle entry, and DocsSamples' `site.css` declares it first, since the first stylesheet to declare layers fixes the order (phase 3: the Dashboard's build and the Tailwind adapter do the same, and the docs tell consumers). Consequences: `reset.css` now sits between preflight and the components, so the bundle includes it (body text at `--sa-text-body`, a token border colour by default); the mapping is unlayered, so it still beats the theme file's own font values in `components`; and where a converted part shares an element with an unconverted component (in-group buttons are `sa-button`, the in-group textarea is `sa-textarea`), the adjustment lives in Tailwind's `components` layer after those rules until that component converts.
- Input (`components/input.css`), InputGroup (`components/input-group.css`) and Switch (`components/switch.css`) converted. Input and the group draw a field-coloured well (white in light mode) with control lines, hover, focus, invalid and disabled states and the tactile recess; the group covers inline and block addons, text, kbd, the button sizes, the textarea and disabled groups. The switch follows the tag radius (6px corners by default, pills with `--sa-pills: 1`), with default and small sizes and a choice card's focus moving to the card. Known difference: the InputGroup "Buttons" sample rounds a group with `[--radius:9999px]`, a shadcn variable the converted group does not read (`--sa-input-radius` does); left for the docs move in phase 5.

Verified (2026-10-09): every bundle builds; the new bundle has none of the replaced rules, and the Nova bundle keeps them. DocsSamples Input, InputGroup and Switch pages in light and dark against Nova (screenshots): white fields, square-cornered switch, sample utility classes still applied. `contrast.mjs` on Input, InputGroup, Switch and the theme showcase (base, 8 presets, 3 random themes): no failures or faint parts in the converted components; 12 text failures in a sample's own teal "part classes" styling (4.44–4.5 in three presets) and faint parts only in unconverted components (sidebar, kbd, segmented control, outline button). Not run: tag helper tests (no C# changed).

Phase 2 completion (2026-10-09): every component the tag helpers render is converted, so no batch order or checkpoint was needed in between (Jerrie asked for one review at the end). The remaining files were converted in parallel by component family, each checked on its own pages, then merged and checked together.

- Converted, one file each in `css/components/`: accordion, alert, alert-dialog, app-header, attachment, avatar, badge, breadcrumb, bubble, button, button-group, card, carousel, checkbox, chip, command, data-grid, dialog, dropdown-menu, empty, field (also CheckboxGroup, RadioGroup and ChoiceGroup, which render field parts and inputs), form-row, form-section, group, input, input-group, input-otp, item, kbd, label, marker, menu (the shared `--sa-menu-*` tokens and the bold and translucent modifiers), message, message-scroller, page-container, page-header, pagination, popover, progress, questionnaire, radio, segmented-control, select, separator, sheet, sidebar, skeleton, slider, spinner, stack, switch, table, tabs, textarea, toaster, toggle, toggle-group, tooltip. Collapsible and Icon have no CSS of their own. No transitional `@layer components` blocks remain. What the token bundle still takes from the old files is the overlay gap rules (`[data-anchor-side]`), the slider value inside running text, and Nova rules for classes no tag helper emits (calendar, combobox, context menu, menubar, drawer, navigation menu, the Radix-style select, sidebar rail and actions, `sa-tabs-content`, `sa-radio-group*`); phase 3 drops them with the old files.
- Approach: Nova is the reference for structure and sizes (heights, paddings and gaps measured equal at the default density, and scaling with `data-density`); the tokens set the look. Prototyped components were ported from the prototype and completed (sizes, states and parts the tag helpers emit that the prototype left out).
- Decisions made along the way, for review:
  - Containers draw a real 1px border where Nova draws a ring, so cards, menus, popovers, dialogs and sheets are 2px larger than Nova's.
  - Corners follow the token radii: 9px containers and toasts (Nova 14–18px), 6px controls (Nova 10px), 3.5px menu items and tab triggers (Nova 8px), 7.5px overlays.
  - Titles use the heading weight, 600 (Nova 500): cards, dialogs, popovers, form sections, the questionnaire.
  - Line height: text-sm parts use `--sa-leading-ui` (20px), because the reset's 1.5 body leading made rows 1px taller than Nova's.
  - Dialogs have a scrim and no backdrop blur (the prototype's choice; a blur would need a knob or a `filter` property in the grammar). Dialogs open over 150ms, sheets over 300ms, with no exit animation (Nova's never ran either). Sheet footers follow the dialog footer knobs, so they get the divider (Nova has none).
  - Fills that sit on any backdrop use a translucent ink tint (`color-mix(ink 7–10%, transparent)`): kbd, the segmented control and tabs tracks, chips, skeletons, the progress track. `surface-sunken` straight on the canvas measures about 1.06, too faint.
  - Choice cards (and questionnaire choices) are field wells; selected is an accent tint with an accent-mixed border, and focus moves from the control to the card.
  - Field separators with text draw their rules either side of the text instead of patching a background behind it.
  - The default badge's edge is a translucent darkening rather than an accent edge, so an author's own fill keeps a matching edge. Dark destructive badges use lighter danger text than `danger-text`, because the translucent tint on a selected data grid row only kept 4.2:1.
  - Alerts have Default and Destructive only (no status variants exist in the tag helper). The destructive alert sits on an opaque danger tint.
  - Overlays set `position-try-fallbacks` themselves, since `try-flip-all` was applied inside the replaced rules.
  - Disabled tabs are dimmed (Nova never dimmed them).
  - The tooltip sets the colours of a keyboard shortcut inside it, through the key's private variables.
- Open questions for review:
  - Resolved (2026-10-10): keys stay sans by default; tokens.md now gives `--sa-font-mono` for code only, and a theme sets `--sa-kbd-font: var(--sa-font-mono)` for mono keys.
  - Resolved (2026-10-10): the section-label knobs are renamed `--sa-section-label-text`, `-weight`, `-font`, `-transform` and `-tracking`, freeing `--sa-label-*` for the Label component, which reads the foundation directly (component tokens removed, below).
  - Resolved (2026-10-10): moot, the component tokens are gone (below).
  - Resolved (2026-10-10): of the candidate foundation tokens, only the translucent ink tint was worth a public name (seven components used it); it is `--sa-color-tint` (ink at 8%), used by the tab and segmented tracks, kbd, chips, skeletons and the progress track, which had 7–10%. The others (selected-field tint, opaque danger surface, layout gap scale, inverse pair) have one or two users and stay tier 2.
- For phase 3: the slider script writes the measured thumb size inline as `--sa-slider-thumb-size`, the same name as the tier 2 token (a density change after load keeps the stale value; rename the script's variable); `empty.css` sets `--tw-border-style: dashed` for the samples' Tailwind `border` utility; the Table and DataGrid tag helpers also put `sa-table-container` on the `<table>`; the attachment shimmer only slows under reduced motion.
- Samples: the ThemeOverride sample overrides shadcn variables, which converted components no longer read (phase 5 rewrites it for knobs); the Alert "Shorthand" sample renders an empty alert in Nova as well (a pre-existing tag helper or sample bug); ComponentPlayground was not checked.

Verified (2026-10-09), after merging: every bundle builds; a parse of the filtered old files finds no replaced selector left and balanced braces; no `!important` and no literal colours in the component files beyond black shadows, mask gradients and the badge edge. Each family was checked on its own pages before merging: sizes probed against Nova at the default density and compact; light and dark screenshots against Nova; open states (menus, dialogs, sheets, toasts, popovers, tooltips) opened by script and screenshotted, and menus' open states contrast-checked; forced focus, invalid, checked, selected and disabled states. Together: screenshots of all 64 DocsSamples pages in light and dark (no broken page), the theme showcase reviewed in both modes, and the command dialog and questionnaire re-measured after the merge. `contrast.mjs` on all 64 pages (base, 8 presets, 3 random themes, light and dark; 202,200 texts, 66,182 parts): no text failure in a converted component after the dark destructive badge fix (re-run on DataGrid, Badge and Table in dark); the rest are sample content (carousel captions measured against the image fallback, the Badge sample's own green and sky badges, the teal "part classes" samples at 4.45–4.5 in two presets, a gradient Delete button the checker cannot measure). Faint parts: the docs' inset sidebar wrapper against the body behind it (1.063; it fills the page, so no visible edge), the current nav item in one extreme random dark theme (1.091, as in the prototype), the sample's own `bg-muted` empty state and soft custom badges, and the avatar group count in the nova preset (1.063). Not run: tag helper tests (no C# changed); hover beyond forced states; mobile widths beyond the sidebar drawer and dialog footers.

Component tokens removed (2026-10-10). The conversion had given the components 1,053 public tokens (the prototype's 15 had 337), and 80% of them only renamed a foundation token (603) or a multiple of `--sa-unit` (216). None of the eight presets set one: themes are knobs. So components no longer have tokens of their own. A script inlined every token declared once (1,000) at its uses, and the 42 declared more than once (dark mode or a second selector) or holding `inherit` or a list became private `--_*` variables; the six `inherit` ones were written as `inherit` directly. Kept public: `--sa-dialog-footer-fill`/`-rule` and the new optional `--sa-kbd-font` (all optional knobs, read through a fallback). `--sa-slider-thumb-size` and `--sa-table-row-detail-*` stay as script-published internals. `tokens.md` drops the component token tier and its grammar; overrides are now tier 2. The prototype in `sandbox/html/token-spec/` keeps its tokens as the historical reference. For phase 3: the Dashboard's `client.css` reads `--sa-chip-radius`, which only the old theme files declare.

Verified (2026-10-10): every bundle builds; the computed styles of all 507,568 elements (and their `::before`/`::after`) on the 64 DocsSamples pages, in the base theme, the nova, soft and terminal presets and one random theme, light and dark, are identical before and after (transitions and animations off). The component files keep balanced braces and comments, and no `--sa-*` name is left outside `tokens.css` except the kept ones above. Not run: hover and open states beyond what the pages render at rest (the inlined values are the same expressions); tag helper tests (no C# changed).

### Phase 2: convert components in batches

Batches by dependency, each a reviewable unit:

1. Already prototyped: Button, Input, InputGroup, Card, Table, Chip, Pagination, DropdownMenu, Tabs, Dialog, Sidebar, PageHeader, DataGrid, Badge, Switch, Toaster.
2. Form controls: Field, FormRow, FormSection, Label, Textarea, Select, CheckboxGroup, RadioGroup, ChoiceGroup, SegmentedControl, Toggle, ToggleGroup, Slider, InputOtp, Attachment.
3. Overlays and menus: Popover, Tooltip, Sheet, AlertDialog, Menu, Command, Questionnaire.
4. Display and layout: Accordion, Alert, Avatar, Breadcrumb, ButtonGroup, Carousel, Collapsible, Empty, Item, Kbd, Marker, Progress, Separator, Skeleton, Spinner, Layout, AppHeader, Message, MessageScroller, Bubble.

For each component: tier 2 tokens and plain CSS in `components/<component>.css`; remove its rules from the old `components.css` and the theme files; replace emitted Tailwind hooks (`group/*`, `peer`, utility classes) with component classes, `:has()` or sibling selectors, updating the tag helpers and their tests; check DocsSamples and ComponentPlayground in the default theme and every preset, light and dark. Alert and status-like components get the status colours; anything needing new knobs is raised at the batch checkpoint rather than added silently.

Checkpoint after each batch: screenshots and the contrast check across the default, the eight prototype presets and random themes; tag helper tests green.

### Phase 3: switch over and remove the old machinery

- Curated presets: choose two or three that differ clearly from the default and each other (proposed: warm tactile, dense, roomy pill), tune them in the builder against every converted component, and pass the contrast check. They ship as `presets/<name>.css` and populate `DashboardThemePreset`.
- Dashboard: replace `DashboardTheme` with the preset API above; link the new files; add the Tailwind adapter to the Dashboard's build so its utilities follow the tokens.
- Remove the mapping file, the old bundles, generators, coverage and visual-regression scripts listed under End state, and Tailwind from TagHelpers.
- Samples: DocsSamples and ComponentPlayground get a preset picker (swaps the preset stylesheet) in place of the theme bundle picker. Update `DashboardThemeTests`.
- Document the browser baseline in the getting-started docs and the package readme.

Checkpoint: no references to the old bundles; full test suite green; DocsSamples and Dashboard checked in every preset.

### Phase 4: tools

- Theme builder on the website (website repository): the prototype builder, previewing real components through the generated inline examples, exporting a knob file and a shareable URL that encodes the knobs. Starts from any preset.
- Consumer skill in `skills/`: turns a description ("warm, editorial, dense, green accent") into a knob file. Contents: the knob reference with each knob's meaning and range, the presets as worked examples, guidance on mapping adjectives to knobs, and a builder URL for the result. Contrast is guaranteed by the token formulas, so the skill does not need to check it.
- Rework the development skills: `create-custom-theme` becomes "add a curated preset" (or is retired); `port-shadcn-component` translates upstream classes into tier 2 tokens instead of copying them; `prototype-component` builds against `stellar-admin.css`.

### Phase 5: docs and references

- Website: a theming section (knobs, presets, the builder, the skill, scoped density, dark mode, the Tailwind adapter, the browser baseline), replacing the per-theme pages.
- Regenerate the consumer skill references; update the product development guide and `docs/repos/website.md` where they describe theme bundles.

## Verification

- The contrast check (WCAG text contrast for every visible text element, composited backgrounds) passes for the default theme, every preset and seeded random themes including extremes, light and dark, for DocsSamples pages, not only the prototype specimen.
- Random-theme screenshots reviewed at each batch checkpoint.
- Per converted component: tag helper tests, DocsSamples output checked in the default theme, and measured sizes against the prototype where the prototype covered it.
- Nested `.dark` scopes, `data-density` scopes and `[data-sa-scope]` panels checked once per batch.
- Not covered automatically: hover, focus and active states beyond forced pseudo-states, mobile widths. Spot-check these at checkpoints.

## Risks

- Conversion volume: about 50 components not yet prototyped. Batches keep it reviewable; the mapping file keeps the product working between batches.
- Components that resist the vocabulary (complex overlays, Carousel, InputOtp) may need new knobs; each is a checkpoint decision.
- Emitted Tailwind classes in consumer-visible markup: some consumers may style against `group/*` or utility classes; removing them is a breaking change to note in the release.
- Relative colour syntax in older browsers fails silently to unset colours; the stated baseline must be prominent.
- The website builder depends on the website's inline example pipeline (`embed-website-components`).
