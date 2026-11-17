# Index layouts

Status: proposed (2026-11-18). API sketch only; nothing is implemented. Do not start until the open questions are answered.

Let an index page switch between layouts, with a switcher at the end of the controls row. A layout can show the resource's own rows or use a handler to load a different data shape (for example, revenue per month summarised from bookings).

- Prototype: `sandbox/html/index-views.html` (untracked). It shows switcher placements A and A2 and layouts whose data shape differs (the map uses `DestinationSummary`, the calendar `DepartureDay`, the chart `MonthRevenue`).
- Prior art: Avo `view_types` and ActiveAdmin `index as:` (same data, different layouts), Filament `contentGrid()`, and Nova lenses (the closest to a different data shape).
- Related: [index-page-refreshing](index-page-refreshing.md) (htmx swaps of the data area) and the filter bar from `explore/index-filters`.

## Scope

The first phase covers only additional tables and custom partials, each with the same or a different data shape. Those two exercise the whole shared API: the layouts collection, keys, the switcher, URL state, shared scopes and search, `Ignore*`, per-layout sort and paging, and the handler for a different data shape.

Built-in board, map, card and calendar layouts come later, each as its own piece of work with its own prototype and tag helpers. Each adds one `Add*` method to the layouts collection without changing the first phase's API. Until then, applications can build them as partials.

## Proposed API (first phase)

Layouts are a collection on the index, like `Columns`. The default table is a seeded entry with the key `"table"`; the switcher renders only when there is more than one layout.

```csharp
resource.Index(index =>
{
    // Unchanged; these configure the default table
    index.Columns(columns => ...);
    index.EnablePaging();

    // Shared by every layout
    index.EnableScopes(scopes => ...);
    index.EnableSearch(term => booking => ..., search => ...);

    index.Layouts(layouts =>
    {
        // Same data shape
        layouts.AddTable("finance", table =>
        {
            table.Title = "Finance";
            table.Icon = "receipt";
            table.Columns(columns => ...);
            table.DefaultSortBy(booking => booking.PaidOn);
            table.EnablePaging(paging => paging.PageSize = 50);
        });
        layouts.AddPartial("calendar", "_BookingCalendar");

        // Different data shape
        layouts.AddTable<MonthRevenue, MonthRevenueHandler>("revenue", table => table.Columns(...));
        layouts.AddPartial<MonthRevenue, RevenueChartHandler>("revenue-chart", "_RevenueChart", chart =>
        {
            chart.IgnoreSearch();
            chart.IgnoreScopes();
        });

        layouts.Default = "finance";
    });
});
```

A different data shape comes from a handler, registered like `IResourceRowDetailHandler<TModel>`. There are two kinds (see Decisions):

```csharp
// StellarAdmin: loads its own data from the request values
public interface IResourceLayoutHandler<TItem>
{
    Task<ResourceListResult<TItem>> ListAsync(ResourceLayoutRequest request, CancellationToken cancellationToken);
}

// StellarAdmin.EntityFrameworkCore: reshapes the already narrowed query
public interface IEfCoreLayoutHandler<TResource, TItem>
{
    Task<ResourceListResult<TItem>> ListAsync(
        IQueryable<TResource> resources,
        ResourceLayoutRequest request,
        CancellationToken cancellationToken
    );
}
```

Rows of a different data shape can point back to records:

```csharp
layouts.AddTable<BookingLedgerRow, BookingLedgerHandler>("ledger", table =>
{
    table.HasRecordKey(row => row.BookingId);           // links to the booking, with row actions
    table.RowActions(actions => actions.Delete = false);
});
layouts.AddTable<DestinationSummary, DestinationSummaryHandler>("destinations", table =>
    table.LinkTo<Destination>(summary => summary.DestinationId)
);
```

`ResourceLayoutRequest` is a record carrying the scope, search and (later) filters; later layouts can add `init` properties (a map's visible area, a calendar's date range) without breaking handlers. A partial receives a `ResourceLayoutModel<TItem>` with the items, the request, and helpers for record links and URLs that keep the current state.

## Decisions

- **Keys:** `AddTable` and `AddPartial` take a key, which is the `?layout=` value. Later built-in layouts default theirs (`"board"`, `"map"`).
- **Shared across layouts:** scopes, filters and search narrow the base rows, then a handler builds its items from them (searching "lisbon" counts only the matching bookings in the revenue). Their state lives in the URL and survives switching layouts.
- **Per layout:** sort, paging, columns, row details and bulk actions. `index.Columns(...)` and `index.EnablePaging()` stay shorthands for the default table.
- **Opting out:** `IgnoreSearch()` and `IgnoreScopes()` on a layout, and `IgnoreFilters()` once the filter bar exists. An ignored control is hidden while that layout is active and not applied; its value stays in the URL.
- **Switcher placement:** at the end of the controls row, after the scopes and search (placement A in the prototype).
- **Removing the default table:** `layouts.Clear()` and then add the layouts; `layouts.Remove("table")` removes only the default table. With one layout, the switcher does not render. Clearing without adding renders an empty data area.
- **No table left:** `index.Columns(...)` or `index.EnablePaging()` without the default table fails validation at startup.
- **Sorting and paging a different data shape:** supported. The request carries sort and paging, and the handler returns the items with a total, like `ResourceListResult<TItem>`.
- **Two kinds of handler:** `IResourceLayoutHandler<TItem>` (core) receives only the request values and loads its own data, so it works for standalone resources and for EF resources whose layout reads another data source; it interprets the scope, search and filters itself. `IEfCoreLayoutHandler<TResource, TItem>` (in `StellarAdmin.EntityFrameworkCore`) receives the `IQueryable<TResource>` with the scope, search and filters already applied and only reshapes it. Both work with `AddTable<TItem, THandler>` and `AddPartial<TItem, THandler>`; the EF handler on a standalone resource fails validation at startup.
- **Rows of a different data shape:** by default they don't link. `table.HasRecordKey(row => row.BookingId)` declares that each row is a record of this resource: the row links to its view page and gets the resource's row actions. `table.LinkTo<Destination>(summary => summary.DestinationId)` links each row to another resource's view page.
- **Row actions per table:** a table gets the row actions the resource allows (view, edit, delete); `table.RowActions(actions => actions.Delete = false)` turns individual ones off. A table cannot turn on an action the resource does not allow. Inline editing stays on the default table.
- **One rendering path:** the default table, added tables and partials render through the same internal layout mechanism, so later built-in layouts plug into it rather than the table being a special case.

## Open questions

None at the moment.

## Later

- **Board, map, cards and calendar:** one built-in layout each, each with its own prototype, tag helpers and plan.
- **Cards:** board cards and map popups default to the resource's `Reference` (title, description, avatar); `board.Card(card => ...)` overrides them.
- **Caps:** board and map have no paging; they need a limit such as `board.MaxItems`.
- **Writing back:** moving a card between board lanes changes the record, which ties into editing.
