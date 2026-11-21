# Cards layout

Status: in progress (2026-11-19). Visuals and API agreed with Jerrie in conversation; stages 1 and 2 built and reviewed; the docs moved to index-data-layouts. Work stage by stage and pause at every checkpoint.

A built-in Cards layout for the index page: `layouts.AddCards(key)` shows each record as a card in a grid, composed from configuration. Anything beyond these options is a partial layout (`AddPartial`).

- Prototype: `sandbox/html/card-layout.html`, the approved visuals: the parts, each option once on the same record, the fixed behaviour and narrow screens.
- Builds on [index-layouts](index-layouts.md): the switcher, keys, URL state, shared scopes and search, `Ignore*`, the list card.

## Scope

In scope: cards of the resource's own rows, with a default sort, paging and the resource's row actions.

Later, each its own stage or plan:

- Handler cards, `AddCards<TItem, THandler>(key)` with `HasRecordKey` and `LinkTo<TOther>` as on handler tables (stage 2 below).
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
        card.Description(d => d.Region);          // none unless set
        card.Image(d => d.PhotoUrl, image =>      // none unless set
            image.Shape = ResourceCardImageShape.Landscape);      // Widescreen default
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
            fields.Layout = ResourceCardFieldLayout.Grid;             // Rows default
            fields.Add(d => d.Season);
            fields.Add(d => d.Guide);
        });
        card.Arrangement = ResourceCardArrangement.Overlay;           // Vertical default
        card.ActionsPlacement = ResourceCardActionsPlacement.Footer;  // Menu default
    });
});
```

- `AddCards(key)` and `AddCards(key, configure)` on `ResourceLayoutsBuilder<TResource>` and `EfCoreResourceLayoutsBuilder<TEntity>`, returning `ResourceCardsLayoutBuilder<TResource>` (from `ResourceLayoutBuilderBase`, so `Title`, `Icon`, `IgnoreScopes` and `IgnoreSearch` come with it).
- A card shows only the parts its `Card(...)` sets. The one fallback is the title: without `card.Title(...)` it is the resource's title (`UseTitle`, the record's name across the admin panel), then the record's key. Nothing comes from `Reference`. `Card(...)` has its own scope, which keeps the card's title apart from the layout's switcher `Title`.
- Stats and fields have their own builders (`ResourceCardStatsBuilder<TResource>`, `ResourceCardFieldsBuilder<TResource>`, each with `Add`); the field layout is `Layout` on the fields builder. Stats and fields share one entry builder for a single value: `Title`, `EmptyText`, `UseDisplay<TDisplay>()` and `UseDisplay<TDisplay>(configure)`. The badge has its own `ResourceCardBadgeBuilder` with only the two `UseDisplay` overloads, since it shows no label and an empty badge is hidden. Without `UseDisplay`, a stat or field renders the compact display template its property picks, as a table cell does; the badge uses `BadgeDisplay`. A display with a handler is a configuration problem, as on columns. Badge colour per value is the app's own display, whose template switches on the value and renders a badge.
- More than 4 stats is a reported configuration problem; the card shows the first four. `Image`'s configure is optional. `Overlay` without an image is a reported configuration problem; the card shows as `Vertical`.
- `cards.EnablePaging` returns the cards' own `ResourceCardsPagingBuilder<TResource>` (`PageSize` only), not the table's. Cards have no page-size picker, so they ignore a `pageSize` query value and their pager links don't carry one.
- The title and description are plain text (`ToString()`); a `UseDisplay` for them can come later if needed.
- New enums, one per setting: `ResourceCardImageShape` (Widescreen, Banner, Landscape, Square), `ResourceCardFieldLayout` (Rows, Grid), `ResourceCardArrangement` (Vertical, Horizontal, Overlay: the image above the content, beside it, or under the title and description), `ResourceCardActionsPlacement` (Menu, Footer).

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

1. **Cards of the resource's rows.** Options (`ResourceCardsLayoutOptions` and the card's part options), the builders and enums, `AddCards` on both layouts builders, the controller (default sort and paging as tables), the view and its CSS in `@layer sa.components` extracted from the prototype, using real `sa-*` components in the view. Tests for the builders, the title from `UseTitle`, the stats limit, sort and paging. Playground: a Cards layout on an image-bearing resource. Checkpoint: Jerrie reviews the playground in the themes, light and dark.
2. **Handler cards.** `AddCards<TItem, THandler>(key)` and the `configure` overload on both layouts builders, with `HasRecordKey` and `LinkTo<TOther>`; the handler is the same `IResourceLayoutHandler<TItem>` as handler tables and partial layouts, and gets the sort and page. Checkpoint.
3. **Docs**, after both kinds of cards so they are written and screenshotted once. Website (`../website`): a Cards section in the layouts docs with screenshot placeholders, and handler cards in the layout data page; consumer skill `skills/stellar-admin/references/layouts.md`, removing cards from its Not supported line. Checkpoint. *Moved to [index-data-layouts](index-data-layouts.md) stage 4 (2026-11-20), so the docs are written once against the new API.*

## Progress

### Checkpoint decisions (Jerrie, 2026-11-19)

1. Pager: don't reuse `sa-data-grid-pager`; cards aren't a data grid. For now the cards view renders its own simple Previous/Next pager from the `sa-pagination` components (keeping the query: layout, scope, search, sort, page size). Follow-up after this plan: prototype a proper pager, perhaps a dedicated `sa-data-pager` the grid could share; evaluate then.
2. More than four stats and a display with a handler stay reported configuration problems, not startup errors. Agreed.
3. Title over the image uses the image's shape (widescreen default), as built; the prototype's 4:3 overlay default is dropped. Agreed after section 9 of the prototype compared the two.
4. Stats and fields must not share a builder. Split `ResourceCardEntriesBuilder` into `ResourceCardStatsBuilder<TResource>` and `ResourceCardFieldsBuilder<TResource>`, and move the field layout from `card.FieldLayout` to `fields.Layout` (remove `ResourceCardBuilder.FieldLayout`; the option can stay `ResourceCardOptions.FieldLayout`). `ResourceCardEntryBuilder` stays shared for one value (badge, stat or field).

Second review (Jerrie, 2026-11-19):

5. The badge gets its own builder; the shared entry builder's `Title` and `EmptyText` did nothing on it.
6. Option C: `card.Orientation` and `image.Placement` merge into `card.Arrangement` (`ResourceCardArrangement`: Vertical, Horizontal, Overlay), since the overlay places the title and description, not the image, and horizontal cards can't have it. Overlay without an image is a reported configuration problem (a warning), shown as Vertical.
7. Cards get their own paging builder, `ResourceCardsPagingBuilder<TResource>`.
8. Nothing is taken from the resource except the title (option a): no description or image from `Reference`. This removes the need to restate the reference's image URL to configure it, and the need for "off" switches.
9. The title and description stay plain text; `UseDisplay` for them later if it becomes an issue.

Decisions 1 and 4 done 2026-11-19: `ResourceCardStatsBuilder` and `ResourceCardFieldsBuilder` (with `Layout`) replace `ResourceCardEntriesBuilder`, and `ResourceCardBuilder.FieldLayout` is gone. The cards view's pager is `sa-pagination` with Previous and Next links (each only when that page exists) around a "Page 2 of 13" status, shown only with more than one page; the links keep layout, scope, search, sort and page size. Verified: StellarAdmin.Tests 396, StellarAdmin.IntegrationTests 800, StellarAdmin.EntityFrameworkCore.IntegrationTests 183 pass; the playground's Products `built-in-cards` page 2 renders both links with the layout kept.

Decisions 5 to 8 done 2026-11-19: `ResourceCardBadgeBuilder`; `card.Arrangement` with `ResourceCardArrangement` (the view marks cards and the grid with `data-arrangement`), and `CardOverlayWithoutImage = 1122` reported as a warning; `ResourceCardsPagingBuilder` (the table paging builder is back to its original form); `CardsLayoutCheck` fills only the title. Verified: StellarAdmin.Tests 398, StellarAdmin.IntegrationTests 800, StellarAdmin.EntityFrameworkCore.IntegrationTests 183 pass; the playground builds and its Destinations cards render.

Stages reordered 2026-11-19 (Jerrie): handler cards before the docs.

Next: the [index-data-layouts](index-data-layouts.md) plan, which takes over stage 3 (the docs) after the index API change. Jerrie reviewed stages 1 and 2 on 2026-11-20; another visual sweep of the cards follows that work. Stage 1 committed as 4f86669, stage 2 as a892b8e.

For the cards visual sweep:

- Card stats are sized by their labels, so in three columns at 1280px they truncate ("$3,…", "Duration (d…"). The docs screenshots use a 1120px window to avoid it.
- Overlay cards show a thin strip of card background below the image (visible in the card-arrangement overlay screenshot).

### Stage 1 (built 2026-11-19, checkpoint review pending)

- Options: `ResourceCardsLayoutOptions` (card, default sort, paging, row actions), `ResourceCardOptions`, `ResourceCardImageOptions`; badge, stats and fields are `FormDisplayFieldOptions`. Builders: `ResourceCardsLayoutBuilder`, `ResourceCardBuilder`, `ResourceCardImageBuilder`, `ResourceCardBadgeBuilder`, `ResourceCardStatsBuilder`, `ResourceCardFieldsBuilder`, `ResourceCardEntryBuilder`, `ResourceCardsPagingBuilder`; the four enums in `StellarAdmin.Resources`. `AddCards` on both layouts builders. `ResourceRowActionsBuilder` gained an internal constructor that targets a cards layout.
- `CardsLayoutCheck` fills the title from the resource's `UseTitle` (decision 8), so the list request loads it, keeps the first four stats, and makes an overlay card without an image vertical.
- Controller: cards take the row actions, paging and default sort; a `sortBy` naming the default's field sets the direction, any other is ignored. The view gets the layout through `ViewData` (`ViewDataKeys.IndexCards`), so `IResourceIndexPageViewModel` is unchanged. `GetKey` returns null for a resource without a key instead of throwing.
- View `_IndexCards.cshtml` with `sa-card`, `sa-dropdown-menu`, `sa-link-button` and `sa-empty`; values render through `ResourceCardRenderer`, which reuses the page summary's compact template rendering and gives each value its text for the `title`. CSS in `Client/css/client.css`.
- Playground: Destinations `cards` (photos, country under the name, paged 12) and Products `built-in-cards` (EF, no images, category badge, price and id stats, SKU and tags in a grid, footer actions).
- Tests: `ResourceCardsLayoutTests` (8, integration), `ResourceCardsLayoutBuilderTests` (6, unit), one EF test in `ResourceLayoutsTests`.
- Verified 2026-11-19: StellarAdmin.Tests 395, StellarAdmin.IntegrationTests 800 and StellarAdmin.EntityFrameworkCore.IntegrationTests 183 pass. Screenshots of both playground layouts in the default light theme, desktop and 420px. Not yet checked: the other themes, dark mode, the open menus, horizontal and overlay cards in a browser (covered only by markup tests).

Differences from the plan (decided above where marked):

- More than four stats, and a display with a handler, are reported configuration problems rather than startup errors, as every other check here repairs and reports (decision 2).
- The CSS sits in the admin panel's stylesheet (`client.css`, `@layer components`) beside the other index styles, not in StellarAdmin.UI's `sa.components`.
- The pager is a simple Previous/Next `sa-pagination`, not the data grid's pager (decision 1).
- The title over the image uses the image's shape (widescreen by default); the prototype's 4:3 for an overlay without a shape is dropped (decision 3).
- The hover lift applies only to a card whose title links somewhere.

### Stage 2 (built 2026-11-19, checkpoint review pending)

- `AddCards<TItem, THandler>(key)` and its `configure` overload on `ResourceLayoutsBuilder` and `EfCoreResourceLayoutsBuilder` (an EF handler or a resource handler, as on tables), returning `ResourceHandlerCardsLayoutBuilder<TResource, TItem>`: `Card(...)` over `TItem`, `DefaultSortBy`/`DefaultSortByDescending`, `EnablePaging` (the cards' paging builder), `HasRecordKey`, `LinkTo<TOther>`, `RowActions`, and the base's `Title`, `Icon`, `IgnoreScopes`, `IgnoreSearch`.
- `ResourceCardsLayoutOptions` has the handler's loader, the same one handler tables use. The controller treats a cards layout with a handler as it does a handler table: the handler gets the scope, search, sort and page, only record cards get row actions, and `LinkTo` cards link to the other resource's view page.
- Handler cards don't take the resource's `UseTitle`, which is over the resource and not the item. Without `card.Title(...)` a card shows its record or link key; with neither, the cards have no title and it's a reported configuration problem (`UntitledHandlerCards`, Jerrie 2026-11-19).
- Playground: Products `price-check-cards` (record cards with row actions, paged) and `category-cards` (linking to categories).
- Tests: three integration tests in `ResourceCardsLayoutTests` (sort, paging and request; record key; link), one EF test in `ResourceLayoutsTests`, two unit tests for the title warning.
- Verified 2026-11-19: StellarAdmin.Tests 400, StellarAdmin.IntegrationTests 803 and StellarAdmin.EntityFrameworkCore.IntegrationTests 184 pass; both playground layouts render (default light theme, desktop) with no new configuration problems.
