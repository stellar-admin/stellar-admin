---
component: DataGrid
tags: [sa-data-grid, sa-data-grid-add-row, sa-data-grid-column, sa-data-grid-edit-template, sa-data-grid-empty, sa-data-grid-header-template, sa-data-grid-item-template, sa-data-grid-pager, sa-data-grid-row-detail, sa-data-grid-selection, sa-data-grid-sort]
generated: true
---

# DataGrid

Renders a table of items, with columns declared by `sa-data-grid-column` children.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-data-grid>` | Renders a table of items, with columns declared by `sa-data-grid-column` children. |
| `<sa-data-grid-add-row>` | Renders a full-width row in a data grid, for a button that adds an item. |
| `<sa-data-grid-column>` | Defines a column of a data grid. |
| `<sa-data-grid-edit-template>` | Renders the editor of a data grid column in rows that are in edit mode. |
| `<sa-data-grid-empty>` | Renders custom content in a full-width row when the data grid has no items. |
| `<sa-data-grid-header-template>` | Renders custom header content for a data grid column. |
| `<sa-data-grid-item-template>` | Renders the content of a data grid column for each row. |
| `<sa-data-grid-pager>` | Renders page number, previous and next links for a paged list. |
| `<sa-data-grid-row-detail>` | Renders details below each data grid row, which users can expand and collapse. |
| `<sa-data-grid-selection>` | Adds a checkbox column to a data grid, so users can select rows. |
| `<sa-data-grid-sort>` | Configures the sort links of a data grid's sortable columns and the current sort. |

## Attributes

### `<sa-data-grid>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `items` | `IEnumerable` | — | — |
| `key-field` | `string` | — | — |
| `editing-keys` | `IEnumerable` | — | — |
| `new-item` | `object` | — | — |
| `rows-only` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-data-grid-add-row>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `position` | `DataGridAddRowPosition` | `Top` | `Top`, `Bottom` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-column>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `field` | `string` | — | — |
| `empty-text` | `string` | — | — |
| `field-for` | `LambdaExpression` | — | — |
| `format` | `string` | — | — |
| `sortable` | `bool` | `false` | `true`, `false` |
| `sort-field` | `string` | — | — |
| `template` | `string` | — | — |
| `template-renders-empty` | `bool` | `false` | `true`, `false` |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-pager>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `page` | `long` | — | — |
| `total-pages` | `long` | — | — |
| `page-size` | `long` | — | — |
| `page-size-options` | `string` | — | — |
| `selected-page-size` | `long` | — | — |
| `total-items` | `long` | — | — |
| `preserve-query` | `bool` | `false` | `true`, `false` |
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-row-detail>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `toggle` | `DataGridRowDetailToggle` | `Start` | `Start`, `End`, `None` |
| `row-click` | `bool` | `false` | `true`, `false` |
| `expand-mode` | `TableRowDetailExpandMode` | `Multiple` | `Multiple`, `Single` |
| `emphasis` | `TableRowDetailEmphasis` | `Band` | `None`, `Band`, `Rail` |
| `divider` | `TableRowDetailDivider` | `Inset` | `None`, `Inset`, `FullWidth` |
| `inset` | `TableRowDetailInset` | `Aligned` | `Bleed`, `Aligned` |
| `animate` | `bool` | `true` | `true`, `false` |
| `expanded-keys` | `IEnumerable` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-selection>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `name` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-data-grid-sort>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `sort-field` | `string` | — | — |
| `sort-direction` | `DataGridSortDirection` | `Ascending` | `Ascending`, `Descending` |
| `preserve-query` | `bool` | `false` | `true`, `false` |
| `preserve-query-except` | `string` | — | — |
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/DataGrid/_Intro.cshtml`*

```razor
    var selectedPageSize = int.TryParse(Context.Request.Query["pageSize"], out var requestedPageSize)
        ? (int?)Math.Max(requestedPageSize, 1)
        : null;
    var pageSize = selectedPageSize ?? 3;
    var currentPage = int.TryParse(Context.Request.Query["pageNo"], out var requestedPage)
        ? Math.Max(requestedPage, 1)
        : 1;
    var sortField = Context.Request.Query["sortBy"].ToString();
    var sortDirection = Context.Request.Query["sortDir"] == "desc"
        ? DataGridSortDirection.Descending
        : DataGridSortDirection.Ascending;

    IEnumerable<Booking> bookings = StaticData.Bookings;
    bookings = (sortField.ToLowerInvariant(), sortDirection) switch
    {
        ("destination", DataGridSortDirection.Ascending) => bookings.OrderBy(b => b.Destination),
        ("destination", DataGridSortDirection.Descending) => bookings.OrderByDescending(b => b.Destination),
        ("amount", DataGridSortDirection.Ascending) => bookings.OrderBy(b => b.Amount),
        ("amount", DataGridSortDirection.Descending) => bookings.OrderByDescending(b => b.Amount),
        _ => bookings
    };
    var pageOfBookings = bookings.Skip((currentPage - 1) * pageSize).Take(pageSize);
    var totalPages = (int)Math.Ceiling(StaticData.Bookings.Length / (double)pageSize);
}
<sa-data-grid items="pageOfBookings" key-field="Id">
    <sa-data-grid-selection name="selectedBookings" id="booking-grid-selection"/>
    <sa-data-grid-sort sort-field="@sortField" sort-direction="sortDirection"
                       asp-page="/DataGrid/Index"
                       asp-route-sortBy="{sortField}" asp-route-sortDir="{sortDirection}"
                       preserve-query="true" preserve-query-except="pageNo"/>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status"/>
    <sa-data-grid-column title="Destination" field="Destination" sortable="true"/>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" sortable="true" class="text-right"/>
    <sa-data-grid-empty>No bookings found.</sa-data-grid-empty>
    <sa-data-grid-pager page="currentPage" total-pages="totalPages"
                        page-size="pageSize" selected-page-size="selectedPageSize"
                        total-items="StaticData.Bookings.Length"
                        page-size-options="3,5,10"
                        asp-page="/DataGrid/Index"
                        asp-route-pageNo="{page}" asp-route-pageSize="{pageSize}"
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

        selection.addEventListener("rowselectionchange", syncButton);
        customElements.whenDefined("sel-table").then(syncButton);

        deleteButton.addEventListener("click", () =>
            alert(`Delete bookings:\n${selection.selectedValues.join("\n")}`));
    })();
