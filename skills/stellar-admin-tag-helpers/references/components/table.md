---
component: Table
tags: [sa-table, sa-table-body, sa-table-caption, sa-table-cell, sa-table-footer, sa-table-head, sa-table-header, sa-table-row, sa-table-row-detail, sa-table-row-detail-toggle, sa-table-row-details, sa-table-selection]
generated: true
---

# Table

A responsive data table, rendered as a `<table>` inside a scrollable container. Compose it with the header, body, footer, row, head, cell, and caption subcomponents.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-table>` | A responsive data table, rendered as a `<table>` inside a scrollable container. Compose it with the header, body, footer, row, head, cell, and caption subcomponents. |
| `<sa-table-body>` | The body of a table, rendered as a `<tbody>`; contains the data rows. |
| `<sa-table-caption>` | A caption for a table, rendered as a `<caption>`; describes the table's contents. |
| `<sa-table-cell>` | A data cell within a table row, rendered as a `<td>`. |
| `<sa-table-footer>` | The footer of a table, rendered as a `<tfoot>`; typically holds summary rows. |
| `<sa-table-head>` | A header cell within a table header row, rendered as a `<th>`. |
| `<sa-table-header>` | The header section of a table, rendered as a `<thead>`; contains the header row. |
| `<sa-table-row>` | A row within a table, rendered as a `<tr>`. |
| `<sa-table-row-detail>` | The details of the table row before it, rendered as a `<tr>` with one cell spanning every column. It is collapsed until the row's `sa-table-row-detail-toggle` expands it. |
| `<sa-table-row-detail-toggle>` | The button that expands and collapses a row's details, placed in any cell of the row. It controls the `sa-table-row-detail` after its row, or the one named by an `aria-controls` attribute. Renders a chevron when it has no content. |
| `<sa-table-row-details>` | Lets the rows of the table it wraps expand to show details. Place an `sa-table-row-detail` after each expandable row and an `sa-table-row-detail-toggle` in a cell of that row. Renders the `sel-table-row-details` web component, which raises bubbling `row-detail-expand` and `row-detail-collapse` events on the detail row, so htmx attributes on an `sa-table-row-detail` can load its details with `hx-trigger="row-detail-expand once"`. |
| `<sa-table-selection>` | Adds row selection to the table it wraps. The checkbox in the table's header row acts as the select-all; a checkbox in a body row selects that row. Selected rows get `data-state="selected"`, the select-all reflects the checked/indeterminate state, and the selection posts as ordinary checkbox form data. Renders the `sel-table-selection` web component: read the current selection from its `selectedValues` property, or listen for its bubbling `selection-change` event (with the values in `event.detail.values`). |

## Attributes

### `<sa-table-row-detail>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `expanded` | `bool` | `false` | `true`, `false` |
| `colspan` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-table-row-detail-toggle>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `all` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-table-row-details>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `row-click` | `bool` | `false` | `true`, `false` |
| `expand-mode` | `TableRowDetailExpandMode` | `Multiple` | `Multiple`, `Single` |
| `emphasis` | `TableRowDetailEmphasis` | `Band` | `None`, `Band`, `Rail` |
| `inset` | `TableRowDetailInset` | `Aligned` | `Bleed`, `Aligned` |
| `animate` | `bool` | `true` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Table/_Intro.cshtml`*

```razor
<sa-table>
        <sa-table-caption>Total Active Value includes Confirmed and Pending bookings only.</sa-table-caption>
    <sa-table-header>
        <sa-table-row>
            <sa-table-head class="w-[100px]">Booking #</sa-table-head>
            <sa-table-head>Status</sa-table-head>
            <sa-table-head>Destination</sa-table-head>
            <sa-table-head class="text-right">Amount</sa-table-head>
        </sa-table-row>
    </sa-table-header>
    <sa-table-body>
        @foreach (var booking in StaticData.Bookings)
        {
            <sa-table-row>
                <sa-table-cell class="font-medium">@booking.Id</sa-table-cell>
                <sa-table-cell>
                    <sa-badge variant="@GetBadgeVariant(booking.Status)">
                        @booking.Status.ToString()
                    </sa-badge>
                </sa-table-cell>
                <sa-table-cell>@booking.Destination</sa-table-cell>
                <sa-table-cell class="text-right">
                    @booking.Amount.ToString("N")
                </sa-table-cell>
            </sa-table-row>    
        }
    </sa-table-body>
    <sa-table-footer>
        <sa-table-row>
            <sa-table-cell colspan="3">
                Total Active Value
            </sa-table-cell>
            <sa-table-cell class="text-right">
                @{
                    var activeValue = StaticData.Bookings
                        .Where(b => b.Status != BookingStatus.Cancelled)
                        .Select(b => b.Amount)
                        .Sum();
                }
                @activeValue.ToString("N")
            </sa-table-cell>
        </sa-table-row>
    </sa-table-footer>
