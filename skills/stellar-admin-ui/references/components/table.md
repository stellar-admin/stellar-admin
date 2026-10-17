---
component: Table
tags: [sa-table, sa-table-body, sa-table-caption, sa-table-cell, sa-table-footer, sa-table-head, sa-table-header, sa-table-row, sa-table-row-detail, sa-table-row-detail-toggle, sa-table-selection-cell, sa-table-selection-head]
generated: true
---

# Table

Renders a table in a container that scrolls sideways when the table is too wide.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-table>` | Renders a table in a container that scrolls sideways when the table is too wide. |
| `<sa-table-body>` | Renders the body of a table, which holds the data rows, as a `<tbody>`. |
| `<sa-table-caption>` | Renders a caption that describes a table, as a `<caption>`. |
| `<sa-table-cell>` | Renders a data cell in a table row, as a `<td>`. |
| `<sa-table-footer>` | Renders the footer of a table, as a `<tfoot>`. |
| `<sa-table-head>` | Renders a header cell in a table's header row, as a `<th>`. |
| `<sa-table-header>` | Renders the header of a table, which holds the header row, as a `<thead>`. |
| `<sa-table-row>` | Renders a row in a table, as a `<tr>`. |
| `<sa-table-row-detail>` | Renders the details of the table row before it. |
| `<sa-table-row-detail-toggle>` | Renders a button that expands and collapses a table row's details. |
| `<sa-table-selection-cell>` | Renders a table cell with a checkbox that selects its row. |
| `<sa-table-selection-head>` | Renders a table header cell with a checkbox that selects or clears every row. |

## Attributes

### `<sa-table>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `row-click` | `bool` | `false` | `true`, `false` |
| `row-detail-animate` | `bool` | `true` | `true`, `false` |
| `row-detail-divider` | `TableRowDetailDivider` | `Inset` | `None`, `Inset`, `FullWidth` |
| `row-detail-emphasis` | `TableRowDetailEmphasis` | `Band` | `None`, `Band`, `Rail` |
| `row-detail-expand-mode` | `TableRowDetailExpandMode` | `Multiple` | `Multiple`, `Single` |
| `row-detail-inset` | `TableRowDetailInset` | `Aligned` | `Bleed`, `Aligned` |
| `row-details` | `bool` | `false` | `true`, `false` |
| `selection` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-table-row-detail>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `expanded` | `bool` | `false` | `true`, `false` |
| `colspan` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-table-row-detail-toggle>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `all` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-table-selection-cell>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | `false` | `true`, `false` |
| `disabled` | `bool` | `false` | `true`, `false` |
| `label` | `string` | `Select row` | — |
| `name` | `string` | — | — |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-table-selection-head>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `label` | `string` | `Select all` | — |
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
                    @booking.Amount.ToString("N2")
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
                @activeValue.ToString("N2")
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
                    @booking.Amount.ToString("N2")
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
                @activeValue.ToString("N2")
            </sa-table-cell>
        </sa-table-row>
    </sa-table-footer>
</sa-table>
```

*From `Pages/Table/_RowDetails.cshtml`*

```razor
<div class="w-full overflow-hidden rounded-lg border">
    <sa-table row-details="true">
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
</div>
```

*From `Pages/Table/_RowDetailsRowClick.cshtml`*

```razor
<div class="w-full overflow-hidden rounded-lg border">
    <sa-table row-details="true" row-click="true"
              row-detail-expand-mode="TableRowDetailExpandMode.Single"
              row-detail-emphasis="TableRowDetailEmphasis.Rail"
              row-detail-inset="TableRowDetailInset.Bleed">
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
                        <sa-button-link href="#" variant="ButtonVariant.Outline" size="ButtonSize.IconSmall"
                                       aria-label="Edit @booking.Id">
                            <sa-icon name="square-pen"/>
                        </sa-button-link>
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
</div>
```

*From `Pages/Table/_RowDetailsDivider.cshtml`*

```razor
    var dividers = new[]
    {
        (Divider: TableRowDetailDivider.Inset, Label: "Inset (default)"),
        (Divider: TableRowDetailDivider.FullWidth, Label: "FullWidth"),
        (Divider: TableRowDetailDivider.None, Label: "None"),
    };
}
<div class="flex w-full flex-col gap-6">
    @foreach (var (divider, label) in dividers)
    {
        <div class="flex flex-col gap-2">
            <p class="text-muted-foreground text-sm">@label</p>
            <div class="overflow-hidden rounded-lg border">
                <sa-table row-details="true" row-detail-divider="divider">
                    <sa-table-header>
                        <sa-table-row>
                            <sa-table-head class="w-px"><span class="sr-only">Details</span></sa-table-head>
                            <sa-table-head>Booking #</sa-table-head>
                            <sa-table-head>Destination</sa-table-head>
                            <sa-table-head class="text-right">Amount</sa-table-head>
                        </sa-table-row>
                    </sa-table-header>
                    <sa-table-body>
                        @foreach (var booking in StaticData.Bookings.Take(2))
                        {
                            var details = StaticData.DetailsFor(booking.Id);
                            <sa-table-row>
                                <sa-table-cell class="w-px">
                                    <sa-table-row-detail-toggle aria-label="Show details for @booking.Id"/>
                                </sa-table-cell>
                                <sa-table-cell class="font-medium">@booking.Id</sa-table-cell>
                                <sa-table-cell>@booking.Destination</sa-table-cell>
                                <sa-table-cell class="text-right">@booking.Amount.ToString("N2")</sa-table-cell>
                            </sa-table-row>
                            <sa-table-row-detail expanded="@(booking == StaticData.Bookings.First())">
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
                                </dl>
                            </sa-table-row-detail>
                        }
                    </sa-table-body>
                </sa-table>
            </div>
        </div>
    }
</div>
```
