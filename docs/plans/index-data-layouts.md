# Index data layouts

Status: proposed (2026-11-20). Direction agreed with Jerrie in conversation (option B, the "data layout" name, renamed types, a development-mode hint for an empty index); the API below awaits his review. Work stage by stage and pause at every checkpoint.

The index page has two levels: the page (title and labels, search, scopes, widgets, the switcher) and one or more *data layouts* (a table, cards or a partial view) that lay out its records. Today the API hides that: `Index(index => ...)` forwards table members (`Columns`, `DefaultSortBy`, `EnablePaging`, `EnableRowDetails`) to an implicit table, while `Index(shared => ..., layouts => ...)` moves them into `AddTable`. The same call lives in two places, adding a second data layout means rewriting the whole `Index()` call, and every table docs page has to pick a form. Filament has the same split: columns belong to the resource's table, and the list page embeds it.

- Builds on [index-layouts](index-layouts.md) and [cards-layout](cards-layout.md); replaces the cards plan's stage 3 (docs), which moves to stage 4 here.

## Scope

In scope: one `Index()` builder with page settings and `Add*` data layouts; the `DataLayout` type renames; the empty index page; migrating the samples, tests, screenshot apps, conventions, consumer skills, agent evals and website docs; reorganising the website's index docs.

Nothing outside this repo and the website uses the API, and the product is unreleased, so only our own code and docs migrate.

Not changed: the `?layout=` query parameter (Jerrie 2026-11-20), the switcher's behaviour, the handler contract, `Ignore*`, and types where "layout" means something else (`FormSectionLayout`, `ResourceCardFieldLayout`, the row-detail panel layouts, the editor layouts). Archived plans keep their old code.

## Proposed API

One `Index()` overload. The index builder has the page settings and adds the data layouts; layout settings live only on the layout's builder.

```cs
resource.Index(index =>
{
    // The page
    index.Title = "Bookings";
    index.EnableSearch(term => booking => booking.Reference.Contains(term));
    index.EnableScopes(scopes => scopes.Add("pending", "Pending", booking => booking.Status == BookingStatus.Pending));

    // The data layouts; the first is the default
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
    index.AddCards("cards", cards => cards.Card(card => card.Description(booking => booking.Traveller)));
    index.AddPartial("map", "_BookingMap");
    index.AddTable<BookingTotal, BookingTotalsHandler>("totals", table => ...);
});
```

- `AddTable`, `AddCards` and `AddPartial`, with their handler overloads, move from `ResourceLayoutsBuilder` onto `ResourceIndexBuilder` unchanged, and from `EfCoreResourceLayoutsBuilder` onto `EfCoreResourceIndexBuilder`. They keep the `Add*` verb: each registers a data layout (options-builders convention).
- The key stays a required argument, also for a single table: it is the data layout's identity in the URL, and one set of overloads per kind keeps the API small (Jerrie 2026-11-20).
- Removed: the two-callback `Index()` overloads, `ResourceSharedIndexBuilder`, `EfCoreResourceSharedIndexBuilder`, `ResourceLayoutsBuilder`, `EfCoreResourceLayoutsBuilder`, the table members forwarded on `ResourceIndexBuilder` and `EfCoreResourceIndexBuilder`, and the internal implicit table (`ImplicitTableKey`, `DeclareLayouts`).
- An index with no data layouts shows no records: the development-mode hint below, and an empty page otherwise.
- Search, scopes and future filters stay on the index, across its data layouts, as today; a data layout opts out with `Ignore*` (Jerrie 2026-11-20).

### Inline editing

Inline editing and inline create move from `edit.UseInlineRow()` and `create.UseInlineRow()` to the table, as `table.EnableInlineEditing(...)` and `table.EnableInlineCreate(...)` (Jerrie 2026-11-20). Both are a table's: the editable fields match its columns, and `AfterSave` and `DimOtherRows` describe it. Only tables of the resource's rows have them, not handler tables.

The edit and create pages stay available when a table edits inline (Jerrie 2026-11-20): other data layouts, such as cards, link to them. The inline table edits in its rows instead of linking.

### Renames

Public types (internal names follow):

