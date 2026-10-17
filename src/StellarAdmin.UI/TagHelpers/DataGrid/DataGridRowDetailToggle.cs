namespace StellarAdmin.UI;

/// <summary>
///     Where a data grid places the buttons that expand and collapse its row details.
/// </summary>
public enum DataGridRowDetailToggle
{
    /// <summary>
    ///     Places the buttons in a column before the data columns.
    /// </summary>
    Start,

    /// <summary>
    ///     Places the buttons in a column after the data columns.
    /// </summary>
    End,

    /// <summary>
    ///     Adds no toggle column. Place an <c>sa-table-row-detail-toggle</c> in a column's item
    ///     template instead.
    /// </summary>
    None,
}
