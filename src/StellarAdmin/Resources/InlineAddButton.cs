namespace StellarAdmin.Resources;

/// <summary>
///     Specifies where the Add button for inline create is shown.
/// </summary>
public enum InlineAddButton
{
    /// <summary>
    ///     Shows the button in the page header.
    /// </summary>
    Header,

    /// <summary>
    ///     Shows the button in a row at the top or bottom of the grid, depending on the
    ///     <see cref="InlineCreatePosition" />.
    /// </summary>
    Grid,

    /// <summary>
    ///     Shows the button in the page header and in the grid.
    /// </summary>
    Both,
}
