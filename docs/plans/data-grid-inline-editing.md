# Table interactivity and inline editing in the data grid

Status: proposed (2026-10-15). StellarAdmin.UI work that the admin panel's [inline editing](inline-editing.md) phase 3 builds on. Designed with a throwaway spike (`sandbox/InlineEditingSpike`, not committed), which is deleted once this plan is reviewed.

## Goal

Give `sa-data-grid` inline editing that works on its own, without the admin panel: rows that know their key, rows rendered in edit mode from per-column edit templates, and a single row rendered for an htmx (or any) row endpoint. At the same time, merge the table's client-side behaviour into one `sel-table` web component instead of one component per feature.

## Decisions (Jerrie, 2026-10-15)

1. Inline editing lives in `sa-data-grid` (design A of the spike). A separate row element (design B) was rejected: its columns cannot be shared with the grid through a partial or a Razor template, and the grid would still have to know a row is being edited.
2. Edit mode is set with attributes on the grid, not with ambient state: `item-key`, `editing-keys`, `new-item` and `rows-only`.
3. Any number of rows can be in edit mode at once; there is at most one new row.
4. The table keeps a single `<tbody>`. A row carries `data-key`; its details row has an id built from the key.
5. A row request does only its own work. Edit loads one item and returns only the editing `<tr>`, without its details row. Save returns the display row plus its details content, which may depend on the edited values. Add queries nothing. The page itself is the only request that lists items.
6. An editing row has no selection checkbox and no visible details toggle. Its details row stays in place, open or closed, untouched by edit and cancel; save replaces only the details' content.
7. Cancel needs no request: the display row is kept when the row enters edit mode and put back on cancel. Only a row the page itself rendered in edit mode goes to the server.
8. Column widths are locked while any row is in edit mode, so editors cannot make the columns jump. Editors fill their cell instead of widening it.
9. Selection and expanded details are remembered by key, so a row keeps them when it is replaced. A row in edit mode still counts as selected.
10. Editing rows' fields are named `rows[key].Field`. Not configurable for now.
11. One `sel-table` web component replaces `sel-table-selection` and `sel-table-row-details`, with selection, row details and editing as separate behaviours inside it, switched on by attributes. `sa-table` and `sa-data-grid` both render it. It does not depend on htmx: it reacts to rows being replaced in the page. The `sa-table-selection` and `sa-table-row-details` wrappers are removed: `sa-table` takes `selection` and `row-details` (with the `row-click` and `row-detail-*` settings) and renders `sel-table` only when one is set (Jerrie, 2026-10-16).

## API

```cshtml
<sa-data-grid items="Model.Items" item-key="Id"
              editing-keys="Model.EditingKeys" new-item="Model.NewItem" rows-only="Model.RowsOnly">
    <sa-data-grid-selection name="selected" />          @* key-field defaults to the grid's item-key *@
    <sa-data-grid-column title="Name" field="Name">
        <sa-data-grid-edit-template>
            <sa-input asp-for="..." />
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-row-detail>...</sa-data-grid-row-detail>  @* key-field defaults to the grid's item-key *@
</sa-data-grid>
```

- `item-key`: the item property whose value identifies a row. Rows get `data-key`, details rows the id `{grid}-detail-{key}`, and generated ids use the key instead of the row position.
- `editing-keys`: the rows rendered with their columns' edit templates, marked `data-editing`. A column without an edit template keeps its display.
- `new-item`: one item rendered in edit mode at the top, keyed `new`.
- `rows-only`: outputs only the rows (`<tr>`, plus the details row for a display row), without the card, table and header, for a row endpoint.
- `<sa-data-grid-edit-template>`: a column's editor, next to its item template.

```html
<sel-table data-selection data-row-details data-expand-mode="multiple" ... data-editing>
    <div class="sa-table-container"><table>...</table></div>
</sel-table>
```

The JavaScript API stays the same, on the one element: `selectedValues` and `selection-change`; `expandAll()`, `collapseAll()`, `toggle()`, `row-detail-expand` and `row-detail-collapse`. Scripts that query `sel-table-selection` or `sel-table-row-details` by name change to `sel-table`, and pages using `sa-table-selection` or `sa-table-row-details` move to the `sa-table` attributes (breaking changes from the previews).

## Stages

