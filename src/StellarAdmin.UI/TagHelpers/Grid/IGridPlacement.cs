namespace StellarAdmin.UI;

// The span and start attributes of an element placed in sa-grid or sa-details.
internal interface IGridPlacement
{
    string? ColumnSpan { get; }

    string? ColumnSpanLarge { get; }

    string? ColumnSpanMedium { get; }

    string? ColumnSpanSmall { get; }

    string? ColumnStart { get; }

    string? ColumnStartLarge { get; }

    string? ColumnStartMedium { get; }

    string? ColumnStartSmall { get; }
}