</sa-table>

@functions
{
    BadgeVariant GetBadgeVariant(BookingStatus status) => status switch
    {
        BookingStatus.Cancelled => BadgeVariant.Destructive,
        BookingStatus.Pending => BadgeVariant.Secondary,
        _ => BadgeVariant.Default
    };
}
```

*From `Pages/Table/_Select.cshtml`*

```razor
<sa-table>
    <sa-table-caption>Total Active Value includes Confirmed and Pending bookings only.</sa-table-caption>
    <sa-table-header>
        <sa-table-row>
            <sa-table-head class="w-[100px]">Booking #</sa-table-head>
            <sa-table-head>Status</sa-table-head>
            <sa-table-head>Destination</sa-table-head>
            <sa-table-head class="text-right">Amount</sa-table-head>
        </sa-table-row>
    </sa-table-header>
    <sa-table-body>
        @foreach (var booking in StaticData.Bookings)
        {
            <sa-table-row>
                <sa-table-cell class="font-medium">@booking.Id</sa-table-cell>
                <sa-table-cell>
                    <sa-select size="SelectSize.Small">
                        @foreach (var selectListItem in bookingStatusList)
                        {
                            var selected = BookingStatus.TryParse(selectListItem.Value, out BookingStatus status) && booking.Status == status;
                            
                            <option value="@selectListItem.Value" selected="@(selected)">@selectListItem.Text</option>
                        }
                    </sa-select>
                </sa-table-cell>
                <sa-table-cell>@booking.Destination</sa-table-cell>
                <sa-table-cell class="text-right">
                    @booking.Amount.ToString("N")
                </sa-table-cell>
            </sa-table-row>    
        }
    </sa-table-body>
    <sa-table-footer>
        <sa-table-row>
            <sa-table-cell colspan="3">
                Total Active Value
            </sa-table-cell>
            <sa-table-cell class="text-right">
                @{
                    var activeValue = StaticData.Bookings
                        .Where(b => b.Status != BookingStatus.Cancelled)
                        .Select(b => b.Amount)
                        .Sum();
                }
                @activeValue.ToString("N")
            </sa-table-cell>
        </sa-table-row>
    </sa-table-footer>
