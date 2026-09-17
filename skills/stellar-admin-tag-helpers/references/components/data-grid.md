---
component: DataGrid
tags: [sa-data-grid, sa-data-grid-column, sa-data-grid-empty, sa-data-grid-header-template, sa-data-grid-item-template, sa-data-grid-pager, sa-data-grid-selection, sa-data-grid-sort]
generated: true
---

# DataGrid

A data grid that renders a full table from declarative column definitions and a data source. Columns are declared with `sa-data-grid-column`; the grid renders the header from the column definitions and re-renders the column content once per data row. The grid renders as a card-style panel; add an `sa-data-grid-pager` child to render a footer bar with paging links and a record-range summary, an `sa-data-grid-sort` child to render sort links on the sortable columns' headers, and an `sa-data-grid-selection` child to render a row-selection checkbox column. Querying, sorting, and paging the data remain the application's responsibility — the grid renders the items it is given.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-data-grid>` | A data grid that renders a full table from declarative column definitions and a data source. Columns are declared with `sa-data-grid-column`; the grid renders the header from the column definitions and re-renders the column content once per data row. The grid renders as a card-style panel; add an `sa-data-grid-pager` child to render a footer bar with paging links and a record-range summary, an `sa-data-grid-sort` child to render sort links on the sortable columns' headers, and an `sa-data-grid-selection` child to render a row-selection checkbox column. Querying, sorting, and paging the data remain the application's responsibility — the grid renders the items it is given. |
| `<sa-data-grid-column>` | Defines a column of a data grid. The cell value comes from, in order of precedence: a nested `sa-data-grid-item-template`, plain child content (re-rendered per row), or the property named by `field` or selected by `field-for`. A field cell renders through a grid display template when the column declares a `template` or the field property carries a `[UIHint]`. The header renders a nested `sa-data-grid-header-template` when present, otherwise `title`, otherwise a field column derives its header from the field's display name. A `class` attribute is applied to both the header and body cells of the column. |
| `<sa-data-grid-empty>` | Custom content for the data grid's empty state, shown as a single full-width row when the grid has no items. When omitted, the grid renders a default "No records found." message. |
| `<sa-data-grid-header-template>` | Custom header content for a data grid column, rendered once and taking precedence over the column's `title` attribute. |
| `<sa-data-grid-item-template>` | The row template of a data grid column, re-rendered once per data row with the current item available through `Html.GridItem<T>()`. Unlike plain child content, the template is never executed during the grid's collect pass, so it can safely dereference the item unconditionally. |
| `<sa-data-grid-pager>` | A pager that renders page-number, previous, and next links for a paged data set. Link destinations are specified with the standard routing attributes (`asp-page`, `asp-action`, `asp-route-*`, etc.); a route value must contain the `{pageNo}` placeholder, which is replaced with the target page number in each link (e.g. `asp-route-pageNo="{pageNo}"`). A route value may also contain the `{pageSize}` placeholder: page links substitute `selected-page-size` when one is supplied and drop the route value otherwise, so a size carries across page navigation only when one was actually chosen, while the links of the footer's page-size selector (`page-size-options`) always substitute the target size and reset the page number to 1. Declared as a child of `sa-data-grid` it renders in the grid's footer bar as compact icon links, alongside a record-range summary when `total-items` is supplied — the footer renders even when there is a single page, showing just the record count. Used standalone it renders the classic labelled pager and nothing at all when there is one page or less. |
| `<sa-data-grid-selection>` | Adds row selection to the data grid: a leading checkbox column with a select-all checkbox in the header. Renders nothing itself — the grid wraps its table in the `sel-table-selection` web component, to which an `id` and `class` set here are transferred. Read the current selection from the element's `selectedValues` property, or listen for its bubbling `selection-change` event; with a `name` the selection also posts as ordinary checkbox form data. |
| `<sa-data-grid-sort>` | Declares how the data grid generates its column-sorting links, and the current sort state. Link destinations are specified with the standard routing attributes (`asp-page`, `asp-action`, `asp-route-*`, etc.); the route values must contain the `{sort}` placeholder, replaced with the column's sort field, and the `{dir}` placeholder, replaced with the target direction (`asc` or `desc`) — e.g. `asp-route-sortBy="{sort}" asp-route-sortDir="{dir}"`. Renders nothing itself; columns opt in with their `sortable` attribute. |

## Attributes

### `<sa-data-grid>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `items` | `IEnumerable` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-data-grid-column>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `field` | `string` | — | — |
| `field-for` | `LambdaExpression` | — | — |
| `format` | `string` | — | — |
| `sortable` | `bool` | `false` | `true`, `false` |
| `sort-field` | `string` | — | — |
| `template` | `string` | — | — |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-pager>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `page-no` | `long` | — | — |
| `total-pages` | `long` | — | — |
| `page-size` | `long` | — | — |
| `page-size-options` | `string` | — | — |
| `selected-page-size` | `long` | — | — |
| `total-items` | `long` | — | — |
| `preserve-query` | `bool` | `false` | `true`, `false` |
| `asp-action` | `string` | `null` | — |
| `asp-area` | `string` | `null` | — |
| `asp-controller` | `string` | `null` | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | `null` | — |
| `asp-page-handler` | `string` | `null` | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | `null` | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-selection>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `key-field` | `string` | — | — |
| `name` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-sort>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `sort-by` | `string` | — | — |
| `sort-direction` | `DataGridSortDirection` | `Ascending` | `Ascending`, `Descending` |
| `preserve-query` | `bool` | `false` | `true`, `false` |
| `preserve-query-except` | `string` | — | — |
| `asp-action` | `string` | `null` | — |
| `asp-area` | `string` | `null` | — |
| `asp-controller` | `string` | `null` | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | `null` | — |
| `asp-page-handler` | `string` | `null` | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | `null` | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Example