1. **One `sel-table` component.** Move the selection and row details behaviours into `sel-table`, unchanged, as separate modules. `sa-table` gets `selection` and `row-details` and renders `sel-table` around its container when either is set; the `sa-table-selection` and `sa-table-row-details` wrappers are removed. `sa-data-grid` renders one `sel-table`. Two fixes found in the spike: selection counts only the row selection checkboxes (marked by the grid and `sa-table`), not checkboxes inside cells such as a switch editor; and the details indent is measured from any row with a toggle, not only the first details row's row. Update the tag helper tests, the Table and DataGrid docs samples and the consumer JavaScript reference; browser-check selection and row details in both components.
   Done (2026-10-16, not committed). `sel-table` (`Client/js/web-components/sel-table.ts`) with `sel-table/selection.ts` and `sel-table/row-details.ts`, switched on by `data-selection` and `data-row-details`; `data-slot="table-interactivity"`. `sa-table` attributes: `selection`, `row-details`, `row-click`, `row-detail-expand-mode`, `row-detail-emphasis`, `row-detail-divider`, `row-detail-inset`, `row-detail-animate`; the table's `id` and other attributes go on `sel-table`, its `class` stays on the `<table>`. `TableElementRendering` gives `sa-table` and the grid the element's attributes. The row selection marker is `data-row-select`: when any row checkbox has it, only marked checkboxes select rows, otherwise every body checkbox does as before. The grid marks its row checkboxes, and the Table row selection sample marks its own. The DocsSamples and ComponentPlayground pages moved to the attributes. Verified: the UI tag helper tests (517 passing), the ComponentPlayground build, and headless Chromium on DocsSamples (Table row selection and row details, DataGrid intro and row details): one element per table, select-all and the row highlight, an unmarked checkbox in a cell leaving the selection and `selection-change` alone, toggles, `expandAll()`/`collapseAll()`, and the details inset unchanged when the first row loses its toggle. Consumer references regenerated. Committed as 9351576.
   Follow-up (2026-10-16, not committed): `sa-table-selection-head` and `sa-table-selection-cell` render the select-all and row checkboxes (`label`; the cell also `name`, `value`, `checked`, `disabled`), the cell marking its checkbox `data-row-select`; the Table row selection sample uses them. Only checkboxes marked `data-row-select` select rows now: the fallback to every body checkbox when none is marked is removed (Jerrie). Verified: the UI tag helper tests (522 passing) and the same headless Chromium checks, all passing.
2. **Row keys and row memory.** `item-key` on `sa-data-grid`, with `data-key`, key-based ids and details row ids; `key-field` on selection and row details defaults to it. `sel-table` remembers selected values and expanded details by key and restores them on rows that are replaced. Tag helper tests and browser checks of replaced rows keeping their state.
3. **Edit mode rendering.** `<sa-data-grid-edit-template>`, `editing-keys`, `new-item` and `rows-only`, with the editing row rules of decisions 6 and 10: `data-editing`, the `rows[key]` field prefix, no checkbox, a hidden toggle (kept so the details indent can still be measured), no details row; a saved row renders with its details row. The cell editor styles (`.sa-cell-editor`, `.sa-cell-check`, the editing row, `.sa-cell-error`) move from the admin panel's `client.css` into StellarAdmin.UI. Tag helper tests for each mode.
4. **Editing behaviour in `sel-table`.** Lock column widths before an editing row is inserted (the widths are tracked while no row is editing, since insertion happens before the component sees it) and release them when no row is editing; keep the display row on entering edit mode and restore it on cancel; remove a discarded new row. Browser checks across the theme fixtures.
5. **Docs and references.** DataGrid docs for row keys and inline editing, with an htmx example of the row endpoints; regenerated consumer skill references; the website component export.

## Spike findings

- Each row request did only its own store call (Edit `Find`, Save `Update` and `Find`, Add nothing), checked with a per-request log.
- Column definitions in a partial cannot see the grid: tag helper context does not cross into a partial. A Razor template delegate renders after the grid has finished, unless written with `await template(null).WriteAction(Output)`.
- One `<tbody>` per item broke the table's row separators and is not how a table should be structured; replacing only the `<tr>` and refreshing the details content works with htmx (`hx-select="tr[data-key]"` with `hx-select-oob="#detail-7:innerHTML"`).
- The selection component counted a switch editor as a selected row.
- The details indent fell back to the first cell when the first details row's row was in edit mode, moving every row's details to the left.
- In automatic table layout, editors (text inputs at about 20 characters, selects at their longest option) and the wider Cancel and Save buttons widened their columns and moved every other column. Locking the header widths with `table-layout: fixed` kept every column at the same position through edit, cancel, add and save.

## Open points

- How the actions column reserves the width of the editing row's buttons. The spike stacked invisible Cancel and Save buttons under Edit in display rows; the library may offer that as a column option.
- The markup the component recognises for cancel and discard (`data-row-cancel`, `data-row-discard` in the spike).
