# Index layouts

An index page can switch between several *layouts*: tables with their own columns, sort, paging and row details, and partial views of your own (cards, a summary). A switcher right-aligned above the list card lists them; it appears only with two or more layouts. Layouts can show the resource's records or items of another shape loaded by a handler.

## Contents

- [Adding layouts](#adding-layouts)
- [Title and icon](#title-and-icon)
- [Tables](#tables)
- [Partial views](#partial-views)
- [Ignoring search or scopes](#ignoring-search-or-scopes)
- [Items from a handler](#items-from-a-handler)
- [Linking items to records](#linking-items-to-records)

## Adding layouts

Use the two-callback overload of `Index`. The first callback holds what every layout shares (title, labels, search, scopes, widgets); table members such as `Columns` don't exist there and don't compile. The second adds the layouts:

```csharp
resource.Index(
    index =>
    {
        index.EnableSearch(term => booking => booking.Reference.Contains(term));
        index.EnableScopes(scopes =>
        {
            scopes.Add("all", "All");
            scopes.Add("pending", "Pending", booking => booking.Status == BookingStatus.Pending);
        });
    },
    layouts =>
    {
        layouts.AddTable("bookings", table =>
        {
            table.Columns(columns =>
            {
                columns.Add(booking => booking.Reference);
                columns.Add(booking => booking.BookedAt, column => column.Sortable());
            });
            table.DefaultSortBy(booking => booking.BookedAt);
            table.EnablePaging();
        });
        layouts.AddTable("finance", table =>
        {
            table.Icon = "receipt";
            table.Columns(columns => columns.Add(booking => booking.TotalAmount, column => column.Sortable()));
            table.EnablePaging(paging => paging.PageSize = 50);
            table.RowActions(actions => actions.Delete = false);
        });
        layouts.AddPartial("cards", "_BookingCards");
    });
```

- The first argument is the key, the URL value (`?layout=finance`). Keys are unique per index (case-insensitive); a blank or repeated key throws `ArgumentException`. An unknown `?layout=` falls back to the first layout.
- The first layout is the default; the switcher follows the order of the `Add*` calls.
- Search and scope apply to every layout and survive switching; sort and paging belong to each table and reset on a switch.
- With nothing to share, pass an empty first callback: `resource.Index(index => { }, layouts => ...)`.
- The one-callback `Index(index => ...)` is still the single-table index; don't mix the two for one resource (the two-callback overload replaces an earlier one-callback table).
- A custom data source receives the active layout's key as `ResourceListRequest.Layout`, for tables that sort on different columns.
- `ResourceListRequest.Fields` holds the expressions the active layout reads from each item: a table's column expressions, nothing for a partial. A data source can load related data from them.

## Title and icon

Every layout builder has `Title` and `Icon`. `Title` is the segment's tooltip and accessible name; it defaults to the key humanized (`"finance"` reads "Finance"). `Icon` is a Lucide icon name; without one the segment shows the title as text.

## Tables

`AddTable(key)` returns the table builder; `AddTable(key, configure)` returns the layouts builder. A table has the members of the one-callback index's table part: `Columns` ([columns](columns.md)), `DefaultSortBy`/`DefaultSortByDescending`, `EnablePaging`, `EnableRowDetails` ([index pages](index-pages.md#row-details)), plus `RowActions`.

- `RowActions(actions => ...)` sets `View`, `Edit` and `Delete` to `false` to hide them in that table. Setting `true` can't show an action the resource doesn't allow.
- Inline editing ([inline editing](inline-editing.md)) appears only in the first table of the resource's own records.

## Partial views

`AddPartial(key, partialName)` renders a partial view instead of the grid, with every record that the search and scope select (no sort, no paging). Its model is `ResourceLayoutModel<TResource>` (namespace `StellarAdmin.Resources`):

| Member | Value |
| --- | --- |
| `Items` | The records (or a handler's items) |
| `Request` | The `ResourceListRequest`: `Layout`, `Search`, `Scope` |
| `GetViewUrl(item)`, `GetEditUrl(item)` | The record's view or edit page, or `null` when there is none |
| `GetIndexUrl(values)` | The index with the current layout, search and scope; `new { layout = "finance" }` adds or replaces values |

```razor
@* Areas/StellarAdmin/Views/bookings/_BookingCards.cshtml *@
@using StellarAdmin.Resources
@model ResourceLayoutModel<Booking>

@foreach (var booking in Model.Items)
{
    <sa-card>
        <sa-card-header><sa-card-title>@booking.Reference</sa-card-title></sa-card-header>
        @if (Model.GetViewUrl(booking) is { } viewUrl)
        {
            <sa-card-footer><sa-link-button href="@viewUrl">View</sa-link-button></sa-card-footer>
        }
    </sa-card>
}
<a href="@Model.GetIndexUrl(new { layout = "finance" })">Compare the totals</a>
```

- Put the partial in `Areas/StellarAdmin/Views/{slug}/` or `Areas/StellarAdmin/Views/Shared/`, with an area `_ViewImports.cshtml` ([customization](customization.md#view-imports)).
- An EF Core partial of records gets no includes, so navigation properties are `null`. Load related data with a handler instead.
- Style with `sa-*` components; Tailwind utilities that StellarAdmin's CSS doesn't contain render unstyled.

## Ignoring search or scopes

`layout.IgnoreSearch()` and `layout.IgnoreScopes()` hide the control while that layout is active and don't apply it to the layout's records or the handler's request (`Search`/`Scope` are `null`). The value stays in the URL and applies again on other layouts.

## Items from a handler

For data of another shape (a summary, a ledger), pass the item type and a handler: `AddTable<TItem, THandler>(key, ...)` or `AddPartial<TItem, THandler>(key, partialName, ...)`. The handler is registered as a scoped service unless it already is.

**EF Core:** implement `IEfCoreLayoutHandler<TEntity, TItem>` (namespace `StellarAdmin.EntityFrameworkCore`). It receives an untracked query with the search and scope already applied, and reshapes it:

```csharp
public record DestinationRevenue(int DestinationId, string Destination, int Bookings, decimal Revenue);

public class DestinationRevenueHandler : IEfCoreLayoutHandler<Booking, DestinationRevenue>
{
    public async Task<ResourceListResult<DestinationRevenue>> ListAsync(
        IQueryable<Booking> bookings, ResourceListRequest request, CancellationToken cancellationToken)
    {
        var rows = bookings
            .GroupBy(booking => new { booking.Departure.Trip.DestinationId, booking.Departure.Trip.Destination.Name })
            .Select(group => new
            {
                group.Key.DestinationId,
                group.Key.Name,
                Bookings = group.Count(),
                Revenue = group.Sum(booking => booking.TotalAmount),
            });

        rows = request.Sort is { Field: nameof(DestinationRevenue.Revenue) } sort
            ? sort.Direction == ResourceSortDirection.Ascending
                ? rows.OrderBy(row => row.Revenue).ThenBy(row => row.Name)
                : rows.OrderByDescending(row => row.Revenue).ThenBy(row => row.Name)
            : rows.OrderBy(row => row.Name);

        var total = await rows.CountAsync(cancellationToken);
        if (request.Paging is { } paging)
            rows = rows.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);

        var items = await rows
            .Select(row => new DestinationRevenue(row.DestinationId, row.Name, row.Bookings, row.Revenue))
            .ToListAsync(cancellationToken);
        return new ResourceListResult<DestinationRevenue>(items, total);
    }
}
```

Sort and page an anonymous projection, and construct the record last: EF Core can't translate a sort on the members of a type the query constructs through its constructor.

**Any resource:** implement `IResourceLayoutHandler<TItem>` (namespace `StellarAdmin.Resources`), whose `ListAsync(ResourceListRequest request, CancellationToken cancellationToken)` loads the items itself and applies `request.Search` and `request.Scope` itself. A custom-data-source resource takes only this kind (an EF handler doesn't compile there); an EF Core resource takes either.

```csharp
layouts.AddTable<DestinationRevenue, DestinationRevenueHandler>("revenue", table =>
{
    table.Title = "Revenue per destination";
    table.Columns(columns =>
    {
        columns.Add(item => item.Destination);
        columns.Add(item => item.Revenue, column => column.Sortable());
    });
    table.DefaultSortByDescending(item => item.Revenue);
    table.EnablePaging();
});
layouts.AddPartial<DestinationRevenue, DestinationRevenueHandler>("revenue-cards", "_RevenueCards");
```

- A handler table sorts and pages through the handler: `request.Sort.Field` is the column's property name, `request.Sort.Selector` its sort expression over the row type, `request.Paging` the page. Return the total of all items so the pager counts pages.
- A handler partial's request has no `Sort` or `Paging`; return every item. Its model is `ResourceLayoutModel<TItem>`.
- Handler tables have no row details and no inline editing.

## Linking items to records

Handler items don't link by default: no row actions in a table, and `GetViewUrl`/`GetEditUrl` return `null` in a partial. Call one of these on the handler table or partial builder:

- `HasRecordKey(item => item.BookingId)`: each item is one of this resource's records. Table rows get the resource's view, edit and delete actions (filtered by `RowActions`); a partial's `GetViewUrl`/`GetEditUrl` return the record's pages.
- `LinkTo<Destination>(item => item.DestinationId)`: each item links to another resource's view page (table rows get a view action; a partial's `GetViewUrl` returns it, `GetEditUrl` stays `null`). The other resource needs `AllowView`; without it the items don't link and a configuration problem is reported. Users not authorized for that resource see no link.

A key function that returns `null` leaves that item unlinked (a totals row). Calling both `HasRecordKey` and `LinkTo` on one layout throws `InvalidOperationException`.
