namespace StellarAdmin.Resources;

/// <summary>
///     Where the Add button for inline create is shown.
/// </summary>
public enum InlineAddButton
{
    /// <summary>
    ///     Shows the button in the page header.
    /// </summary>
    Header,

    /// <summary>
    ///     Shows the button in the grid, in the first row for <see cref="InlineCreatePosition.Top" />
    ///     and the last row for <see cref="InlineCreatePosition.Bottom" />.
    /// </summary>
    Grid,

    /// <summary>
    ///     Shows the button in the page header and in the grid.
    /// </summary>
    Both,
}
