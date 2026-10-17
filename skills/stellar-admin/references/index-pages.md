# Index pages

The index page lists a resource's records in a grid. Everything is configured in `resource.Index(index => ...)`. Columns are in [columns](columns.md), and editing and adding in the grid's rows in [inline editing](inline-editing.md).

## Contents

- [Title and row action labels](#title-and-row-action-labels)
- [Search](#search)
- [Scopes (filter tabs)](#scopes-filter-tabs)
- [Sorting](#sorting)
- [Paging](#paging)
- [Row details](#row-details)
- [Custom data sources](#custom-data-sources)

```csharp
resource.Index(index =>
{
    index.Title = "All bookings";
    index.Columns(columns => { /* see columns.md */ });
    index.EnableSearch(term => booking => booking.Reference.Contains(term), search => search.Placeholder = "Search by reference");
    index.EnableScopes(scopes => { /* ... */ });
    index.DefaultSortBy(booking => booking.DepartureDate);
    index.EnablePaging(paging => paging.PageSize = 10);
});
```

Search, scope tabs, sorting and paging all update the grid in place, keep each other's state in the URL, and reset to page 1 when the search or scope changes.

## Title and row action labels

| Property | Default |
| --- | --- |
| `index.Title` | The plural label |
| `index.CreateLabel` | `Create` |
| `index.EditLabel`, `index.ViewLabel`, `index.DeleteLabel` | `Edit {singular}`, `View {singular}`, `Delete {singular}` (row action labels) |

## Search

**EF Core:** pass a predicate factory. It receives the trimmed search text and returns a predicate, and it must translate through the provider:

```csharp
index.EnableSearch(term => booking =>
    booking.Reference.Contains(term) || booking.Customer!.Name.Contains(term));
```

- `Contains` follows the database's collation: case-sensitive on SQLite and on case-sensitive collations. For case-insensitive matching there, lower-case both sides: `term => { var lowered = term.ToLower(); return booking => booking.Reference.ToLower().Contains(lowered); }`.
- Reference paths such as `booking.Customer!.Name` translate to joins.
- Set the placeholder with the second argument, `search => search.Placeholder = "..."`. The default is `Search {plural label}...`.
- The search box appears in the page header. Typing searches after a short pause.

**Custom data sources:** call `index.EnableSearch()` (or with a placeholder callback). The text arrives as `ResourceListRequest.Search`; the data source decides what matches.

## Scopes (filter tabs)

Scopes are named tabs above the grid. Only one is active at a time.

**EF Core:** the third argument is the filter. A scope without one shows every record:

```csharp
index.EnableScopes(scopes =>
{
    scopes.Add("all", "All");
    scopes.Add("pending", "Pending", booking => booking.Status == BookingStatus.Pending);
    scopes.Add("confirmed", "Confirmed", booking => booking.Status == BookingStatus.Confirmed);
    scopes.Add("cancelled", "Cancelled", booking => booking.Status == BookingStatus.Cancelled);
    scopes.DefaultScope = "all";
});
```

- `Add(id, title, predicate)`: the id is the URL value (`?scope=pending`), the title is the tab text. The id is not parsed; filter text in it does nothing.
- `DefaultScope` selects the tab used when the URL names none or an unknown id. Without it, the list is unfiltered until a tab is chosen, so set it to the unfiltered scope.
- The scope predicate combines with search.
- Scopes are navigation, not authorization: a user can request any scope. Restrict records with EF global query filters or a custom data source.

**Custom data sources:** `scopes.Add("pending", "Pending")` without a predicate; the selected id arrives as `ResourceListRequest.Scope`.

## Sorting

Opt each column in with `Sortable()` and optionally set a default:

```csharp
index.Columns(columns =>
{
    columns.Add(booking => booking.DepartureDate, column => column.Sortable());
    columns.Add(booking => booking.Total, column => column.Sortable());
    columns.Add(booking => booking.Customer!.Name, column =>
    {
        column.Title = "Customer";
        column.Sortable(booking => booking.Customer!.Name.ToLower());   // sort by another expression
    });
});
index.DefaultSortBy(booking => booking.DepartureDate);               // ascending: soonest first
// or index.DefaultSortByDescending(booking => booking.Total);
```

- The default sort must name a column that is sortable; otherwise options validation fails every request.
- Clicking a header toggles the direction.
- EF Core breaks ties with the primary key.

## Paging

```csharp
index.EnablePaging();                                    // 25 per page; 10, 25, 50, 100 to choose from
index.EnablePaging(paging =>
{
    paging.PageSize = 10;
    paging.PageSizes = [10, 20, 50];
});
```

`PageSize` must be one of `PageSizes` (default 10, 25, 50, 100); otherwise options validation fails every panel request with a 500. Set both when using another size: `paging.PageSize = 15; paging.PageSizes = [15, 30, 60];`. Without paging, every matching record is listed.

## Row details

Rows can expand to show more about a record. Each `EnableRowDetails` call replaces the previous one.

```csharp
// A partial rendered with the index, with the row's entity as its model
index.EnableRowDetails("_BookingSummary");

// EF Core: a projection loaded when the row first expands
index.EnableRowDetails("_BookingPricing", booking => new BookingPricing(booking.Total, booking.Travellers));

// A model loaded on expand by a handler implementing IResourceRowDetailHandler<BookingNotes>
index.EnableRowDetails<BookingNotes, BookingNotesHandler>("_BookingNotes");

// Two or more panels become tabs
index.EnableRowDetails(panels =>
{
    panels.Add("Summary", "_BookingSummary");
    panels.Add<BookingNotes, BookingNotesHandler>("Notes", "_BookingNotes");
});
```

- Put the partials in `Areas/StellarAdmin/Views/{slug}/` or `Areas/StellarAdmin/Views/Shared/`, with an area `_ViewImports.cshtml` ([customization](customization.md#view-imports)).
- The callback overloads configure presentation: `details => { details.Emphasis = DataGridRowDetailEmphasis.Rail; details.ExpandMode = DataGridRowDetailExpandMode.Single; details.RowClick = true; }`. Other settings: `Animate`, `Divider` (`DataGridRowDetailDivider`), `Inset`, `Toggle`. The enums are in `StellarAdmin.UI`.
- Row details are for per-row information. Content for the whole page, such as a summary above the grid, belongs in an index override ([customization](customization.md#add-content-to-a-generated-page)).

## Custom data sources

For a resource registered with `AddResource<T>`, `EnableSearch()` and `scopes.Add(id, title)` only configure the UI. The data source receives `ResourceListRequest` with `Search`, `Scope`, `Sort` (`Field` is the column's property name) and `Paging`, applies them before paging, and returns the total count of matches. See [resources](resources.md#custom-data-sources).
