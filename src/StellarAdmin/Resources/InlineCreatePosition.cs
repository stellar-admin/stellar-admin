namespace StellarAdmin.Resources;

/// <summary>
///     Where the index grid adds a new row for inline create.
/// </summary>
public enum InlineCreatePosition
{
    /// <summary>
    ///     Adds the row above the first row.
    /// </summary>
    Top,

    /// <summary>
    ///     Adds the row below the last row of the current page.
    /// </summary>
    Bottom,
}
