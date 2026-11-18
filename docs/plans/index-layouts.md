# Index layouts

Status: approved, stage 6 drafted (2026-11-18). API and stages agreed with Jerrie; stages 1 to 5 and the builder renames committed, stage 6 awaiting review. Work stage by stage (see Stages) and pause at every checkpoint.

Let an index page switch between layouts, with a switcher at the end of the controls row. A layout can show the resource's own rows or use a handler to load a different data shape (for example, revenue per month summarised from bookings).

- Prototype: `sandbox/html/index-views.html` (untracked). It shows switcher placements A and A2 and layouts whose data shape differs (the map uses `DestinationSummary`, the calendar `DepartureDay`, the chart `MonthRevenue`).
- Prior art: Avo `view_types` and ActiveAdmin `index as:` (same data, different layouts), Filament `contentGrid()`, and Nova lenses (the closest to a different data shape).
- Related: [index-page-refreshing](index-page-refreshing.md) (htmx swaps of the data area) and the filter bar from `explore/index-filters`.

## Scope

The first phase covers only additional tables and custom partials, each with the same or a different data shape. Those two exercise the whole shared API: the `Index` overload, keys, the switcher, URL state, shared scopes and search, `Ignore*`, per-layout sort and paging, and the handler for a different data shape.

Built-in board, map, card and calendar layouts come later, each as its own piece of work with its own prototype and tag helpers. Each adds one `Add*` method to the layouts collection without changing the first phase's API. Until then, applications can build them as partials.

## Proposed API (first phase)

`resource.Index` gets a second overload. The existing one-argument overload is the simple route and stays as it is: one table, no switcher. The two-argument overload is the layouts route: the first lambda holds the settings shared by every layout, and the second adds the layouts. The overload called decides the route at registration.

```csharp
// Simple route: unchanged
resource.Index(index =>
{
    index.Columns(columns => ...);
    index.DefaultSortBy(booking => booking.DepartsOn);
    index.EnablePaging();
    index.EnableScopes(scopes => ...);
    index.EnableSearch(term => booking => ..., search => ...);
});

// Layouts route
resource.Index(
    index =>
    {
        // Shared by every layout; table members such as Columns don't exist here
        index.EnableScopes(scopes => ...);
        index.EnableSearch(term => booking => ..., search => ...);
    },
    layouts =>
    {
        // Same data shape; the first layout is the default
        layouts.AddTable("bookings", table =>
        {
            table.Title = "Bookings";
            table.Icon = "table";
            table.Columns(columns => ...);
            table.DefaultSortBy(booking => booking.DepartsOn);
            table.EnablePaging();
        });
        layouts.AddTable("finance", table =>
        {
            table.Columns(columns => ...);
            table.EnablePaging(paging => paging.PageSize = 50);
            table.RowActions(actions => actions.Delete = false);
        });
        layouts.AddPartial("calendar", "_BookingCalendar");

        // Different data shape
        layouts.AddTable<BookingLedgerRow, BookingLedgerHandler>("ledger", table =>
        {
            table.HasRecordKey(row => row.BookingId);  // links to the booking, with row actions
            table.Columns(columns => ...);
        });
        layouts.AddTable<DestinationSummary, DestinationSummaryHandler>("destinations", table =>
        {
            table.LinkTo<Destination>(summary => summary.DestinationId);
            table.Columns(columns => ...);
        });
        layouts.AddPartial<MonthRevenue, RevenueChartHandler>("revenue-chart", "_RevenueChart", chart =>
        {
            chart.IgnoreSearch();
            chart.IgnoreScopes();
        });
    }
);
```

The builders:

