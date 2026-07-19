---
component: Table
tags: [sa-table, sa-table-body, sa-table-caption, sa-table-cell, sa-table-footer, sa-table-head, sa-table-header, sa-table-row]
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
    var bookingStatusList = Html.GetEnumSelectList<BookingStatus>();
}
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
