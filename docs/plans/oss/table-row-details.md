# Table row details

Status: phases 1–3 implemented; phase 3 awaiting review (2026-10-09). Dashboard integration remains a separate, unplanned piece of work. Visual exploration approved in `sandbox/html/expandable-rows.html`. Dashboard integration is out of scope and gets its own plan later.

## Goal

Let a table row expand to show arbitrary content below it. The behaviour and styling live at the `sa-table-*` level so hand-written tables can use them. `sa-data-grid` adds a convenience layer on top, the same way `sa-data-grid-selection` composes `sa-table-selection`. What goes inside the detail is always author content.

## Decisions (Jerrie, 2026-10-09)

- Concept content (fields, nested table, tabs, split) is application content. Nothing concept-specific goes into the library.
- Toggle position: leading or trailing column, or placed manually anywhere. Row click is an independent option on top.
- Emphasis: `None`, `Band` or `Rail`, selectable. This deliberately overrides the prototype skill's "one opinionated default per theme" rule.
- Inset: `Bleed` or `Aligned`. A card around the content is author markup.
- Animation can be turned off. Reduced motion is always honoured.
- Expand mode: one or many open.
- The toggle must be placeable anywhere (the Dashboard will put it beside edit/delete).
- Expand/collapse events. They must work with htmx without depending on it.
- Rows can be server-rendered as expanded.

## Table-level API

```cshtml
<sa-table-row-details row-click="true" expand-mode="TableRowDetailExpandMode.Single"
                      emphasis="TableRowDetailEmphasis.Band" inset="TableRowDetailInset.Aligned"
                      animate="true">
    <sa-table>
        <sa-table-header><sa-table-row>
            <sa-table-head><sa-table-row-detail-toggle all="true" /></sa-table-head>
            <sa-table-head>Reference</sa-table-head>
        </sa-table-row></sa-table-header>
        <sa-table-body>
            <sa-table-row>
                <sa-table-cell><sa-table-row-detail-toggle /></sa-table-cell>
                <sa-table-cell>VG-24817</sa-table-cell>
            </sa-table-row>
            <sa-table-row-detail expanded="false">...</sa-table-row-detail>
        </sa-table-body>
    </sa-table>
</sa-table-row-details>
```

- `sa-table-row-details` renders the `sel-table-row-details` web component, with `data-slot="table-row-details"` and data attributes for emphasis, inset, animate, row-click and expand-mode. Defaults: `row-click=false`, `expand-mode=Multiple`, `emphasis=Band`, `inset=Aligned`, `animate=true` (defaults to confirm against the prototype).
- `sa-table-row-detail` renders `<tr data-slot="table-row-detail">` with one `td` and the reveal wrappers (`grid-template-rows` 0fr→1fr, an `overflow-hidden min-h-0` inner box, and a pinned box sized with `100cqi` against a `container-type: inline-size` table container). Optional `colspan`. Without it the component fills it from the header cell count. `expanded` renders `data-state="open"` instead of `hidden` + `data-state="closed"`.
- `sa-table-row-detail-toggle` renders a ghost `icon-sm` button with a chevron, `data-slot="table-row-detail-toggle"`, `aria-expanded` and an `aria-label` (overridable). With `all="true"` it is the expand-all/collapse-all button and is hidden in single mode. Optional `for` targets a detail row by id. Otherwise the toggle pairs with the detail row that follows its own row. Leading toggles rotate right→down and trailing ones down→up. The direction comes from a `data-direction` derived from position, or an explicit attribute. Decide during implementation.
- Parent row state is `data-expanded` (not `data-state`, which `sel-table-selection` already uses for `selected`).

### Web component (`sel-table-row-details`)

- Event delegation on the wrapper only. No per-row listeners, no JS state. All state is in the DOM (`hidden`, `data-state`, `data-expanded`, `aria-expanded`). That means htmx swaps of rows, tbody, the detail content or the whole grid, `hx-boost` and history restore need no re-initialization.
- Assigns ids and `aria-controls` when missing. Syncs `aria-expanded` and `data-expanded` with server-rendered state on connect and when the subtree changes (MutationObserver for htmx-inserted rows).
- Row click ignores clicks inside `a, button, input, select, textarea, label, form, [role=button], [data-no-row-toggle]` and text selections.
- Single mode collapses others before expanding.
- Public methods: `expand(row)`, `collapse(row)`, `toggle(row)`, `collapseAll()` and `expandAll()`.