```csharp
// On the resource builder
Index(Action<ResourceIndexBuilder<TResource>> configure);
Index(
    Action<ResourceSharedIndexBuilder<TResource>> configure,
    Action<ResourceLayoutsBuilder<TResource>> layouts
);

// ResourceSharedIndexBuilder<TResource>: title, scopes, search, filters, widgets, labels
// ResourceIndexBuilder<TResource> : ResourceSharedIndexBuilder<TResource>, adding the table members
//     (columns, sort, paging, row details, inline editing)

// ResourceLayoutsBuilder<TResource>
layouts.AddTable(string key);                                   // returns the table builder
layouts.AddTable(string key, Action<...> configure);            // returns the layouts builder
layouts.AddTable<TItem, THandler>(string key, ...);
layouts.AddPartial(string key, string partialName, ...);
layouts.AddPartial<TItem, THandler>(string key, string partialName, ...);

// Every layout builder
layout.Title = "Finance";
layout.Icon = "receipt";
layout.IgnoreSearch();
layout.IgnoreScopes();
layout.IgnoreFilters();                                         // once the filter bar exists

// Table layouts: the table members of ResourceIndexBuilder, plus
table.RowActions(actions => { actions.View = ...; actions.Edit = ...; actions.Delete = ...; });
table.HasRecordKey(item => item.BookingId);                     // different data shape only
table.LinkTo<TOther>(item => item.DestinationId);               // different data shape only
```

A different data shape comes from a handler, registered like `IResourceRowDetailHandler<TModel>`. There are two kinds (see Decisions):

```csharp
// StellarAdmin: loads its own data from the request values
public interface IResourceLayoutHandler<TItem>
{
    Task<ResourceListResult<TItem>> ListAsync(ResourceListRequest request, CancellationToken cancellationToken);
}

// StellarAdmin.EntityFrameworkCore: reshapes the already narrowed query
public interface IEfCoreLayoutHandler<TResource, TItem>
{
    Task<ResourceListResult<TItem>> ListAsync(
        IQueryable<TResource> resources,
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
```

Handlers receive the existing `ResourceListRequest` (scope, search, sort, paging, keys), which gains a `Layout` property and later filters; later layouts can add `init` properties (a map's visible area, a calendar's date range) without breaking handlers or data sources. A partial receives a `ResourceLayoutModel<TItem>` with the items, the request, and helpers for record links and URLs that keep the current state.

## Decisions

