namespace StellarAdmin.UI;

/// <summary>
///     Specifies where the content of a table row's details starts.
/// </summary>
[GenerateDataAttributeText]
public enum TableRowDetailInset
{
    /// <summary>
    ///     The content starts at the row's first cell.
    /// </summary>
    Bleed,

    /// <summary>
    ///     The content starts at the column after the toggle's column.
    /// </summary>
    Aligned,
}