</script>
```

*From `Pages/DataGrid/_RowDetails.cshtml`*

```razor
<sa-data-grid items="StaticData.Bookings" key-field="Id" class="w-full">
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status"/>
    <sa-data-grid-column title="Destination" field="Destination"/>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right"/>
    <sa-data-grid-row-detail expanded-keys='new[] { "TRP-4821" }'>
        @{ var details = StaticData.DetailsFor(Html.GridItem<Booking>().Id); }
        <div class="flex flex-col gap-3">
            <p>
                <span class="font-medium">@details.Traveller</span>
                <span class="text-muted-foreground">· @details.Party · @details.Email</span>
            </p>
            <sa-data-grid items="details.Itinerary">
                <sa-data-grid-column title="Date" field="Date"/>
                <sa-data-grid-column title="Type" field="Type"/>
                <sa-data-grid-column title="Description" field="Description"/>
                <sa-data-grid-column title="Amount" field="Amount" class="text-right"/>
            </sa-data-grid>
        </div>
    </sa-data-grid-row-detail>
</sa-data-grid>
```

*From `Pages/DataGrid/_Editing.cshtml`*

```razor
<sa-data-grid items="StaticData.Bookings.Take(3)" key-field="Id" editing-keys='new[] { "TRP-4822" }'>
    <sa-data-grid-selection name="selectedBookings"/>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status">
        <sa-data-grid-edit-template>
            <sa-select class="sa-cell-editor" wrap-in-field="false" size="SelectSize.Small"
                       asp-for="@Html.GridItem<Booking>().Status" asp-items="Html.GetEnumSelectList<BookingStatus>()" aria-label="Status"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Destination" field="Destination">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Destination" aria-label="Destination"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor text-right" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Amount" aria-label="Amount"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
</sa-data-grid>
```

*From `Pages/DataGrid/_NewItem.cshtml`*

```razor
<sa-data-grid items="StaticData.Bookings.Take(3)" key-field="Id"
              new-item='new Booking("", BookingStatus.Pending, "", 0)'>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status">
        <sa-data-grid-edit-template>
            <sa-select class="sa-cell-editor" wrap-in-field="false" size="SelectSize.Small"
                       asp-for="@Html.GridItem<Booking>().Status" asp-items="Html.GetEnumSelectList<BookingStatus>()" aria-label="Status"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Destination" field="Destination">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Destination"
                      placeholder="City, country" aria-label="Destination"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor text-right" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Amount" aria-label="Amount"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
</sa-data-grid>
```

*From `Pages/DataGrid/_AddRow.cshtml`*

```razor
<sa-data-grid items="StaticData.Bookings.Take(3)" key-field="Id">
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status"/>
    <sa-data-grid-column title="Destination" field="Destination"/>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right"/>
    <sa-data-grid-add-row position="DataGridAddRowPosition.Bottom">
        <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.Small">
            <sa-icon name="plus"/>
            Add booking
        </sa-button>
    </sa-data-grid-add-row>
</sa-data-grid>
```

*From `Pages/DataGrid/_EditorTemplates.cshtml`*

```razor
<sa-data-grid items="StaticData.Bookings.Take(3)" key-field="Id" editing-keys='new[] { "TRP-4822" }'>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Status" field="Status">
        <sa-data-grid-edit-template>
            @* Picked by the property's [UIHint("BookingStatus")]: EditorTemplates/BookingStatus.cshtml *@
            @Html.EditorFor(_ => Html.GridItem<Booking>().Status)
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Destination" field="Destination">
        <sa-data-grid-edit-template>
            <partial name="_DestinationEditor" for="@Html.GridItem<Booking>().Destination"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right">
        <sa-data-grid-edit-template>
            @* Named: EditorTemplates/Currency.cshtml *@
            @Html.EditorFor(_ => Html.GridItem<Booking>().Amount, "Currency")
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
</sa-data-grid>
```

*From `Pages/DataGrid/_EditingErrors.cshtml`*

```razor
    // The posted values, rendered again with the errors that a Save handler found
    var bookings = StaticData.Bookings.Take(3)
        .Select(booking => booking.Id == "TRP-4822" ? booking with { Destination = "", Amount = -50 } : booking);
}
<sa-data-grid items="bookings" key-field="Id" editing-keys='new[] { "TRP-4822" }'>
    <sa-data-grid-column title="Booking #" field="Id" class="font-medium"/>
    <sa-data-grid-column title="Destination" field="Destination">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Destination" aria-label="Destination"/>
            <sa-field-error class="sa-cell-error" asp-for="@Html.GridItem<Booking>().Destination"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
    <sa-data-grid-column title="Amount" field="Amount" format="{0:C}" class="text-right">
        <sa-data-grid-edit-template>
            <sa-input class="sa-cell-editor text-right" wrap-in-field="false" asp-for="@Html.GridItem<Booking>().Amount" aria-label="Amount"/>
            <sa-field-error class="sa-cell-error" asp-for="@Html.GridItem<Booking>().Amount"/>
        </sa-data-grid-edit-template>
    </sa-data-grid-column>
</sa-data-grid>
```