*From `Pages/DataGrid/_Intro.cshtml`*

```razor
    var selectedPageSize = int.TryParse(Context.Request.Query["pageSize"], out var requestedPageSize)
        ? (int?)Math.Max(requestedPageSize, 1)
        : null;
    var pageSize = selectedPageSize ?? 3;
    var pageNo = int.TryParse(Context.Request.Query["pageNo"], out var requestedPage)
        ? Math.Max(requestedPage, 1)
        : 1;
    var sortBy = Context.Request.Query["sortBy"].ToString();
    var sortDirection = Context.Request.Query["sortDir"] == "desc"
        ? DataGridSortDirection.Descending
        : DataGridSortDirection.Ascending;

    IEnumerable<Booking> bookings = StaticData.Bookings;
    bookings = (sortBy.ToLowerInvariant(), sortDirection) switch
    {
        ("destination", DataGridSortDirection.Ascending) => bookings.OrderBy(b => b.Destination),
        ("destination", DataGridSortDirection.Descending) => bookings.OrderByDescending(b => b.Destination),
        ("amount", DataGridSortDirection.Ascending) => bookings.OrderBy(b => b.Amount),
        ("amount", DataGridSortDirection.Descending) => bookings.OrderByDescending(b => b.Amount),
        _ => bookings
    };
    var pageOfBookings = bookings.Skip((pageNo - 1) * pageSize).Take(pageSize);
    var totalPages = (int)Math.Ceiling(StaticData.Bookings.Length / (double)pageSize);
}
<sa-data-grid items="pageOfBookings">
    <sa-data-grid-selection key-field="Id" name="selectedBookings" id="booking-grid-selection"/>
    <sa-data-grid-sort sort-by="@sortBy" sort-direction="sortDirection"
                       asp-page="/DataGrid/Index"
                       asp-route-sortBy="{sort}" asp-route-sortDir="{dir}"
                       preserve-query="true" preserve-query-except="pageNo"/>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status"/>
    <sa-data-grid-column title="Destination" field="Destination" sortable="true"/>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" sortable="true" class="text-right"/>
    <sa-data-grid-empty>No bookings found.</sa-data-grid-empty>
    <sa-data-grid-pager page-no="pageNo" total-pages="totalPages"
                        page-size="pageSize" selected-page-size="selectedPageSize"
                        total-items="StaticData.Bookings.Length"
                        page-size-options="3,5,10"
                        asp-page="/DataGrid/Index"
                        asp-route-pageNo="{pageNo}" asp-route-pageSize="{pageSize}"
                        preserve-query="true"/>
</sa-data-grid>
<sa-button id="booking-grid-delete" variant="ButtonVariant.Destructive" class="mt-4" hidden>
    Delete selected
</sa-button>
<script>
    (() => {
        const selection = document.getElementById("booking-grid-selection");
        const deleteButton = document.getElementById("booking-grid-delete");

        const syncButton = () => deleteButton.hidden = selection.selectedValues.length === 0;

        selection.addEventListener("selection-change", syncButton);
        customElements.whenDefined("sel-table-selection").then(syncButton);

        deleteButton.addEventListener("click", () =>
            alert(`Delete bookings:\n${selection.selectedValues.join("\n")}`));
    })();
</script>
```
