namespace StellarAdmin.UI;

/// <summary>
///     Specifies the line between an expanded data grid row and its details.
/// </summary>
[GenerateDataAttributeText]
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
