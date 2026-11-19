# List request fields

Status: approved (2026-11-19), not started. Jerrie agreed the direction and decisions 1 to 4; the names `Fields` and `Selector` are still open.

The index tells its data source what each item will be read for, so the EF data source stops looking up the layout again to find its columns. This prepares for grouped table columns and card layouts, which both change where a layout's fields live.

- Related: [index-layouts](index-layouts.md) (stage 3 recorded that an EF partial gets no includes) and the `card-layout` and `grouped-columns` worktrees.

## Problem

`ResourceListRequest` carries the layout's key but not what the layout reads. `EfCoreResourceDataSource.ListAsync` (`src/StellarAdmin.EntityFrameworkCore/EfCoreResourceDataSource.cs:79`) calls `FindLayout(request.Layout)` again. It uses the columns of a table, the first record table's columns for a handler table's key, and nothing for any other layout. `Arrange` then turns those columns into `Include` paths and finds the sort column by `SortField`. Because of this:

- every new layout kind has to be special-cased in the data source, or it gets no includes (which is what happened to partials);
- a request without a layout key, such as a core reference lookup, falls back to the first layout's columns by accident;
- custom data sources only get the key, and have to map it back to options themselves.

## Decisions (agreed 2026-11-19)

1. The request carries the expressions the caller will read from each item. The layout works out its own list; only plain expressions cross `IResourceDataSource`.
2. The controller resolves the sort to an expression and passes it down. The EF data source stops matching `SortField` against columns.
3. Lookups and references that list through a data source pass the fields they read themselves (title, description, media) instead of relying on the first layout.
4. Partials without a handler keep an empty field list for now. Letting a partial's layout builder declare its fields can come later.

## Proposed API

```csharp
public sealed record ResourceListRequest
{
    // existing: Keys, Layout, Paging, Scope, Search, Sort

    // The expressions the caller reads from each returned item, such as an index column's p => p.Category.Name.
    // A data source can use them to load related data; one that ignores them still works. The default is empty.
    public IReadOnlyList<LambdaExpression> Fields { get; init; } = [];
}

public sealed record ResourceSort(string Field, ResourceSortDirection Direction)
{
    // The expression to order by, such as the column's sort expression or field expression, or null when the caller
    // only knows the field name. The default is null.
    public LambdaExpression? Selector { get; init; }
}
```

`Field` and `Layout` stay as they are: custom data sources and handlers sort by `Field` and branch on `Layout` today.

Open names: `Fields` and `Selector`.

## Changes

Layout side (`src/StellarAdmin/Resources/Options`):

- `ResourceLayoutOptions` gets an internal virtual list of the fields the layout reads. It's empty by default and is overridden by `ResourceTableLayoutOptions` (each column's `FieldExpression`). `ResourcePartialLayoutOptions` keeps the default.
- `ResourceTableLayoutOptions` gets an internal way to find a sortable column by field name, which returns the sort selector (`SortExpression ?? FieldExpression`). The controller's sort validation, `DisableEntitySorting`, `FindDefaultOrder` and `DataSourceReferenceItems` use it in place of their own walks over `Columns`. Grouped columns will only need to change these two members.

Controller (`src/StellarAdmin/Areas/StellarAdmin/Controllers/ResourceController.Index.cs`):

- `TryCreateListRequest` sets `Fields` from the shown layout and `Sort` with its `Selector`, including the table's default sort.
- `LoadRowsAsync` (the reload after an inline save) passes the shown layout's fields.
- Handler layouts get the same `Fields` and `Sort`, expressed over their item type.

Core references (`src/StellarAdmin/Resources/Editors/DataSourceReferenceItems.cs`):

- `FindAsync`, `ListAsync` and `SearchAsync` pass the title, description and media expressions as `Fields`, and the first record table's default sort with its `Selector`.

EF data source (`src/StellarAdmin.EntityFrameworkCore/EfCoreResourceDataSource.cs`):

