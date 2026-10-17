namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of editing a resource in its index row.
/// </summary>
public sealed class ResourceInlineEditOptions
{
    /// <summary>
    ///     Gets what the grid shows after a row is saved.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies what the grid shows. The default is
    ///     <see cref="InlineRowAfterSave.ReloadGrid" />.
    /// </value>
    public InlineRowAfterSave AfterSave { get; internal set; } = InlineRowAfterSave.ReloadGrid;

    /// <summary>
    ///     Gets a value that indicates whether the rest of the page is dimmed while a row
    ///     is edited.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the other rows and the page's controls are dimmed and locked
    ///     until the row is saved or canceled; otherwise, <see langword="false" />. The default is
    ///     <see langword="true" />.
    /// </value>
    public bool DimOtherRows { get; internal set; } = true;
}