- **Two routes through `Index` overloads:** the one-argument overload is today's single table; the two-argument overload configures shared settings and layouts. The builder types make table members unavailable in the shared lambda, so a stray `Columns` call doesn't compile. There is no seeded default table, no `Default` property and no `Remove` or `Clear`. An index with nothing shared passes an empty first lambda.
- **Always layouts internally:** the index always holds a list of layouts. The one-argument overload adds a single, implicit table layout; the two-argument overload adds the declared ones. The controller, data sources and views only deal with layouts. The implicit layout is never visible to consumers, so `Default`, `Remove` and `layouts.Table(...)` don't come back; with one layout, the switcher doesn't render and `?layout=` never appears in URLs.
- **Order:** the first layout is the default, and the order of the `Add*` calls is the switcher's order. With one layout, the switcher does not render.
- **Keys:** `AddTable` and `AddPartial` take a key, which is the `?layout=` value. Later built-in layouts default theirs (`"board"`, `"map"`).
- **Shared across layouts:** scopes, filters and search narrow the base rows, then a handler builds its items from them (searching "lisbon" counts only the matching bookings in the revenue). Their state lives in the URL and survives switching layouts.
- **Per layout:** sort, paging, columns, row details, row actions and bulk actions.
- **Paging belongs to tables:** a partial receives every row (after scopes, search and filters); it has no paging. Only tables offer `EnablePaging`.
- **Explicit keys everywhere:** every `Add*` takes the key first, so `AddPartial("calendar", "_BookingCalendar")` keeps that shape.
- **Table settings live on the layout options:** `Columns`, `DefaultSort`, `Paging` and `RowDetails` move from `ResourceIndexOptions` onto the table layout options, with no shortcuts on the index, because a layouts index may have no table.
- **Switcher labels:** a layout's `Title` is the tooltip, the accessible name of its switcher segment, and the visible text in a labelled switcher. Without a `Title`, it falls back to the key (`"finance"` becomes "Finance").
- **One request type:** handlers and data sources share `ResourceListRequest`, which gains `Layout`; there is no separate `ResourceLayoutRequest`.
- **Shared builder name:** the first lambda of the two-argument overload is a `ResourceSharedIndexBuilder<TResource>`.
- **Opting out:** `IgnoreSearch()` and `IgnoreScopes()` on a layout, and `IgnoreFilters()` once the filter bar exists. An ignored control is hidden while that layout is active and not applied; its value stays in the URL.
- **Switcher placement:** at the end of the controls row, after the scopes and search (placement A in the prototype).
- **Sorting and paging a different data shape:** supported. The request carries sort and paging, and the handler returns the items with a total, like `ResourceListResult<TItem>`.
- **Two kinds of handler:** `IResourceLayoutHandler<TItem>` (core) receives only the request values and loads its own data, so it works for standalone resources and for EF resources whose layout reads another data source; it interprets the scope, search and filters itself. `IEfCoreLayoutHandler<TResource, TItem>` (in `StellarAdmin.EntityFrameworkCore`) receives the `IQueryable<TResource>` with the scope, search and filters already applied and only reshapes it. Both work with `AddTable<TItem, THandler>` and `AddPartial<TItem, THandler>`; the EF handler on a standalone resource fails validation at startup.
- **Rows of a different data shape:** by default they don't link. `table.HasRecordKey(row => row.BookingId)` declares that each row is a record of this resource: the row links to its view page and gets the resource's row actions. `table.LinkTo<Destination>(summary => summary.DestinationId)` links each row to another resource's view page.
- **Row actions per table:** a table gets the row actions the resource allows (view, edit, delete); `table.RowActions(actions => actions.Delete = false)` turns individual ones off. A table cannot turn on an action the resource does not allow. Inline editing is limited to tables with the resource's own data shape.
- **One rendering path:** tables and partials render through the same internal layout mechanism, so later built-in layouts plug into it.

## Open questions

- **Builder names (decided 2026-11-18):** the table-only builders take a `ResourceTable` prefix: `ResourceTableColumnsBuilder<TItem>`, `ResourceTableColumnBuilder<TItem>` (the row type, since handler tables pass their items), `ResourceTablePagingBuilder<TResource>`, `ResourceTableRowDetailsBuilder<TResource>`, `ResourceTableRowDetailPanelsBuilder<TResource>`, `ResourceTableRowDetailPanelBuilder` and `EfCoreResourceTableRowDetailPanelsBuilder<TEntity>`. `ResourceRowActionsBuilder<TResource>` keeps its layout-neutral name, since later layouts may offer row actions too.

## Technical notes

Findings from reading the code on 2026-11-18; no spike yet. No rewrite is needed, but the index assumes one table per resource in several places.

