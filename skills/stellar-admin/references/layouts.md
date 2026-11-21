# Index data layouts

An index page can switch between several *data layouts*: tables with their own columns, sort, paging and row details, built-in cards ([cards](cards.md)), and partial views of your own (a grouped list, a summary). A switcher right-aligned above the list card lists them; it appears only with two or more data layouts. Data layouts can show the resource's records or items of another shape loaded by a handler.

## Contents

- [Adding data layouts](#adding-data-layouts)
- [Title and icon](#title-and-icon)
- [Tables](#tables)
- [Partial views](#partial-views)
- [Ignoring search or scopes](#ignoring-search-or-scopes)
- [Items from a handler](#items-from-a-handler)
- [Linking items to records](#linking-items-to-records)

## Adding data layouts

`resource.Index(index => ...)` configures the page (title, labels, search, scopes, widgets) and adds its data layouts with `AddTable`, `AddCards` and `AddPartial`. Table members such as `Columns` belong to the table's builder, not the index's:

```csharp
resource.Index(index =>
{
    index.EnableSearch(term => booking => booking.Reference.Contains(term));
    index.EnableScopes(scopes =>
    {
        scopes.Add("all", "All");
        scopes.Add("pending", "Pending", booking => booking.Status == BookingStatus.Pending);
    });

    index.AddTable("bookings", table =>
    {
        table.Columns(columns =>
        {
            columns.Add(booking => booking.Reference);
            columns.Add(booking => booking.BookedAt, column => column.Sortable());
        });
        table.DefaultSortBy(booking => booking.BookedAt);
        table.EnablePaging();
    });
    index.AddTable("finance", table =>
    {
        table.Icon = "receipt";
        table.Columns(columns => columns.Add(booking => booking.TotalAmount, column => column.Sortable()));
        table.EnablePaging(paging => paging.PageSize = 50);
        table.RowActions(actions => actions.Delete = false);
    });
    index.AddCards("cards", cards => cards.Card(card =>
    {
        card.Title(booking => booking.Reference);
        card.Badge(booking => booking.Status);
        card.Stats(stats => stats.Add(booking => booking.TotalAmount));
    }));
});
```

- The first argument is the key, the URL value (`?layout=finance`). Keys are unique per `Index` call (case-insensitive); a blank or repeated key throws `ArgumentException`. An unknown `?layout=` falls back to the first data layout.
- The first data layout is the default; the switcher follows the order of the `Add*` calls.
- An index without data layouts shows no records.
- Search and scope apply to every data layout and survive switching; sort and paging belong to each table or cards data layout and reset on a switch.
- A custom data source receives the active data layout's key as `ResourceListRequest.Layout`, for tables that sort on different columns.
- `ResourceListRequest.Fields` holds the expressions the active data layout reads from each item: a table's column expressions, a card's title, description, image, badge, stats and fields, nothing for a partial. A data source can load related data from them.

## Title and icon

Every data layout builder has `Title` and `Icon`. `Title` is the segment's tooltip and accessible name; it defaults to the key humanized in sentence case (`"by-day"` reads "By day"). `Icon` is a Lucide icon name; without one the segment shows the title as text.

## Tables

`AddTable(key)` returns the table builder; `AddTable(key, configure)` returns the index builder. A table has `Columns` ([columns](columns.md)), `DefaultSortBy`/`DefaultSortByDescending`, `EnablePaging`, `EnableRowDetails` ([index pages](index-pages.md#row-details)), `RowActions`, and `EnableInlineEditing`/`EnableInlineCreate` ([inline editing](inline-editing.md)).

- `RowActions(actions => ...)` sets `View`, `Edit` and `Delete` to `false` to hide them in that table. Setting `true` can't show an action the resource doesn't allow.
- Inline editing belongs to the table that enables it. Other data layouts, such as cards, link to the edit and create pages.
- Cards take `RowActions` too ([cards](cards.md#row-actions)).

## Partial views

`AddPartial(key, partialName)` renders a partial view instead of the grid, with every record that the search and scope select (no sort, no paging). Its model is `ResourceDataLayoutModel<TResource>` (namespace `StellarAdmin.Resources`):

| Member | Value |
| --- | --- |
| `Items` | The records (or a handler's items) |
| `Request` | The `ResourceListRequest`: `Layout`, `Search`, `Scope` |
| `GetViewUrl(item)`, `GetEditUrl(item)` | The record's view or edit page, or `null` when there is none |
| `GetIndexUrl(values)` | The index with the current layout, search and scope; `new { layout = "finance" }` adds or replaces values |

```csharp
index.AddPartial("by-day", "_BookingsByDay");
```

```razor
@* Areas/StellarAdmin/Views/bookings/_BookingsByDay.cshtml *@
@using StellarAdmin.Resources
@model ResourceDataLayoutModel<Booking>

@foreach (var day in Model.Items.GroupBy(booking => booking.BookedAt.Date).OrderByDescending(day => day.Key))
{
    <sa-card>
        <sa-card-header><sa-card-title>@day.Key.ToString("dddd, d MMMM yyyy")</sa-card-title></sa-card-header>
        <sa-card-content>
            @foreach (var booking in day)
            {
                <sa-group>
                    @if (Model.GetViewUrl(booking) is { } viewUrl)
                    {
                        <a href="@viewUrl">@booking.Reference</a>
                    }
                    else
                    {
                        @booking.Reference
                    }
                    <sa-badge variant="BadgeVariant.Secondary">@booking.Status</sa-badge>
                </sa-group>
            }
        </sa-card-content>
    </sa-card>
}
<a href="@Model.GetIndexUrl(new { layout = "finance" })">Compare the totals</a>
```

- Put the partial in `Areas/StellarAdmin/Views/{slug}/` or `Areas/StellarAdmin/Views/Shared/`, with an area `_ViewImports.cshtml` ([customization](customization.md#view-imports)).
- An EF Core partial of records gets no includes, so navigation properties are `null`. Load related data with a handler instead.
- Style with `sa-*` components; Tailwind utilities that StellarAdmin's CSS doesn't contain render unstyled.

## Ignoring search or scopes

`layout.IgnoreSearch()` and `layout.IgnoreScopes()` hide the control while that data layout is active and don't apply it to the data layout's records or the handler's request (`Search`/`Scope` are `null`). The value stays in the URL and applies again on other data layouts.

## Items from a handler

For data of another shape (a summary, a ledger), pass the item type and a handler: `AddTable<TItem, THandler>(key, ...)`, `AddCards<TItem, THandler>(key, ...)` ([cards](cards.md#cards-from-a-handler)) or `AddPartial<TItem, THandler>(key, partialName, ...)`. The handler is registered as a scoped service unless it already is.

**EF Core:** implement `IEfCoreDataLayoutHandler<TEntity, TItem>` (namespace `StellarAdmin.EntityFrameworkCore`). It receives an untracked query with the search and scope already applied, and reshapes it:

```csharp
public record DestinationRevenue(int DestinationId, string Destination, int Bookings, decimal Revenue);

public class DestinationRevenueHandler : IEfCoreDataLayoutHandler<Booking, DestinationRevenue>
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

**Any resource:** implement `IResourceDataLayoutHandler<TItem>` (namespace `StellarAdmin.Resources`), whose `ListAsync(ResourceListRequest request, CancellationToken cancellationToken)` loads the items itself and applies `request.Search` and `request.Scope` itself. A custom-data-source resource takes only this kind (an EF handler doesn't compile there); an EF Core resource takes either.

```csharp
index.AddTable<DestinationRevenue, DestinationRevenueHandler>("revenue", table =>
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
index.AddPartial<DestinationRevenue, DestinationRevenueHandler>("revenue-cards", "_RevenueCards");
```

- A handler table or handler cards sort and page through the handler: `request.Sort.Field` is the column's property name, `request.Sort.Selector` its sort expression over the row type, `request.Paging` the page. Return the total of all items so the pager counts pages.
- A handler partial's request has no `Sort` or `Paging`; return every item. Its model is `ResourceDataLayoutModel<TItem>`.
- Handler tables have no row details and no inline editing.

## Linking items to records

Handler items don't link by default: no row actions in a table or on cards, and `GetViewUrl`/`GetEditUrl` return `null` in a partial. Call one of these on the handler table, cards or partial builder:

- `HasRecordKey(item => item.BookingId)`: each item is one of this resource's records. Table rows and cards get the resource's view, edit and delete actions (filtered by `RowActions`); a partial's `GetViewUrl`/`GetEditUrl` return the record's pages.
- `LinkTo<Destination>(item => item.DestinationId)`: each item links to another resource's view page (table rows and cards get a view action; a partial's `GetViewUrl` returns it, `GetEditUrl` stays `null`). The other resource needs `AllowView`; without it the items don't link and a configuration problem is reported. Users not authorized for that resource see no link.

A key function that returns `null` leaves that item unlinked (a totals row). Calling both `HasRecordKey` and `LinkTo` on one data layout throws `InvalidOperationException`.
