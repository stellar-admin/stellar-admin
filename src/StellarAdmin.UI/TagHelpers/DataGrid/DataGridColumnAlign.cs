namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a data grid column aligns its head and cells.
/// </summary>
[GenerateDataAttributeText]
public enum DataGridColumnAlign
{
    /// <summary>
    ///     Aligns the head and cells to the start of the column.
    /// </summary>
    Start,

    /// <summary>
    ///     Centres the head and cells in the column.
    /// </summary>
    Center,

    /// <summary>
    ///     Aligns the head and cells to the end of the column, as for numbers.
    /// </summary>
    End,
}
