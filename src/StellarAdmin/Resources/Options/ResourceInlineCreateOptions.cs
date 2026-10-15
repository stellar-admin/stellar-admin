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
    ///     Whether the other rows are dimmed while the new row is edited. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows { get; set; } = true;

    /// <summary>
    ///     Where the new row is added. Defaults to <see cref="InlineCreatePosition.Top" />.
    /// </summary>
    public InlineCreatePosition Position { get; set; } = InlineCreatePosition.Top;
}
