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
- Design pass on the default theme (it becomes the product's face): accent, neutral tint, font, radius, density, depth. Pick two or three curated presets to ship first. Approve visually in the prototype before anything moves.
- Move `tokens.css`, the reset and the structural CSS into the library source. Add the new build alongside the old one, so both bundles exist during migration.
- Add a temporary mapping file that sets the shadcn names (`--background`, `--primary`, …) and Tailwind's `--color-*` from the tokens, so unconverted components keep working on the new foundation.
- Port the verification tools into `util/theme-check/`: the contrast checker (CDP, system Chromium, as the concept's `contrast.mjs`) and the random-theme screenshot pass.

Checkpoint: default theme and presets approved; public token list approved; DocsSamples renders on the new foundation with every component still on old CSS through the mapping.

### Phase 2: convert components in batches

Batches by dependency, each a reviewable unit:

1. Already prototyped: Button, Input, InputGroup, Card, Table, Chip, Pagination, DropdownMenu, Tabs, Dialog, Sidebar, PageHeader, DataGrid, Badge, Switch, Toaster.
2. Form controls: Field, FormRow, FormSection, Label, Textarea, Select, CheckboxGroup, RadioGroup, ChoiceGroup, SegmentedControl, Toggle, ToggleGroup, Slider, InputOtp, Attachment.
3. Overlays and menus: Popover, Tooltip, Sheet, AlertDialog, Menu, Command, Questionnaire.
4. Display and layout: Accordion, Alert, Avatar, Breadcrumb, ButtonGroup, Carousel, Collapsible, Empty, Item, Kbd, Marker, Progress, Separator, Skeleton, Spinner, Layout, AppHeader, Message, MessageScroller, Bubble.

For each component: tier 2 tokens and plain CSS in `components/<component>.css`; remove its rules from the old `components.css` and the theme files; replace emitted Tailwind hooks (`group/*`, `peer`, utility classes) with component classes, `:has()` or sibling selectors, updating the tag helpers and their tests; check DocsSamples and ComponentPlayground in the default theme and every preset, light and dark. Alert and status-like components get the status colours; anything needing new knobs is raised at the batch checkpoint rather than added silently.

Checkpoint after each batch: screenshots and the contrast check across presets and random themes; tag helper tests green.

### Phase 3: switch over and remove the old machinery

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