</sa-table>
```

*From `Pages/Table/_RowDetails.cshtml`*

```razor
<div class="w-full overflow-hidden rounded-md border">
    <sa-table-row-details>
        <sa-table>
            <sa-table-header>
                <sa-table-row>
                    <sa-table-head class="w-px">
                        <sa-table-row-detail-toggle all="true"/>
                    </sa-table-head>
                    <sa-table-head>Booking #</sa-table-head>
                    <sa-table-head>Status</sa-table-head>
                    <sa-table-head>Destination</sa-table-head>
                    <sa-table-head class="text-right">Amount</sa-table-head>
                </sa-table-row>
            </sa-table-header>
            <sa-table-body>
                @foreach (var booking in StaticData.Bookings)
                {
                    var details = StaticData.DetailsFor(booking.Id);
                    <sa-table-row>
                        <sa-table-cell class="w-px">
                            <sa-table-row-detail-toggle aria-label="Show details for @booking.Id"/>
                        </sa-table-cell>
                        <sa-table-cell class="font-medium">@booking.Id</sa-table-cell>
                        <sa-table-cell>@booking.Status</sa-table-cell>
                        <sa-table-cell>@booking.Destination</sa-table-cell>
                        <sa-table-cell class="text-right">@booking.Amount.ToString("N2")</sa-table-cell>
                    </sa-table-row>
                    <sa-table-row-detail expanded="@(booking.Id == "TRP-4822")">
                        <dl class="grid grid-cols-2 gap-x-8 gap-y-3 sm:grid-cols-3">
                            <div class="flex flex-col gap-0.5">
                                <dt class="text-muted-foreground text-xs">Lead traveller</dt>
                                <dd>@details.Traveller</dd>
                            </div>
                            <div class="flex flex-col gap-0.5">
                                <dt class="text-muted-foreground text-xs">Email</dt>
                                <dd>@details.Email</dd>
                            </div>
                            <div class="flex flex-col gap-0.5">
                                <dt class="text-muted-foreground text-xs">Party</dt>
                                <dd>@details.Party</dd>
                            </div>
                            <div class="col-span-full flex flex-col gap-0.5">
                                <dt class="text-muted-foreground text-xs">Notes</dt>
                                <dd>@details.Notes</dd>
                            </div>
                        </dl>
                    </sa-table-row-detail>
                }
            </sa-table-body>
        </sa-table>
    </sa-table-row-details>
</div>
```

*From `Pages/Table/_RowDetailsRowClick.cshtml`*

```razor
<div class="w-full overflow-hidden rounded-md border">
    <sa-table-row-details row-click="true"
                          expand-mode="TableRowDetailExpandMode.Single"
                          emphasis="TableRowDetailEmphasis.Rail"
                          inset="TableRowDetailInset.Bleed">
        <sa-table>
            <sa-table-header>
                <sa-table-row>
                    <sa-table-head>Booking #</sa-table-head>
                    <sa-table-head>Destination</sa-table-head>
                    <sa-table-head class="text-right">Amount</sa-table-head>
                    <sa-table-head class="w-px"><span class="sr-only">Actions</span></sa-table-head>
                </sa-table-row>
            </sa-table-header>
            <sa-table-body>
                @foreach (var booking in StaticData.Bookings.Take(4))
                {
                    <sa-table-row>
                        <sa-table-cell class="font-medium">@booking.Id</sa-table-cell>
                        <sa-table-cell>@booking.Destination</sa-table-cell>
                        <sa-table-cell class="text-right">@booking.Amount.ToString("N2")</sa-table-cell>
                        <sa-table-cell class="w-px whitespace-nowrap">
                            <sa-linkbutton href="#" variant="ButtonVariant.Outline" size="ButtonSize.IconSmall"
                                           aria-label="Edit @booking.Id">
                                <sa-icon name="square-pen"/>
                            </sa-linkbutton>
                            <sa-table-row-detail-toggle aria-label="Show itinerary for @booking.Id"/>
                        </sa-table-cell>
                    </sa-table-row>
                    <sa-table-row-detail>
                        <ul class="flex flex-col divide-y">
                            @foreach (var segment in StaticData.DetailsFor(booking.Id).Itinerary)
                            {
                                <li class="flex items-center gap-4 py-2">
                                    <span class="text-muted-foreground w-16">@segment.Date</span>
                                    <span class="flex-1">
                                        <span class="font-medium">@segment.Type</span> · @segment.Description
                                    </span>
                                    <span class="tabular-nums">@segment.Amount</span>
                                </li>
                            }
                        </ul>
                    </sa-table-row-detail>
                }
            </sa-table-body>
        </sa-table>
    </sa-table-row-details>
</div>
```
