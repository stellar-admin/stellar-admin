# Grouped columns

Status: done (2026-11-19). Stage 1 (UI components) and stage 2 (the admin panel API) are implemented and documented, in the product and the website.

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
- Website (`../website`): demos exported with `docs generate demos` (both new demos, the data grid snippets switched to `align`, and the stylesheet hash on every demo); `content/docs/ui/components/data-grid.mdx` gained "Aligning columns" and "Column groups" sections with the `<sa-data-grid-column-group>` table. `pnpm types:check` and `pnpm build` pass; `pnpm lint` fails only on formatting in three untouched files (`.vscode/settings.json`, `src/routes/docs/$.tsx`, `src/styles/app.css`).

## Stage 2: admin panel API

Designed and agreed 2026-11-19, then implemented. It builds on the list request fields rework ([list-request-fields](list-request-fields.md)).

`ResourceTableLayoutOptions.Columns` stays the flat list of columns in display order, and each column refers to its innermost group. Because of this, `Fields`, `FindSort`, `SortableColumns`, the inline-row field mapping (`rowFields?[index]` in `_IndexDataGrid.cshtml`, `GetColumnFields` in `ResourceController.Index.cs`, `InlineRowCheck`) and the checks that remove columns (`ComputedColumnCheck`, `BadgeDisplayCheck`, `ValueTextCheck`) don't change. A group whose columns are all removed isn't rendered.

```csharp
index.Columns(columns =>
{
    columns.Add(b => b.Reference).Sortable();
    columns.AddGroup("Trip", trip =>
    {
        trip.Add(b => b.Destination).Sortable();
        trip.AddGroup("Dates", dates =>
        {
            dates.Align = DataGridColumnGroupAlign.Start;
            dates.Add(b => b.Departs).Sortable();
            dates.Add(b => b.Returns).Sortable();
        });
    });
    columns.Add(b => b.Total, total => total.Align = DataGridColumnAlign.End);
});
```

Builders (`src/StellarAdmin/Resources/Builders`), shared by record and handler tables:

```csharp
// ResourceTableColumnsBuilder<TItem>
public ResourceTableColumnsBuilder<TItem> AddGroup(string title, Action<ResourceTableColumnGroupBuilder<TItem>> configure);

// New: ResourceTableColumnGroupBuilder<TItem>, with no Clear()
public DataGridColumnGroupAlign Align { set; }
public ResourceTableColumnBuilder<TItem> Add<TProperty>(Expression<Func<TItem, TProperty>> field);
public ResourceTableColumnGroupBuilder<TItem> Add<TProperty>(Expression<Func<TItem, TProperty>> field, Action<ResourceTableColumnBuilder<TItem>> configure);
public ResourceTableColumnGroupBuilder<TItem> AddGroup(string title, Action<ResourceTableColumnGroupBuilder<TItem>> configure);

// ResourceTableColumnBuilder<TItem>: set-only, like Title and EmptyText
public DataGridColumnAlign Align { set; }
```

Options (`src/StellarAdmin/Resources/Options`):

```csharp
// DataGridColumnOptions
public DataGridColumnAlign? Align { get; }            // null: End for a NumberDisplay column, otherwise Start
public DataGridColumnGroupOptions? Group { get; }     // the innermost group, or null

// New
public sealed class DataGridColumnGroupOptions
{
    public string Title { get; }
    public DataGridColumnGroupAlign Align { get; }    // default Center
    public DataGridColumnGroupOptions? Parent { get; }
}
```

Decisions (agreed 2026-11-19):

1. The group builder is its own type, without `Clear()`. `Clear()` on the top-level builder removes every column, grouped or not.
2. `AddGroup` requires the title.
3. A column without `Align` is `End` when its display is `NumberDisplay`, otherwise `Start`. `_IndexDataGrid.cshtml` drops `text-right` and renders `align`.
4. Groups get no class option, as admin columns have none.

The options reuse the StellarAdmin.UI enums `DataGridColumnAlign` and `DataGridColumnGroupAlign`, as `RowDetails` reuses `DataGridRowDetailToggle`. `_IndexDataGrid.cshtml` builds the group tree from runs of consecutive columns that share a group, and renders nested `sa-data-grid-column-group` elements recursively. The builders keep a group's columns together, so a group is always one run.

Follow-up: tests, `PublicAPI.Unshipped.txt`, the consumer skills references and the website admin docs, per the usual workflow.

### Stage 2 progress (2026-11-19)

- Done: `DataGridColumnGroupOptions`, `Align` and `Group` on `DataGridColumnOptions`, `Align` on `ResourceTableColumnBuilder`, `AddGroup` on `ResourceTableColumnsBuilder`, the new `ResourceTableColumnGroupBuilder`, and the `PublicAPI.Unshipped.txt` entries.
- `_IndexDataGrid.cshtml` renders the columns with recursive local functions (`RenderColumns`, `RenderColumn`, `FindChildGroup`) and renders `align` instead of `text-right`.
- Change to stage 1, approved by Jerrie: `sa-data-grid-column` and `sa-data-grid-column-group` no longer have a `ParentTag` restriction. Razor checks `ParentTag` at compile time, and markup in a local function has no parent element, so neither tag helper matched there. Both already do nothing outside a grid.
- Tests: `ResourceTableColumnsBuilderTests` (new) and `ResourceTableColumnBuilderTests` (`Align`); `ResourceColumnGroupTests` (new integration tests); `ResourceColumnDisplayTests` asserts `data-align` instead of `text-right` and gained an alignment override test.
- Checks run: full solution test run (`dotnet test --solution StellarAdmin.slnx --configuration Release`), 2239 passed. Checked in a browser through the `columns-groups` screenshots and the AdminPlayground Trips index on :5207.
- Consumer skills: `skills/stellar-admin/references/columns.md` gained "Alignment" and "Column groups" sections and the `Align` row; the `SKILL.md` columns row names `Align` and `AddGroup`. `skills generate` rewrote `skills/stellar-admin-ui/references/components/data-grid.md`: the duplicate rows from the old two `ParentTag` targets went, and the examples use `align`. `skills check` passes.
- Website (`../website`, committed in be03791b): `content/docs/admin/columns.mdx` gained "Aligning columns" and "Grouping columns" sections, an `Align` row and the `columns-groups` screenshot, generated from `tools/StellarAdmin.Cli/Docs/Screenshots/Columns/ColumnsGroupsScenario.cs` and `columns-groups.spec.yaml` (light and dark, checked by eye). `pnpm types:check` and `pnpm build` pass.
- AdminPlayground: the Trips index groups Country and City under Location, inside Destination with Arrival airport.
