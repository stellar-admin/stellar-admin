namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures adding a resource as a new row in its index grid.
/// </summary>
public sealed class ResourceInlineCreateOptions
{
    /// <summary>
    ///     Where the Add button is shown. Defaults to <see cref="InlineAddButton.Both" />.
    /// </summary>
    public InlineAddButton AddButton { get; set; } = InlineAddButton.Both;

    /// <summary>
    ///     What the grid shows after the new row is saved. Defaults to
    ///     <see cref="InlineRowAfterSave.ReloadGrid" />.
    /// </summary>
    public InlineRowAfterSave AfterSave { get; set; } = InlineRowAfterSave.ReloadGrid;

    /// <summary>
    ///     Whether the other rows and the page's controls are dimmed and locked while the new row is edited,
    ///     until it is saved or cancelled. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows { get; set; } = true;

    /// <summary>
    ///     Where the new row is added. Defaults to <see cref="InlineCreatePosition.Top" />.
    /// </summary>
    public InlineCreatePosition Position { get; set; } = InlineCreatePosition.Top;
}
