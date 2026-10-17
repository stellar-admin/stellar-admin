namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of the inline row that adds a resource to the index.
/// </summary>
public sealed class ResourceInlineCreateOptions
{
    /// <summary>
    ///     Gets or sets where the add button is shown.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the button is shown. The default is
    ///     <see cref="InlineAddButton.Both" />.
    /// </value>
    public InlineAddButton AddButton { get; set; } = InlineAddButton.Both;

    /// <summary>
    ///     Gets or sets what the grid shows after the new row is saved.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies what the grid shows. The default is
    ///     <see cref="InlineRowAfterSave.ReloadGrid" />.
    /// </value>
    public InlineRowAfterSave AfterSave { get; set; } = InlineRowAfterSave.ReloadGrid;

    /// <summary>
    ///     Gets or sets a value that indicates whether the rest of the page is dimmed while the new
    ///     row is edited.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the other rows and the page's controls are dimmed and locked
    ///     until the row is saved or canceled; otherwise, <see langword="false" />. The default is
    ///     <see langword="true" />.
    /// </value>
    public bool DimOtherRows { get; set; } = true;

    /// <summary>
    ///     Gets or sets where the new row is added.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the row is added. The default is
    ///     <see cref="InlineCreatePosition.Top" />.
    /// </value>
    public InlineCreatePosition Position { get; set; } = InlineCreatePosition.Top;
}
