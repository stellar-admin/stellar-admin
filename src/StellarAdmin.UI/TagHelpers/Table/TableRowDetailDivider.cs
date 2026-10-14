namespace StellarAdmin.UI;

/// <summary>
///     The line between an expanded row and its details.
/// </summary>
public enum TableRowDetailDivider
{
    /// <summary>No line.</summary>
    None,

    /// <summary>A line that starts and ends where the details' content does.</summary>
    Inset,

    /// <summary>A line across the table, like the lines between rows.</summary>
    FullWidth,
}

internal static class TableRowDetailDividerExtensions
{
    extension(TableRowDetailDivider divider)
    {
        public string GetDataAttributeText() =>
            divider switch
            {
                TableRowDetailDivider.None => "none",
                TableRowDetailDivider.Inset => "inset",
                TableRowDetailDivider.FullWidth => "full-width",
                _ => string.Empty,
            };
    }
}