1. **Options model (medium).** `ResourceIndexOptions` (`src/StellarAdmin/Resources/Options/ResourceIndexOptions.cs`) holds the columns, default sort, paging and row details directly. These move into a list of layout options, each with its own columns, sort, paging, row details and row actions. Scopes, search, title, widgets and labels stay on the index.
2. **Builder split (medium, mostly mechanical).** There are two index builders, `ResourceIndexBuilder<T>` and `EfCoreResourceIndexBuilder<T>`, both on `ResourceIndexBuilderBase` (646 lines, mixing shared and table members). Each needs a shared-only variant and a table variant, so four builder types instead of two. Both resource builders get the second `Index` overload.
3. **EF data source (medium).** `EfCoreResourceDataSource.ListAsync` applies scope and search, counts, adds includes and sorts from `Index.Columns`, then pages, all in one method. It splits into a narrowing step (scope, search, later filters), which is what `IEfCoreLayoutHandler` receives as its `IQueryable`, and a per-layout step (includes, sort and paging from that layout's columns). `ResourceListRequest` therefore gains a `Layout` property.
4. **Standalone data sources (small).** They already receive `Scope`, `Search`, `Sort` and `Paging`; they need the layout only when tables sort on different columns.
5. **Controller (medium).** `ResourceController.Index.cs` (541 lines) builds one grid model; paging, sort, inline rows and row details assume that table. It picks the active layout from `?layout=`, dispatches to the data source, an `IResourceLayoutHandler` or an `IEfCoreLayoutHandler`, and carries the layout in the URLs it builds and in the htmx swaps.
6. **Tables with a different data shape (largest unknown).** The grid view model and the column display pipeline are typed on `TResource`. A table over `TItem` needs column builders generic on the item, display inference without EF metadata, and keys from `HasRecordKey` for row actions. Standalone resources already infer displays without EF, so much may carry over; unverified.
7. **Views (small to medium).** The switcher (`sa-segmented-control`) at the end of the controls row; `_IndexPageGrid` renders the active layout, either the grid or the partial with a `ResourceLayoutModel<TItem>`; `Ignore*` hides the scopes or search for that layout.
8. **Validation (small).** Duplicate keys, a handler that doesn't match the resource kind, and `HasRecordKey` together with `LinkTo`.

Unchanged: the simple `Index` route's public API and behaviour, scopes, search, widgets and labels. Row details and inline editing move per table without changing their logic.

## Stages

Each stage ends at a checkpoint: stop, update the Progress list below with what was done and what was actually verified, add or update the stage's AdminPlayground example, and wait for Jerrie's review before starting the next stage. Do not commit unless Jerrie asks. The playground runs at `http://localhost:5191` (`sandbox/AdminPlayground`); port 5205 is Jerrie's. EF resources in the playground include Products (has scopes), Categories and Excursions; standalone ones include Trips, Destinations, Guides and Customers. Pick the example resource per stage and say which one at the checkpoint.

1. **Implicit table layout (no visible change).** Introduce the internal layout list and the table layout options; move `Columns`, `DefaultSort`, `Paging` and `RowDetails` from `ResourceIndexOptions` onto them; the existing one-argument `Index()` fills a single implicit table layout. Split `EfCoreResourceDataSource.ListAsync` into the narrowing step and the per-layout sort and paging step. The controller, data sources and views read the active layout. Checkpoint: existing tests pass and the playground indexes behave as before.
2. **`Index` overload and same-shape tables.** The two-argument overload, `ResourceSharedIndexBuilder<TResource>` and the table builder split for both the core and EF builders, `ResourceLayoutsBuilder<TResource>` with `AddTable`, the switcher (placement A), `Title`/`Icon` with the key fallback, `?layout=` in URLs and htmx swaps, `Layout` on `ResourceListRequest`, `IgnoreSearch`/`IgnoreScopes`, per-table sort, paging and row details, `RowActions`, and validation (duplicate keys). Checkpoint: a playground EF resource with two tables showing different columns.
3. **Partials with the resource's own rows.** `AddPartial(key, partialName)`, `ResourceLayoutModel<TResource>` (items, request, record-link and URL helpers), every row with no paging. Checkpoint: a playground partial layout (for example a simple calendar or card list) on an EF resource.
4. **Partials with custom handlers.** `AddPartial<TItem, THandler>`, `IResourceLayoutHandler<TItem>` (core) and `IEfCoreLayoutHandler<TResource, TItem>` (EF, receiving the narrowed `IQueryable`), handler validation per resource kind, and `Ignore*` on these layouts. Checkpoint: a summary partial with a different data shape on an EF resource and on a standalone resource.
5. **Tables with their own data shape.** `AddTable<TItem, THandler>`, the grid and column display pipeline over `TItem`, sorting and paging through the handler, `HasRecordKey` with row actions, `LinkTo<TOther>`, and validation (`HasRecordKey` with `LinkTo`). Spike the column pipeline over `TItem` at the start of this stage. Checkpoint: a ledger table that links back to its records and a summary table that links to another resource.
6. **Docs (after stage 5 is approved).** Website docs for index layouts (follow `.agents/skills/write-website-docs/SKILL.md`; website checkout at `../website`), the consumer skill in `skills/` with regenerated references (see `docs/repos/skills.md`), and screenshots where the docs need them.

Conventions to follow throughout: `docs/conventions/options-builders.md`, `csharp-file-organization.md`, `unit-testing.md` (TUnit, mirror the SUT's folders), `xml-documentation.md`.

## Progress

- Stage 1: implemented 2026-11-18, awaiting review. `ResourceLayoutOptions` (key) and `ResourceTableLayoutOptions` (columns, default sort, paging, row details) hold the table settings; `ResourceIndexOptions.Layouts` lists them, and the one-argument `Index` fills a table keyed `table` (`ResourceIndexOptions.ImplicitTableKey`), also added for an index that is never configured. The index builder and its columns, paging and row-details builders target a table by key (`TableKey`, the implicit one for now). The checks (columns, badges, computed columns, row details, paging, default sort) run per table; inline rows check the default table; references sort by the first table's default sort. `EfCoreResourceDataSource.ListAsync` is split into `Narrow` (keys, scope, search) and `Arrange` (a table's includes, sort and paging); the controller reads `IndexTable`, which is the first layout until stage 2 adds `?layout=`. Verified: the full solution test run passes (2168 tests), plus two new `ResourceOptionsFactoryTests`; 69 playground index URLs (every index, its sort, page, page-size and scope links, and searches) and three htmx grid swaps return the same HTML as a baseline build of `HEAD`, once tokens, generated ids and checkout paths are normalized. No playground example, since nothing is visible.
- Stage 2: implemented 2026-11-18, awaiting review. Builders: `ResourceIndexBuilderBase` keeps only the shared members (title, labels, widgets); the table members (columns, default sort, paging, row details) moved to `ResourceTableLayoutBuilderBase`, on `ResourceLayoutBuilderBase` (`Title`, `Icon`, `IgnoreScopes`, `IgnoreSearch`), with `RowActions` (`ResourceRowActionsBuilder`: `View`, `Edit`, `Delete`). The one-argument index builders (`ResourceIndexBuilder`, `EfCoreResourceIndexBuilder`) forward their table members to an internal table builder for the implicit table, so their public API is unchanged. New: `ResourceSharedIndexBuilder`/`EfCoreResourceSharedIndexBuilder`, `ResourceLayoutsBuilder`/`EfCoreResourceLayoutsBuilder` (`AddTable(key)`, `AddTable(key, configure)`; a blank or repeated key, case-insensitive, throws `ArgumentException`), `ResourceTableLayoutBuilder`/`EfCoreResourceTableLayoutBuilder` (the EF one adds the EF row-detail overloads), and the two-argument `Index` on both resource builders. The two-argument overload replaces an implicit table that an earlier one-argument call configured (the integration test host configures one by default); approved by Jerrie. Options: `ResourceLayoutOptions` gains `Title`, `Icon`, `IgnoresScopes`, `IgnoresSearch`; `ResourceTableLayoutOptions` gains `RowActions`. Requests: `ResourceIndexQuery.Layout` and `ResourceListRequest.Layout` (the active layout's key; null outside the index); the EF data source arranges by that layout. Controller: the active layout comes from `?layout=` (case-insensitive, unknown keys fall back to the first); every index URL carries it (scope tabs, search, delete form, inline row and row-detail requests, redirects; sort and pager links keep it through `preserve-query`). Ignored scopes or search are hidden and not applied but stay in the URL. Row actions are filtered per table; inline rows only appear in the first table, which the inline row check matches against the form (approved by Jerrie). View: the switcher is an `sa-segmented-control` at the end of the controls row (icon with an `sr-only` title and a `title` tooltip, or the title as text; the key humanized when there is no title), rendered only with two or more layouts. Switching loads the whole page with htmx (`hx-target="body"`, keeping scope and search, dropping sort and paging) with `settle:0`, since htmx's settle copies the old radios' attributes onto the new ones with the same ids and loses the selection. Playground: Products (EF) now has a Catalog table (the former columns, row details, paging) and a Pricing table (name, category, price; sorted by price descending, 25 per page, no delete button). Verified: full solution test run passes (2184 tests), with 7 new `ResourceLayoutsBuilderTests` and 7 new `ResourceIndexLayoutsTests` (switcher order, titles, icon and selection; `?layout=` columns, sort and paging; unknown key; ignored scopes and search; layout in links; per-table delete); all 38 playground pages return 200; in headless Chromium on Products, switching to Pricing keeps scope and search, shows its columns and 25 rows without delete buttons, selects its segment, keeps `layout=` through sort, paging and scope links, and switching back restores the catalog; row details load for the catalog and 404 for Pricing, which has none.
- Stage 3: implemented 2026-11-18, awaiting review. `AddPartial(key, partialName)` and `AddPartial(key, partialName, configure)` on both layouts builders (a blank partial name throws; keys share the tables' duplicate check), returning `ResourcePartialLayoutBuilder<TResource>` (title, icon, `IgnoreScopes`, `IgnoreSearch`; one builder for core and EF). Options: `ResourcePartialLayoutOptions` (`PartialName`). Model: `ResourceLayoutModel<TItem>` in `StellarAdmin.Resources` with `Items`, `Request` (layout key, applied scope and search), `GetViewUrl(item)` and `GetEditUrl(item)` (null without that page), and `GetIndexUrl(values)` (the index with the query's layout, scope and search, plus or replacing the given values). Controller: a partial's request has no sort and no paging, so it lists every item the scope and search select; the view model's new `PartialLayout` (partial name and model) replaces the grid inside `#index-page-data-grid`, so the search still swaps it; links in the partial navigate normally (`hx-boost` off), padded by `.sa-resource-index-partial`. Row details for a partial layout return 404 instead of throwing. Playground: Products gets a third layout, Cards (`_ProductCards`, icon `layout-grid`): a card per product with SKU, price, View and Edit, and a count with a link to the Pricing layout through `GetIndexUrl`. An EF partial gets no includes (no columns to read them from), so the cards don't show the category; Jerrie decided a partial that needs related data waits for stage 4's handlers, with no include setting. Verified: full solution test run passes (2191 tests), with 4 new `ResourceLayoutsBuilderTests` (order and settings, a key a table uses, blank partial names) and 3 new `ResourceIndexLayoutsTests` (the partial gets every searched item with no sort or paging and replaces the grid; view, edit and index URLs; row details 404); all 41 sidebar pages return 200; in headless Chromium on Products, switching to Cards keeps the search and selects its segment, typing a search swaps the cards and their count, the count link keeps scope and search on the way to Pricing, and View opens the product.
- Stage 4: implemented 2026-11-18, awaiting review. `IResourceLayoutHandler<TItem>` (core) and `IEfCoreLayoutHandler<TEntity, TItem>` (EF) return a `ResourceListResult<TItem>` from the request (layout key, scope and search; no sort or paging), and the EF one also receives the untracked query with the scope and search applied (`EfCoreResourceDataSource.Narrow`). `AddPartial<TItem, THandler>(key, partialName)` and the `configure` overload: on the core layouts builder `THandler` must implement `IResourceLayoutHandler<TItem>`, so an EF handler on a standalone resource doesn't compile (no startup validation needed); on the EF layouts builder `THandler` may implement either interface (an EF handler wins when it implements both), and one that implements neither throws `InvalidOperationException` at registration. The handler is registered scoped if it isn't already. The partial gets a `ResourceLayoutModel<TItem>` with the handler's items; `GetViewUrl` and `GetEditUrl` return null for handler items, and `GetIndexUrl` works as before. `Ignore*` needed no new code: an ignored scope or search is null in the handler's request. The controller skips the data source for a handler layout. Playground: Products (EF) gets a fourth layout, Prices per category (`CategoryPriceSummaryHandler`, `_CategoryPriceSummary`, icon `chart-column`), which reads each product's category from the narrowed query, so the related data an EF partial lacked in stage 3 now comes through a handler; Destinations (standalone) moves to the layouts route with its table and a Countries layout (`DestinationCountriesHandler`, which loads from the data source itself, `_DestinationCountries`, icon `globe`). Verified: full solution test run passes (2197 tests), with 1 new `ResourceLayoutsBuilderTests` test (layout added, handler registered scoped), 1 new `ResourceIndexLayoutsTests` test (handler items replace the resource's, the data source isn't called, an ignored search is null, no view link, index URL keeps the state) and a new EF `ResourceLayoutsTests` (the EF handler gets the scoped and searched entities, two cases; a core handler on an EF resource; a handler of neither kind throws); all 41 sidebar pages return 200; in headless Chromium, switching Products to Prices per category selects its segment, and searching "Lamp" swaps it to one Lighting card whose numbers match the price summary widget; Destinations switches to Countries.
- Handler items had no record links (`GetViewUrl`/`GetEditUrl` returned null); Jerrie settled in stage 5 that partials get them (see the stage 5 entry).
- Stage 5: implemented 2026-11-18, awaiting review. Spike: the grid was already independent of the row type (columns are `LambdaExpression`s, displays are inferred from the expression's parameter type, the view model's `Items` is an `IEnumerable` and keys come from `GetKey(object)`), so no new column pipeline was needed. `AddTable<TItem, THandler>(key)` and the `configure` overload on both layouts builders (the core one takes `IResourceLayoutHandler<TItem>`; the EF one takes either kind, as `AddPartial<TItem, THandler>` does) return `ResourceHandlerTableLayoutBuilder<TResource, TItem>`: title, icon, `Ignore*`, `Columns` (a `ResourceColumnsBuilder<TItem>`, whose type parameter now means the row type), `DefaultSortBy`/`DefaultSortByDescending`, `EnablePaging`, `RowActions`, `HasRecordKey(Func<TItem, object?>)` and `LinkTo<TOther>(Func<TItem, object?>)`. The keys are delegates rather than expressions, since nothing translates them, and a null key is a row without links. Calling both `HasRecordKey` and `LinkTo` throws `InvalidOperationException`. No row details or inline rows on these tables. The handler receives the request with the table's sort (the column's sort name) and paging, and its total drives the pager. Rows with a record key get the resource's view, edit and delete actions (filtered by `RowActions`); `LinkTo` rows get only a view link to the other resource, shown when that resource is registered, has a view page and the user is authorized; otherwise there is no link, and a missing resource or view page is a configuration problem (`RowLinkUnavailable`, reported when the table renders). Internals: `ResourceTableLayoutOptions` gained `Loader`, `RecordKey` and `Link`; `ResourceIndexOptions.RecordTables` (tables of the resource's own rows) replaces the first-table lookups for inline rows, reference sorting and EF sort checks, and the EF data source arranges a request for a handler table's key by the first record table. `IResourceIndexPageViewModel.GetKey` now returns `string?`, and `GetViewUrl(object)` is new (the grid's view link uses it). `ResourceRegistration` gained `ResolveCanView`. Playground: Products (EF) gets Price check (`ProductPriceCheckHandler`, icon `scale`; each product against its category's average, sorted and paged by the handler, `HasRecordKey`) and Category summary (`CategorySummaryHandler`, icon `tags`; products and average price per category, `LinkTo<Category>`); Categories gets a view page for those links. Products now has six layouts. Verified: full solution test run passes (2204 tests), with 2 new `ResourceLayoutsBuilderTests` (handler table options and registration; `HasRecordKey` with `LinkTo` throws), 4 new `ResourceIndexLayoutsTests` (handler rows with the request's scope, search, sort and page and no links; record rows' actions with `RowActions`, and a row without a key has none; `LinkTo` links to the other resource's view page; a linked resource without a view page gives no links and a problem) and 1 new EF `ResourceLayoutsTests` test (an EF handler table shows the searched, reshaped entities); in headless Chromium on Products, Price check selects its segment, sorting by Difference and searching "Lamp" go through the handler, View opens the product, and a Category summary row opens its category's view page. Partial record links (Jerrie approved 2026-11-18): `AddPartial<TItem, THandler>` on both layouts builders now returns `ResourceHandlerPartialLayoutBuilder<TResource, TItem>`, with the same `HasRecordKey` and `LinkTo<TOther>` as handler tables (shared in `ResourceLayoutBuilderBase`; `Link` and `RecordKey` moved to `ResourceLayoutOptions`). A record key gives `GetViewUrl`/`GetEditUrl` the resource's pages; `LinkTo` gives `GetViewUrl` the other resource's view page under the same checks, and `GetEditUrl` stays null. The loader now receives the item URLs (`ResourceLayoutUrls`). Playground: the Prices per category cards link their titles to the category (`LinkTo<Category>`; `CategoryPriceSummary` gained `CategoryId`). Verified: full solution test run passes (2208 tests), with 2 new `ResourceLayoutsBuilderTests` (a partial's record key; `LinkTo` with `HasRecordKey` throws) and 2 new `ResourceIndexLayoutsTests` (record items link to their view and edit pages, an item without a key has none; `LinkTo` items link to the other resource's view page with no edit link); on the playground (curl) the Prices per category card titles link to `/stellaradmin/categories/view/N`, which opens.
- Builder renames: done 2026-11-18, awaiting review. The renames in Open questions, with the files, the two column builders' type parameter (`TItem`), both `PublicAPI.Unshipped.txt` files, tests (three test files renamed to match), the scenario and CLI screenshot code, the playground and the live row-detail-expand-when plan. Archived plans keep the old names. The website and the consumer skills don't name these builders. Verified: the solution builds and the full test run passes (2208 tests).
- Stage 6: drafted 2026-11-18, awaiting review. Website (`../website`, uncommitted): new `content/docs/admin/layouts.mdx` (adding layouts with the two-callback `Index`, keys and `?layout=`, title and icon, tables with `RowActions`, partial views and `ResourceLayoutModel<TItem>`, `IgnoreSearch`/`IgnoreScopes`) and `layout-data.mdx` (EF and core handlers, handler tables with sorting and paging, handler partials, `HasRecordKey` and `LinkTo<TOther>`), both after Inline editing in `meta.json`, and a Layouts bullet in `index-pages.mdx`. Three `<Screenshot>` placeholders (`layouts-switcher`, `layouts-partial`, `layout-data-table`) have no images yet. Consumer skill: new `skills/stellar-admin/references/layouts.md`, with a task row, the description, the namespaces and a Not supported line (built-in board, map, calendar and card layouts) in `SKILL.md`, and a link from `references/index-pages.md`. Verified: the website type check passes and the three pages return 200 from a dev server, with the linked anchors present; the skill validator (8 tests, 96 files) and `skills check` pass. The EF handler examples sort and page an anonymous projection, since EF Core can't translate a sort on a record created through its constructor; not run against a database.

## Later

- **Board, map, cards and calendar:** one built-in layout each, each with its own prototype, tag helpers and plan.
- **Cards:** board cards and map popups default to the resource's `Reference` (title, description, avatar); `board.Card(card => ...)` overrides them.
- **Caps:** board and map have no paging; they need a limit such as `board.MaxItems`.
- **Writing back:** moving a card between board lanes changes the record, which ties into editing.
