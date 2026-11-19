# Cards layout

Status: proposed (2026-11-19). Visuals and API agreed with Jerrie in conversation; stages not started. Work stage by stage and pause at every checkpoint.

A built-in Cards layout for the index page: `layouts.AddCards(key)` shows each record as a card in a grid, composed from configuration. Anything beyond these options is a partial layout (`AddPartial`).

- Prototype: `sandbox/html/card-layout.html` (untracked), the approved visuals: the parts, each option once on the same record, the fixed behaviour and narrow screens.
- Builds on [index-layouts](index-layouts.md): the switcher, keys, URL state, shared scopes and search, `Ignore*`, the list card.

## Scope

In scope: cards of the resource's own rows, with a default sort, paging and the resource's row actions.

Later, each its own stage or plan:

- Handler cards, `AddCards<TItem, THandler>(key)` with `HasRecordKey` and `LinkTo<TOther>` as on handler tables (stage 3 below).
- A sort menu fed by a `cards.Sortable(...)` list (see Sorting).
- The selection checkbox: it waits for bulk actions, which the product doesn't have yet.
- A built-in value-to-variant mapping for badges; it belongs on `BadgeDisplay` and would serve tables too.

## Proposed API

```csharp
layouts.AddCards("cards", cards =>
{
    cards.Title = "Cards";                        // the switcher, as on tables
    cards.Icon = "layout-grid";
    cards.DefaultSortBy(d => d.Name);             // and DefaultSortByDescending
    cards.EnablePaging(paging => paging.PageSize = 24);
    cards.RowActions(actions => actions.Delete = false);

    cards.Card(card =>
    {
        card.Title(d => d.Name);                  // default: the resource's UseTitle
        card.Description(d => d.Region);          // default: Reference UseDescription
        card.Image(d => d.PhotoUrl, image =>      // default: Reference UseImage
        {
            image.Shape = ResourceCardImageShape.Landscape;       // Widescreen default
            image.Placement = ResourceCardImagePlacement.Overlay; // Top default
        });
        card.Badge(d => d.Status);                // BadgeDisplay unless UseDisplay says otherwise
        card.Stats(stats =>
        {
            stats.Add(d => d.TourCount);
            stats.Add(d => d.FromPrice, stat =>
            {
                stat.Title = "From";
                stat.UseDisplay<NumberDisplay>(number => number.Format = "C0");
            });
            stats.Add(d => d.Rating).UseDisplay<RatingDisplay>();  // the app's own display
        });
        card.Fields(fields =>
        {
            fields.Add(d => d.Season);
            fields.Add(d => d.Guide);
        });
        card.FieldLayout = ResourceCardFieldLayout.Grid;              // Rows default
        card.Orientation = ResourceCardOrientation.Horizontal;        // Vertical default
        card.ActionsPlacement = ResourceCardActionsPlacement.Footer;  // Menu default
    });
});
```

- `AddCards(key)` and `AddCards(key, configure)` on `ResourceLayoutsBuilder<TResource>` and `EfCoreResourceLayoutsBuilder<TEntity>`, returning `ResourceCardsLayoutBuilder<TResource>` (from `ResourceLayoutBuilderBase`, so `Title`, `Icon`, `IgnoreScopes` and `IgnoreSearch` come with it).
- With no `Card(...)`, a card shows the record's title, description and image from the resource's title and `Reference`, and no badge, stats or fields. `Card(...)` overrides parts; its own scope keeps the card's title apart from the layout's switcher `Title`.
- Badge, stats and fields share one entry builder: `Title`, `EmptyText`, `UseDisplay<TDisplay>()` and `UseDisplay<TDisplay>(configure)`. Without `UseDisplay`, a stat or field renders the compact display template its property picks, as a table cell does; the badge uses `BadgeDisplay`. A display with a handler is a configuration problem, as on columns. Badge colour per value is the app's own display, whose template switches on the value and renders a badge.
- More than 4 stats is a startup error. `Image`'s configure is optional.
- New enums, one per setting: `ResourceCardImageShape` (Widescreen, Banner, Landscape, Square), `ResourceCardImagePlacement` (Top, Overlay), `ResourceCardFieldLayout` (Rows, Grid), `ResourceCardOrientation` (Vertical, Horizontal), `ResourceCardActionsPlacement` (Menu, Footer).

### Sorting

Cards sort as tables do: the sort is in the URL (`sortBy`, `sortDirection`), and the default applies when the URL has none. `DefaultSortBy` records a sort named after its field, as a table's sort is. The later `Sortable(...)` list reads the same parameters and always includes the default, so an application that only sets a default keeps working and the menu shows it selected. Until then a cards layout ignores a `sortBy` that isn't its default. Handler cards will get the sort in their request, as handler tables do.

## Behaviour (fixed, from the prototype)

- The cards sit in the list card with the scopes, search and pager. The grid fills columns from a minimum width (16rem; 22rem with horizontal cards).
- The title links to the View page and its link covers the card; the menu and footer buttons sit above it. Keyboard focus on the link rings the whole card with the button focus treatment.
- Row actions are View, Edit and Delete, filtered by `RowActions` and the resource. Menu: a ⋯ menu in the header, or on the image with the title over it. Footer: Edit as a button and Delete in a ⋯ menu at the end of the footer, with no header menu.
- On a vertical card with an image, the badge sits on the image, keeping its variant's colours with a shadow; Outline and Ghost get a surface fill. Otherwise it sits beside the title.
- A record without an image gets a placeholder.
- Every value, label, title and description carries its full text in a `title`, so a value cut off with "…" shows in full on hover.
- When the grid is under 30rem wide (a container query, so narrow panels count too), horizontal cards stack with the image on top.

Where the prototype differs from the product: its ⋯ buttons are plain buttons (the product uses the dropdown menu); its outline badge with a status dot, the star rating and the "$" formatting are prototype markup, which the product gets from `BadgeDisplay` and the field displays (the star from an app's own display); its badge on the image always gets the surface fill (the product keeps the variant's colours, above); and its selection checkbox waits for bulk actions.

## Stages

1. **Cards of the resource's rows.** Options (`ResourceCardsLayoutOptions` and the card's part options), the builders and enums, `AddCards` on both layouts builders, the controller (default sort and paging as tables), the view and its CSS in `@layer sa.components` extracted from the prototype, using real `sa-*` components in the view. Tests for the builders, defaults from `UseTitle`/`Reference`, the stats limit, sort and paging. Playground: a Cards layout on an image-bearing resource. Checkpoint: Jerrie reviews the playground in the themes, light and dark.
2. **Docs.** Website (`../website`): a Cards section in the layouts docs with screenshot placeholders; consumer skill `skills/stellar-admin/references/layouts.md`, removing cards from its Not supported line. Checkpoint.
3. **Handler cards.** `AddCards<TItem, THandler>(key)` and the `configure` overload on both layouts builders, with `HasRecordKey` and `LinkTo<TOther>`; the handler gets the sort and page. Docs in the layout data page. Checkpoint.

## Progress

Nothing implemented yet.
