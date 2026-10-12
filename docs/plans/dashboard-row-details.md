# Dashboard row details

Status: proposed (2026-10-12). Builds on [table-row-details](oss/table-row-details.md), which shipped the `sa-table-*` and `sa-data-grid` layers.

## Goal

Let a Dashboard resource index expand a row to show application content below it: more fields of the same resource, rows of a related resource, or anything else. The content is always an application partial. The Dashboard supplies the expandable rows, the loading and, for several panels, tabs.

## Decisions (Jerrie, 2026-10-12)

1. The settings reuse the TagHelpers enums (`DataGridRowDetailToggle`, `TableRowDetailExpandMode`, `TableRowDetailEmphasis`, `TableRowDetailInset`). The Dashboard passes them through unchanged. (Recommended; not explicitly confirmed.)
2. Both loading modes: inline (the partial's model is the row item, rendered with the index) and on demand (a handler or an EF Core projection loads the model when the row expands).
3. `ExpandWhen` is deferred.
4. Expanded rows close when search, paging, sorting or a scope swaps the grid. Open rows are not kept in the URL.
5. A no-code fields panel (`details.Fields(...)`) is deferred until display helpers exist ([crud-screen-tag-helpers](crud-screen-tag-helpers.md)).
6. Several panels render as tabs. This needs panel tabs in the library: extend the existing tabs family rather than reuse tab links or the segmented control.
7. Phasing: single panel, then panel tabs, then multiple panels.
8. A panel's URL name defaults to its title in kebab case (`"Payment history"` → `payment-history`), with an optional `Name` override.
9. Extend the tabs family with `sa-tab`, `sa-tab-panel` and a `sel-tabs` component.
10. A segmented-control look for tabs (`TabListVariant`) is deferred.

## Phase 1 — Dashboard, single panel

### API

```csharp
// Inline: the partial's model is the row item (TResource)
index.EnableRowDetails("_ProductSummary");

// On demand through a handler
index.EnableRowDetails<CustomerOrders, CustomerOrdersHandler>("_CustomerOrders");

// On demand through an EF Core projection (EfCoreResourceIndexBuilder only)
index.EnableRowDetails("_ProductPricing", p => new ProductPricing(p.Name, p.Price, p.Category!.Name));

// Every overload takes an optional configure lambda
index.EnableRowDetails("_ProductSummary", details =>
{
    details.Toggle = DataGridRowDetailToggle.Leading;
    details.RowClick = true;
    details.ExpandMode = TableRowDetailExpandMode.Single;
    details.Emphasis = TableRowDetailEmphasis.Band;
    details.Inset = TableRowDetailInset.Aligned;
    details.Animate = true;
});
```

```csharp
public interface IResourceRowDetailHandler<TModel>
{
    Task<TModel?> FindAsync(string id, CancellationToken cancellationToken);
}
```

- Builder overloads follow the options-builder rule: the no-callback overload returns `ResourceRowDetailsBuilder<TResource>`, the callback overload returns the index builder. Partial name is essential data, so it is an argument. Settings are setter-only properties that call `Services.Configure<ResourceOptions<TResource>>`.
- `EnableRowDetails<TModel, THandler>` registers `THandler` as scoped, as `AllowEdit<TModel, THandler>` does.
- Defaults match `sa-data-grid-row-detail`.

### Options

- `ResourceIndexOptions.RowDetails` (`ResourceRowDetailsOptions?`, null when disabled): the settings plus `Panels` (`IList<ResourceRowDetailPanelOptions>`). Phase 1 always holds one panel; phase 3 opens up the collection.
- `ResourceRowDetailPanelOptions`: `Title`, `Name`, `PartialName`, and a loader. Inline panels have no loader. On-demand panels hold an internal loader delegate `Func<IServiceProvider, string, CancellationToken, Task<object?>>` that resolves the handler, or runs the EF query.

### EF Core

- `EfCoreResourceIndexBuilder.EnableRowDetails<TModel>(string partialName, Expression<Func<TEntity, TModel>> select, Action<...>? )` with overloads in the house pattern.
- The loader: `context.Set<TEntity>().AsNoTracking().Where(<primary key> == <converted id>).Select(select).SingleOrDefaultAsync()`. Reuse the key conversion in `EfCoreResourceDataSource.FindEntityByKeyAsync` (extract it to a shared internal helper, and do not change its behavior).

### Rendering

- `_IndexDataGrid.cshtml` adds `sa-data-grid-row-detail` when `Model.RowDetails` is set, with `key-field` from the view model's key.
- Inline: `<partial name="@panel.PartialName" model="Html.GridItem<object>()" />`.
- On demand: the content element holds the shared `_SheetLoading`-style placeholder and carries `hx-get` (the RowDetail URL), `hx-trigger="row-detail-expand once"` on the detail row, and `hx-target="find [data-slot=table-row-detail-content]"`. Check that these htmx attributes are not overridden by the grid container's inherited `hx-target`, `hx-select` and `hx-swap` (set `hx-select="unset"` or equivalent as needed).
- The edit and delete column stays trailing. The toggle defaults to leading.
- View model: add `RowDetails` to `IResourceIndexPageViewModel` (settings, panels, and a URL builder for on-demand panels).

### Endpoint

- `ResourceController.RowDetail(string id, [FromQuery] string? panel)`, `GET {resource}/RowDetail/{id}`. It runs under the resource's existing authorization, returns 404 when row details are off, the panel is unknown, or the loader returns null, and renders the panel's partial without a layout.
- An unknown or inline panel name returns 404 (inline panels are never served).

### Playground (DashboardPlayground, no schema changes)

- Products: inline `_ProductSummary` from `Product.Details`.
- Products, alternative registration or a second demo resource: EF projection with the category name.
- Customers: a handler (the custom data source has no EF Core), for example recent orders from static data.

### Tests

- Dashboard unit tests: builder registration, defaults, the handler registration, and the panel name derivation.
- EF Core tests: the projection loader, including a missing key and a composite or non-int key if the data source supports one.
- Dashboard integration tests: the index renders detail rows for inline and on-demand panels, the RowDetail endpoint returns the partial, 404s, and enforces resource authorization.
- CDP browser check in the playground: inline expand, on-demand load once, single mode, and expanded rows after a search swap.

## Phase 2 — Panel tabs in the library

Extend the existing tabs family (`sa-tab-list`, `sa-tab-link`) with button tabs and panels. It reuses the existing tab-list CSS and variants, so it looks the same as today's tabs.

```cshtml
<sa-tabs>
    <sa-tab-list>
        <sa-tab for="summary" selected="true">Summary</sa-tab>
        <sa-tab for="itinerary">Itinerary</sa-tab>
    </sa-tab-list>
    <sa-tab-panel id="summary">...</sa-tab-panel>
    <sa-tab-panel id="itinerary"
                  hx-get="/bookings/1/itinerary" hx-trigger="tab-show once">Loading…</sa-tab-panel>
</sa-tabs>
```

- `sa-tabs` renders the `sel-tabs` web component. `sa-tab-list` keeps its current output and stays usable alone with `sa-tab-link`.
- `sa-tab` renders `<button type="button" role="tab">` with `aria-controls`, `aria-selected` and a roving `tabindex`. `sa-tab-panel` renders `role="tabpanel"` with `aria-labelledby` and `hidden` when not selected. The server picks the initial tab (`selected`, else the first).
- `sel-tabs`: event delegation on itself, state in the DOM only (as `sel-table-row-details`). Click and arrow, Home and End keys (automatic activation). A bubbling `tab-show` event on the panel after it is shown (`detail: { tab, panel }`), and `tab-hide` on the hidden one. No events for the initial state. Public `select(tab)`. Nested `sel-tabs` are excluded, as nested tables are.
- Check the role attributes the current `sa-tab-list` renders and add `role="tablist"` only when it contains `sa-tab`s, so link tabs keep link semantics.
- Panel styling: `.sa-tab-panel` with the theme's gap above it. Update the theme coverage manifest.
- Tests: TagHelpers unit tests, a CDP check (keyboard, events, htmx `tab-show once`).
- DocsSamples demos, website `tabs.mdx` section and API reference, skills examples and regenerated reference, in the same pass as phase 3's docs.

## Phase 3 — Dashboard, multiple panels

```csharp
index.EnableRowDetails(panels =>
{
    panels.Add("Summary", "_ProductSummary");
    panels.Add<CustomerOrders, CustomerOrdersHandler>("Orders", "_CustomerOrders");
    panels.Add("Pricing", "_ProductPricing", p => new ProductPricing(...)); // EF Core panels builder
}, details => details.Emphasis = TableRowDetailEmphasis.Band);
```

- `ResourceRowDetailPanelsBuilder<TResource>` with `Add` overloads that return `ResourceRowDetailPanelBuilder` (`Name` property). The EF Core index builder's overload hands over `EfCoreRowDetailPanelsBuilder<TContext, TEntity>`, which adds the projection overload, so no `<TContext>` type argument is needed.
- One panel renders without tabs. Two or more render `sa-tabs` inside the detail content: inline panels render in place, and on-demand panels use `hx-trigger="tab-show once"`. When the first panel is on demand it loads on `row-detail-expand once`, as in phase 1.
- Panel names must be unique; registration throws on a duplicate.
- Playground: one resource with an inline, a handler and an EF panel.
- Docs: the Dashboard consumer reference in `skills/` (resource index section) gains row details. Check where Dashboard features are documented on the website before adding pages there.

## Out of scope

- Opening rows from the URL or keeping them across grid swaps.
- `ExpandWhen`, a fields panel, and the segmented tab look (deferred above).
- Changes to the `sa-table-*` and `sa-data-grid` row-detail layers, unless a phase finds a bug.

## Verification log

None yet.