| Now | Becomes |
| --- | --- |
| `IResourceLayoutHandler<TItem>` | `IResourceDataLayoutHandler<TItem>` |
| `IEfCoreLayoutHandler<TEntity, TItem>` | `IEfCoreDataLayoutHandler<TEntity, TItem>` |
| `ResourceLayoutBuilderBase` | `ResourceDataLayoutBuilderBase` |
| `ResourceTableLayoutBuilderBase`, `ResourceTableLayoutBuilder`, `ResourceHandlerTableLayoutBuilder` | `ResourceTableDataLayoutBuilderBase`, `ResourceTableDataLayoutBuilder`, `ResourceHandlerTableDataLayoutBuilder` |
| `ResourceCardsLayoutBuilder`, `ResourceHandlerCardsLayoutBuilder` | `ResourceCardsDataLayoutBuilder`, `ResourceHandlerCardsDataLayoutBuilder` |
| `ResourcePartialLayoutBuilder`, `ResourceHandlerPartialLayoutBuilder` | `ResourcePartialDataLayoutBuilder`, `ResourceHandlerPartialDataLayoutBuilder` |
| `EfCoreResourceTableLayoutBuilder` | `EfCoreResourceTableDataLayoutBuilder` |
| `ResourceLayoutOptions`, `ResourceTableLayoutOptions`, `ResourceCardsLayoutOptions`, `ResourcePartialLayoutOptions` | `ResourceDataLayoutOptions`, `ResourceTableDataLayoutOptions`, `ResourceCardsDataLayoutOptions`, `ResourcePartialDataLayoutOptions` |
| `ResourceLayoutModel`, `ResourceIndexLayoutViewModel`, `ResourceIndexPartialLayoutViewModel` | `ResourceDataLayoutModel`, `ResourceIndexDataLayoutViewModel`, `ResourceIndexPartialDataLayoutViewModel` |

Our prose (docs, XML comments, configuration problem messages, the consumer skill) says "data layout", never "layout" alone.

## Stages

1. **Renames.** The table above, mechanically, with the PublicAPI files, XML docs, messages and our own views (`@model` of the partial data layout views in the samples and tests). No behaviour change. Checkpoint.
2. **One index builder.** The API above in StellarAdmin and EF, including inline editing on the table, then migrating every caller: the home page's setup steps (`src/StellarAdmin/Home/SetupSteps.cs`), the playground, the screenshot apps (`tools/StellarAdmin.Cli/Docs/Screenshots`), scenario tests, unit and integration tests, the options-builders convention and design docs, the consumer skills (`skills/stellar-admin`, `skills/stellar-admin-setup`), the development skills and the agent evals (`build/agent-evals`). About 160 files. Audit the configuration problem messages and XML docs that speak of the index's columns, the implicit table or inline rows on the edit page. Checkpoint.
3. **The empty index.** In development, an index with no data layouts shows a hint saying how to add one; elsewhere it is empty. Design the hint with Jerrie first. Checkpoint.
4. **Docs.** Migrate the code in the 16 website pages that configure the index, reorganise the index docs around page and data layouts (sidebar grouping to decide), write the cards docs (the cards plan's stage 3), and retitle "Layout data" (for example "Data from a handler"), and update the inline editing, configuration problems and template override pages. Regenerate the generated consumer skill references (see `docs/repos/skills.md`) and edit the hand-written ones to match. Checkpoint.

## Open questions

- Stage 4: the sidebar grouping, either flat groups (URLs unchanged) or nested folders (URLs change, needing redirects). The flat sketch from the conversation: **Index Pages** (Overview: the page and its data layouts, title and labels, row actions; Search; Scopes; Data layouts: several, keys, the switcher, title and icon, `Ignore*`, partial views as a section; Data from a handler), **Tables** (a short Tables page; Columns; Sorting and paging; Row details; Inline editing), **Cards** (Cards: the parts; Card arrangement: arrangements, image shapes, actions placement, sort and paging). Sorting and paging stays table-centric, and the cards pages link to it.
- Stage 4: Cards as one page or two (the parts, then the arrangement).
- Stage 4: the `layouts-switcher` screenshot and its example switch to built-in cards; the partial view example gets something less card-like.