- `ListAsync` no longer calls `FindLayout` or reads `Index.RecordTables`.
- `Arrange` takes its includes from `request.Fields`, through the same `FindDisplayIncludePaths` and title include paths as now.
- It orders by `request.Sort.Selector`, then by the key. A sort without a selector throws `InvalidOperationException`, since EF has no column to resolve the field name against.
- A field or selector whose parameter isn't `TEntity` throws `ArgumentException`.

EF references (`EfCoreLookupItems`) run their own projected queries and don't change.

## Implementation notes

Code locations as of 2096e58 (`layout-column-collection` branch):

- `EfCoreResourceDataSource.cs`:
  - `ListAsync`: lines 79-98, the `FindLayout` call and the switch over layout kinds.
  - `Narrow`: line 181. It stays, and `EfCoreLayoutLoader` keeps handing it to EF handlers.
  - `Arrange`: lines 272-316.
  - `FindDisplayIncludePaths`: line 540. It joins `EfCorePropertyPaths.FindDisplayIncludePaths` and `FindTitleIncludePaths` with `EfCoreResourceTitles.FindIncludePaths`.
- Columns are read in these places, all of which move to the table's two internal members:
  - `ResourceController.Index.cs` `TryCreateListRequest`, about line 745: the sort column is matched `OrdinalIgnoreCase` on `SortField`, and `table.DefaultSort` is otherwise used as is.
  - `StellarAdminBuilderExtensions.cs` (EF) `DisableEntitySorting`, line 397.
  - `EfCoreResourceReference.cs` `FindDefaultOrder`, line 131.
  - `DataSourceReferenceItems.cs` `ListAsync`/`SearchAsync`: they pass `RecordTables.FirstOrDefault()?.DefaultSort`.
- `ResourceTableLayoutOptions.DefaultSort` is a `ResourceSort` without a selector, so its selector must be resolved whenever a default sort goes on a request, including for references.
- Controller entry points:
  - `IndexLayout`/`IndexTable`: line 35.
  - `IndexView`'s handler-partial branch: line 417.
  - `ListRowsAsync` (handler table or data source): line 536.
  - `LoadRowsAsync`: line 627.
- Handler requests:
  - A handler table's fields are its columns' expressions over `TItem`.
  - A handler partial's fields are empty.
  - Only record layouts reach `EfCoreResourceDataSource`, so its fields are always over `TEntity`.
- Existing test fixtures that construct `ResourceSort` positionally keep compiling (`ReferenceFixtures`, `ProductDataSource`, `ProductRowsHandler`).
- Add `Fields` and `Selector` to `src/StellarAdmin/PublicAPI.Unshipped.txt`, with XML docs per the convention.

Out of scope: `EfCorePropertyPaths` doesn't find navigations read from an inner lambda's parameter, such as `p.Tags.Select(t => t.Category.Name)` (only `Tags` is included). It stays as it is.

Working agreement: implement on the `layout-column-collection` branch, then stop for review; no commit unless Jerrie asks. The `card-layout` and `grouped-columns` worktrees build on this afterwards. The explanatory diagrams are in `/tmp/stellar-admin-diagrams/` (`column-data-flow.html` for today and `list-request-fields.html` for the proposal); they aren't part of the repo.

## Verification

- Unit tests (TUnit, mirroring the SUT's folders):
  - a table layout's fields and sort selector;
  - a partial with no fields;
  - `DataSourceReferenceItems` passes its title, description and media fields.
- EF integration tests:
  - includes come from `request.Fields`;
  - ordering uses `request.Sort.Selector`;
  - a sort without a selector throws;
  - a request with no fields includes nothing.
- Existing index layout, inline edit and reference tests pass unchanged.
- Full solution test run.

## Docs and references

- `skills/stellar-admin/references/layouts.md:58` and the other consumer references that describe `ResourceListRequest` gain `Fields` and `Selector`, regenerated per [skills](../repos/skills.md).
- Website docs pages that describe custom data sources need the same addition, batched with the website.