### Events (htmx contract)

- `row-detail-expand` and `row-detail-collapse` are bubbling, non-cancelable `CustomEvent`s dispatched on the detail `tr` after the state change (expand fires after `hidden` is removed, so the content is visible to htmx). `detail: { row, detailRow }`. Hyphenated names, no colons, so `hx-trigger` and `hx-on` need no escaping.
- Not fired for server-rendered initial state (same rule as `selection-change`).
- Documented lazy-load pattern: `<div hx-get="/bookings/24817/details" hx-trigger="row-detail-expand from:closest tr once" hx-swap="outerHTML">skeleton</div>`. Drop `once` to refresh on every expand. A server-expanded row renders its content inline (or uses `hx-trigger="load"`).
- Content height changes after a swap need no JS because the grid-rows reveal sizes to content.
- Verify with real htmx in a browser: lazy load, swapping the grid via `hx-select`, adding rows with `beforeend`, and history restore.

### CSS (`components.css`)

Chevron rotation. Reveal transition, disabled by `data-animate="false"` and `prefers-reduced-motion`. Remove the divider under an expanded parent. Emphasis band (`color-mix` muted on parent + detail) and rail (inset 2px primary shadow on the parent's first cell + detail cell). Inset aligned = toggle column width + cell padding from tokens, with no JS measuring. Row-click cursor. Run the theme generator only if a theme needs a custom rule.

## Data grid API

```cshtml
<sa-data-grid items="Model.Bookings">
    <sa-data-grid-column ... />
    <sa-data-grid-row-detail toggle="DataGridRowDetailToggle.Leading" row-click="false"
                             expand-mode="TableRowDetailExpandMode.Multiple"
                             emphasis="TableRowDetailEmphasis.Band" inset="TableRowDetailInset.Aligned"
                             animate="true"
                             expanded="@(Html.GridItem<Booking>().Id == Model.OpenId)">
        ...per-row template, Html.GridItem<Booking>() available...
    </sa-data-grid-row-detail>
</sa-data-grid>
```

- Same pattern as selection: the collect pass registers settings on `DataGridContext`, and the row pass deposits the rendered template and the evaluated `expanded` on `DataGridRowContext`.
- `toggle`: `Leading` (default), `Trailing` or `None`. Leading and trailing generate a toggle column with expand-all in its header. `None` means the author places `<sa-table-row-detail-toggle />` in a column item template (the Dashboard's actions column).
- The grid wraps the table container in `sel-table-row-details` (nested with `sel-table-selection` when both are present), computes `colspan` and includes the toggle column in the empty-state column count.
- Fix the existing edge-padding leak: `.sa-data-grid` `[&_tr>*:first-child]` also hits nested tables. Scope it to the grid's own rows.
- Reuse the table-level enums (`TableRowDetail*`). Only `DataGridRowDetailToggle` is grid-specific. Per [[dedicated-enums-per-component]] that is fine because grid row details *is* table row details.

## Phases

1. Table level: tag helpers, enums, web component, CSS, TUnit tests for output, browser check in DocsSamples (5206) including htmx scenarios. Stop for review.
2. Data grid layer and the nested padding fix, with tests and a browser check. Stop for review.
3. Docs: DocsSamples demos (Voyager Travel), website docs page, consumer skill reference regeneration. Only when asked.

## Open questions

- Should `emphasis`/`inset` defaults come from the theme rather than fixed defaults?
- Toggle chevron direction for manually placed toggles.
- What happens without JS: details stay hidden (no fallback planned).

## Phase 1 implementation notes (2026-10-09)

Changes from the design above:

- The parent row gets no `data-expanded`. The parent-row styles use `tr:has(+ .sa-table-row-detail[data-state="open"])`, so server-rendered state styles correctly before the script runs, and the toggle reflects state through `aria-expanded`.
- The toggle has no `for` attribute. An author-supplied `aria-controls` targets a detail row by id. Otherwise the component pairs the toggle with the row after its own row and assigns `aria-controls`.
- One chevron for every toggle (new `SemanticIconRole.RowDetailIndicator`, appended, `chevron-right` in all three packs). It rotates 90° when expanded, including the expand-all toggle. That settles the chevron-direction question.
- Insets and the pinned width are measured by the component (`--sa-table-row-detail-inset-start/-end`, `--sa-table-row-detail-width`), not CSS container units. Measuring follows each theme's cell padding and any edge padding a container adds, and avoids inline-size containment collapsing shrink-wrapped tables. Bleed lines up with the first cell's padding.
- `.sa-table-row-detail-clip` uses `overflow: clip`. `hidden` made it the sticky content's scroll container, so the content did not stay pinned.
- Ghost buttons fill when `aria-expanded="true"`. The toggle drops that fill (not on hover) because the rotated chevron shows the state, matching the prototype.
- htmx 4 (the version in the repo) parses `from:closest tr` as `from:closest` followed by a stray `tr`. It needs `from:<closest tr/>` or a quoted value. The version-independent pattern puts the htmx attributes on `sa-table-row-detail` itself, where the event fires: `hx-get="…" hx-trigger="row-detail-expand once" hx-target="find [data-slot=table-row-detail-content]"`. Note for docs: under htmx 2's implicit inheritance, `hx-target` on the row is inherited by htmx elements inside the details.
- Bug found and fixed in the browser check: the MutationObserver watched `hidden` across the subtree, so hiding expand-all in single mode re-triggered sync forever. It now reacts only to child-list changes and to `hidden` on detail rows, and never rewrites an unchanged attribute.

Files: `src/StellarAdmin.TagHelpers/TagHelpers/Table/TableRowDetail*.cs` (three tag helpers, three enums), `Client/js/web-components/sel-table-row-details.ts`, the `.sa-table-row-detail*` rules in `Client/css/components.css`, Core semantic icon role and pack mappings, `util/theme-coverage/coverage.json` Table tags and hooks, tests in `tests/StellarAdmin.TagHelpers.Tests/TagHelpers/Table/`, and the ComponentPlayground page `Demo/TableRowDetails` (query string sets the options).

Known limits: details stay hidden without JavaScript. A nested `sa-table-row-details` inside another inherits the outer wrapper's emphasis and row-click selectors where its own settings differ. `sel-table-selection` treats every `tbody` checkbox as a row checkbox, so checkboxes inside details would join the selection. Fix that in phase 2, where grids combine both.

## Phase 2 implementation notes (2026-10-09)

- `sa-data-grid-row-detail` takes `toggle` (`Leading`, `Trailing`, `None`), the five table-level settings, and `key-field` with `expanded-keys` instead of the planned per-row `expanded="@(Html.GridItem<T>()…)"`. The grid evaluates a child's attributes during its collect pass too, where `Html.GridItem<T>()` throws, so a per-row expression cannot work there. Keys follow `sa-data-grid-selection`'s `key-field` pattern and are compared as invariant strings. `expanded-keys` without `key-field` throws.
- The template is the element's own content, run only in row passes (like `sa-data-grid-item-template`). The grid renders the detail row with the shared `TableRowDetailRendering` helper (also used by the table tag helpers now), and the toggles through `TableRowDetailToggleTagHelper`, with the server-side `aria-expanded`.
- Order: selection, toggle, data columns, then the trailing toggle. Nesting: `sel-table-selection > sel-table-row-details > table container`. Toggle cells use an inline `width: 1px` (library bundles do not ship arbitrary utilities). Empty-state and detail `colspan` count the toggle column.
- The aligned inset now starts at the column after the toggle's column, so a toggle after the selection checkbox aligns with the first data column. A toggle in the last cell (trailing or an actions column) falls back to bleed.
- Padding leak fixed: `.sa-data-grid` pads `tr:not(td tr) > :first-child/:last-child`, excluding `.sa-table-row-detail-cell`. Rows of tables nested in cells keep their own padding, and the detail cell stays unpadded so the pinned content cannot overflow the container.
- `sel-table-selection` now ignores checkboxes inside nested selection tables and inside its own detail rows (both for row checkboxes and select-all).
- The ComponentPlayground demo files disappeared before the phase 1 commit (only the navigation entry was committed). Jerrie confirmed restoring them, so they were recreated with two grid sections added.

## Phase 3 implementation notes (2026-10-09)

- DocsSamples demos: `Table/_RowDetails` (leading toggle, expand-all, one row expanded), `Table/_RowDetailsRowClick` (toggle in an actions cell, row click, single, rail, bleed), `DataGrid/_RowDetails` (`expanded-keys`, a nested itinerary grid). They use `StaticData.DetailsFor(bookingId)` (new `BookingDetails` and `ItinerarySegment` records). The exported demos are static, so the htmx loading pattern is documented as code, not shown live.
- Website: Row details, Row details with row click, and Loading row details with htmx sections plus three API reference entries in `table.mdx`, and a Row details section with an `<sa-data-grid-row-detail>` TypeTable in `data-grid.mdx`. The htmx section documents the on-row pattern and the htmx 2 and htmx 4 `from:` syntax for the child-element pattern.
- Skills: `skills.examples.json` adds the three demos. `references/icons.md` (handwritten) gains the `RowDetailIndicator` row and its rotation note.
- Generator quirk, not fixed: both exporters drop leading `@` lines as directives, which also removes a leading `@{` block. The grid demo avoids it with a single-quoted `expanded-keys='new[] { "TRP-4821" }'`. The existing `DataGrid/_Intro` snippet already starts mid-block for this reason.

## Verification log

2026-10-09, phase 1:

- `dotnet run --project tests/StellarAdmin.TagHelpers.Tests`: 261 passed (10 new). `tests/StellarAdmin.Core.Tests`: 72 passed.
- `node util/theme-coverage/check.mjs` passed. `npm run build` (JS and all theme bundles) succeeded.
- Headless Chromium over CDP against ComponentPlayground (port 5208, htmx 4.0.0), 18 checks passed:
  - server-expanded row, aria and colspan sync;
  - aligned and bleed insets;
  - expand, collapse and expand-all with their events;
  - htmx loading once from a child element and from the detail row;
  - htmx `innerHTML` replace and `beforeend` append;
  - row click ignoring links, single mode, trailing toggle;
  - `animate=false`, and the pinned content while scrolling sideways at 390px.
- Screenshots reviewed in vega (light and dark), parallax, nova (rail), observatory, and ledger dark (rail), at desktop width plus 390px.
- Not run: DocsSamples (phase 3), the Dashboard and EF integration suites (not affected), visual-regression scripts.

2026-10-09, phase 2:

- `tests/StellarAdmin.TagHelpers.Tests`: 268 passed (7 new grid row details tests in `TagHelpers/DataGrid/DataGridTagHelperTests.RowDetails.cs`). `tests/StellarAdmin.Dashboard.IntegrationTests`: 399 passed. Theme coverage passed (DataGrid tags and hooks updated). The JS and CSS bundles built.
- CDP against ComponentPlayground (5208), 11 new grid checks passed:
  - selection wrapping row details, with the toggle after the checkbox;
  - `expanded-keys`, and a `colspan` covering selection, toggle and columns;
  - the aligned inset after selection and toggle;
  - a nested grid with its own padding, an unpadded detail cell and no overflow;
  - a checkbox in the details staying out of the selection, while row selection still works;
  - grid expand-all;
  - `toggle=None` with the toggle in an actions column, row click and single mode;
  - bleed fallback for an end-of-row toggle;
  - mobile pinning.
- The 18 phase 1 checks were re-run and still pass. Screenshots reviewed (vega, desktop).

2026-10-09, phase 3:

- DocsSamples built, and the three partials rendered with no unresolved `sa-*` elements. Screenshots reviewed in observatory dark (the default), parallax dark, and at 390px.
- `dotnet run --project docs/DocsSamplesGenerator` regenerated 448 demos. Website HTML diffs are the expected bundle-hash and SVG attribute-order churn. Theme CSS diffs are limited to the row-details rules and the grid padding selector. New: three demo HTML files, three `_include` snippets, new hashed `site` and `stellar-admin.js` assets.
- Website: `pnpm lint` and `pnpm types:check` passed, and `pnpm build` exited 0. The built Table and Data Grid docs pages were screenshotted through `vite preview`. The grid demo needed `class="w-full"` so the nested grid does not scroll inside the demo frame.
- `util/SkillsGenerator` regenerated `table.md` and `data-grid.md`, and `--check` reported no drift. `dotnet build src/StellarAdmin.TagHelpers --no-incremental`: 0 warnings.
