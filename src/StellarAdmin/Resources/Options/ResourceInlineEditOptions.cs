namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures editing a resource in its index grid row.
/// </summary>
public sealed class ResourceInlineEditOptions
{
    /// <summary>
    ///     Whether the other rows and the page's controls are dimmed and locked while a row is edited,
    ///     until it is saved or cancelled. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows { get; set; } = true;
}
