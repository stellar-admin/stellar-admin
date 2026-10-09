namespace StellarAdmin.TagHelpers;

/// <summary>
///     Where a data grid places the buttons that expand and collapse its row details.
/// </summary>
public enum DataGridRowDetailToggle
{
    /// <summary>In a column before the data columns.</summary>
    Leading,

    /// <summary>In a column after the data columns.</summary>
    Trailing,

    /// <summary>
    ///     No toggle column; place an <c>sa-table-row-detail-toggle</c> in a column's item
    ///     template.
    /// </summary>
    None,
}
