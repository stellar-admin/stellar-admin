namespace StellarAdmin.UI;

/// <summary>
///     Specifies the line between an expanded data grid row and its details.
/// </summary>
public enum DataGridRowDetailDivider
{
    /// <summary>
    ///     No line.
    /// </summary>
    None,

    /// <summary>
    ///     A line that starts and ends where the details' content does.
    /// </summary>
    Inset,

    /// <summary>
    ///     A line across the grid, like the lines between rows.
    /// </summary>
    FullWidth,
}

internal static class DataGridRowDetailDividerExtensions
{
    extension(DataGridRowDetailDivider divider)
    {
        public string GetDataAttributeText() =>
            divider switch
            {
                DataGridRowDetailDivider.None => "none",
                DataGridRowDetailDivider.Inset => "inset",
                DataGridRowDetailDivider.FullWidth => "full-width",
                _ => string.Empty,
            };
    }
}
