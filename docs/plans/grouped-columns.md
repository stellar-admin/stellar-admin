# Grouped columns

Status: in progress (2026-11-19). Stage 1 (UI components) is implemented and documented, the website changes uncommitted; stage 2 (the admin panel API) has not been designed.

Column groups in the data grid header: a group label spanning the columns under it, nested to any depth, with the columns inside still sortable one field at a time.

## Prototype

`sandbox/html/grouped-columns.html` (2026-11-19) shows a grouped grid, a grid without groups, and groups nested several levels deep, all sortable. Its decisions:

- Header dividers: a line between every header cell and a line under every group (1b). They apply to every table, plain `sa-table` and grids without groups included (1c).
- A column outside a group spans the group rows, with its head on the column heads' line (2a).
- A right-aligned sortable head sits flush with its numbers (3a).
- The sort arrow is the only cue for a sorted column inside a group; the group shows nothing.

## Today

- `DataGridTagHelper.RenderHeader` renders a single header row from the collected `DataGridColumn`s.
- `sa-data-grid-column` has `ParentTag = "sa-data-grid"` and no alignment option. `_IndexDataGrid.cshtml:58` adds `text-right` to number columns.
- `DataGridSortTagHelper.RenderSortLink` sets `margin-left: -0.625rem` inline on every sort link.
- `data-grid.css` pads the cells at the card's edge with `tr > :first-child` and `:last-child`, which picks the wrong cells in a header with several rows.
- Plain `sa-table` has no sorting; `sa-table-head` passes `colspan`, `rowspan` and `scope` through.

## Stage 1: UI components

```cshtml
<sa-data-grid items="Model.Bookings">
    <sa-data-grid-sort asp-page="..." asp-route-sortField="{sortField}" asp-route-sortDirection="{sortDirection}" />
    <sa-data-grid-column field="Reference" title="Booking" sortable />
    <sa-data-grid-column-group title="Trip">
        <sa-data-grid-column field="Destination" sortable />
        <sa-data-grid-column-group title="Dates" align="Start">
            <sa-data-grid-column field="Departs" sortable />
            <sa-data-grid-column field="Returns" sortable />
        </sa-data-grid-column-group>
    </sa-data-grid-column-group>
    <sa-data-grid-column field="Total" align="End" sortable />
</sa-data-grid>
```

- `sa-data-grid-column-group`: new, with `title`, `align` (`DataGridColumnGroupAlign`: `Start`, `Center`, `End`; default `Center`) and `class` on its header cell. It isn't sortable. Groups nest; `sa-data-grid-column` may sit in the grid or in a group.
- `sa-data-grid-column` gets `align` (`DataGridColumnAlign`: `Start`, `Center`, `End`; default `Start`) for its head and body cells, rendered as `data-align`. On `End` the CSS pulls the sort link flush with the numbers; the inline sort-link margin moves into the CSS.
- The grid records each column's group path during the collect pass and builds the header rows from the tree: a group gets a colspan of its column count and `scope="colgroup"`, a column outside a group a rowspan down to the column heads' line, and the select-all and row-detail toggle cells span every header row. Body rows don't change; in the row passes a group only passes its children through.
- The grid marks the header cells at the card's edges with `data-edge-start` and `data-edge-end`, and the edge padding selects those instead of `:first-child` and `:last-child`.
- `table.css`: the header dividers for every table, the line under groups (`th[scope="colgroup"]`, so a hand-written plain table gets it too), and column heads spanning group rows sitting on the column heads' line. Plain `sa-table-head` and `sa-table-cell` get no `align`; `text-right` keeps working there.
- Tests, DocsSamples demos, website docs and the generated skills reference follow the usual component workflow.

### Stage 1 progress (2026-11-19)

- Done: `sa-data-grid-column-group` (`DataGridColumnGroupTagHelper`, `DataGridColumnGroupAlign`), `align` on `sa-data-grid-column` (`DataGridColumnAlign`), the header rows built from the group tree in `DataGridTagHelper.RenderHeader`, edge marks, the sort-link margin moved into `data-grid.css`, the dividers and group line in `table.css`.
- Demos: DocsSamples `Pages/DataGrid/_ColumnGroups.cshtml` (sortable, end-aligned Amount) and `_NestedColumnGroups.cshtml`; `_ColumnGroups` added to the skills examples manifest and the skills reference regenerated (`skills check` passes).
- Checks run: `StellarAdmin.UI.Tests` (824 passed, 7 new in `DataGridTagHelperTests.ColumnGroups.cs`), `StellarAdmin.IntegrationTests` (787 passed); screenshots of both demos, the intro grid and row details (default, Ops dark, Soft) on DocsSamples :5206.
- Website (uncommitted in `../website`): demos exported with `docs generate demos` (both new demos, the data grid snippets switched to `align`, and the stylesheet hash on every demo); `content/docs/ui/components/data-grid.mdx` gained "Aligning columns" and "Column groups" sections with the `<sa-data-grid-column-group>` table. `pnpm types:check` and `pnpm build` pass; `pnpm lint` fails only on formatting in three untouched files (`.vscode/settings.json`, `src/routes/docs/$.tsx`, `src/styles/app.css`).

## Stage 2: admin panel API

Not designed. It includes switching `_IndexDataGrid.cshtml` from `text-right` to `align="End"`.
