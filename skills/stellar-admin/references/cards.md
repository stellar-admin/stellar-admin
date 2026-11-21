# Cards

`index.AddCards(key, cards => ...)` adds a data layout that shows each record as a card in a grid instead of a table row. Cards sit beside tables and partial views in the index's [data layouts](layouts.md), and share their search and scopes.

## Contents

- [Adding cards](#adding-cards)
- [Parts of a card](#parts-of-a-card)
- [Stats and fields](#stats-and-fields)
- [Arrangement, image shape and actions](#arrangement-image-shape-and-actions)
- [Row actions](#row-actions)
- [Sorting and paging](#sorting-and-paging)
- [Cards from a handler](#cards-from-a-handler)

## Adding cards

```csharp
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;

resource.Index(index =>
{
    index.AddCards("trips", cards =>
    {
        cards.DefaultSortBy(trip => trip.Title);
        cards.EnablePaging(paging => paging.PageSize = 12);
        cards.Card(card =>
        {
            card.Image(trip => trip.CoverImageUrl);
            card.Title(trip => trip.Title);
            card.Description(trip => trip.Summary);
            card.Badge(trip => trip.Difficulty);
            card.Stats(stats =>
            {
                stats.Add(trip => trip.DurationDays);
                stats.Add(trip => trip.BasePrice, stat => stat.UseDisplay<NumberDisplay>(number => number.Format = "C0"));
            });
            card.Fields(fields =>
            {
                fields.Add(trip => trip.Destination);
                fields.Add(trip => trip.LeadGuide);
            });
        });
    });
});
```

- `AddCards(key)` returns the cards builder; `AddCards(key, configure)` returns the index builder. Keys follow the data layout rules ([data layouts](layouts.md#adding-data-layouts)).
- A card shows only the parts you configure, except the title.
- Cards fill as many columns as fit the page.

## Parts of a card

| `card.` member | Value |
| --- | --- |
| `Title(item => ...)` | A `string?`. Links to the view page. Defaults to the resource's title (`UseTitle` or the convention), else the record's key |
| `Description(item => ...)` | A `string?` below the title |
| `Image(item => url)` | A `string?` image URL; a record without one shows a placeholder. `Image(url, image => image.Shape = ...)` sets the shape |
| `Badge(item => ...)` | Any property, shown with `BadgeDisplay` beside the title, or on the image of a vertical or overlay card. An empty value shows no badge. `Badge(field, badge => badge.UseDisplay<TDisplay>(...))` changes the display |
| `Stats(stats => ...)` | Short values in a strip below the title, at most four |
| `Fields(fields => ...)` | Labels and values below the stats |
| `Arrangement`, `ActionsPlacement` | See [below](#arrangement-image-shape-and-actions) |

- Title and description are plain text, truncated with the full text on hover.
- A badge whose color depends on the value needs a custom display.

## Stats and fields

`stats.Add(field)` and `fields.Add(field)` return the entry builder; `Add(field, configure)` returns the stats or fields builder. Entry members:

| Member | Default |
| --- | --- |
| `Title` | `[Display(Name)]`, else the property name split into words |
| `EmptyText` | `[DisplayFormat(NullDisplayText)]`, else a dash |
| `UseDisplay<TDisplay>()`, `UseDisplay<TDisplay>(configure)` | The display a table column would use for the property ([columns](columns.md)) |

`fields.Layout = ResourceCardFieldLayout.Grid` shows the fields in two columns with each label above its value; the default `Rows` puts each field on its own row, label at the start and value at the end.

## Arrangement, image shape and actions

The enums are in `StellarAdmin.Resources`.

- `card.Arrangement` (`ResourceCardArrangement`): `Vertical` (default, image above the content), `Horizontal` (image beside the content; falls back to above it when narrow), `Overlay` (title and description over the bottom of the image, the rest below). An overlay card without an image is shown vertical, and a configuration problem is reported.
- `image.Shape` (`ResourceCardImageShape`): `Widescreen` (16:9, default), `Banner` (21:9), `Landscape` (4:3), `Square`.
- `card.ActionsPlacement` (`ResourceCardActionsPlacement`): `Menu` (default, a ⋯ menu with View, Edit and Delete in the header, on the image for overlay cards) or `Footer` (an Edit button and a ⋯ menu with Delete in the footer; the card opens the view page).

```csharp
cards.Card(card =>
{
    card.Arrangement = ResourceCardArrangement.Horizontal;
    card.ActionsPlacement = ResourceCardActionsPlacement.Footer;
    card.Image(trip => trip.CoverImageUrl, image => image.Shape = ResourceCardImageShape.Square);
    card.Title(trip => trip.Title);
});
```

## Row actions

Cards show the view, edit and delete actions the resource allows. With a view page, the title links to it and clicking the card opens it. `cards.RowActions(actions => actions.Delete = false)` hides an action on the cards only; `View`, `Edit` and `Delete` can be set, and `true` can't show an action the resource doesn't allow. Cards have no inline editing; they link to the edit page.

## Sorting and paging

- Cards have no sort controls. `cards.DefaultSortBy(item => ...)` or `DefaultSortByDescending(...)` sets their order; without one they come in the data source's order. The property doesn't need to be a sortable column.
- `cards.EnablePaging()` shows 25 cards per page; `EnablePaging(paging => paging.PageSize = 12)` sets another size. The pager below the cards has Previous and Next links and "Page 2 of 6". Cards have no page-size picker and no `PageSizes`. A `PageSize` of 0 or less uses 25 and reports a configuration problem.
- Without paging, every matching record is shown.
- Sort and paging belong to each cards data layout and reset when the user switches data layouts.

## Cards from a handler

`AddCards<TItem, THandler>(key, cards => ...)` shows items a handler loads; the handler is written as for a handler table ([data layouts](layouts.md#items-from-a-handler)). The card's parts are configured over `TItem`.

```csharp
index.AddCards<DestinationRevenue, DestinationRevenueHandler>("destinations", cards =>
{
    cards.DefaultSortByDescending(item => item.Revenue);
    cards.EnablePaging();
    cards.LinkTo<Destination>(item => item.DestinationId);
    cards.Card(card =>
    {
        card.Title(item => item.Destination);
        card.Stats(stats => stats.Add(item => item.Bookings));
    });
});
```

- The panel doesn't sort or page handler items: the handler applies `request.Sort` (the default sort) and `request.Paging`, and returns that page with the total count.
- `HasRecordKey(item => ...)` makes each item one of this resource's records, with its row actions; `LinkTo<TOther>(item => ...)` links each card to another resource's view page ([linking items](layouts.md#linking-items-to-records)).
- Handler items have no resource title. Handler cards without `card.Title(...)`, `HasRecordKey` or `LinkTo` have no title, and a configuration problem is reported.
