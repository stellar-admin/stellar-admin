namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures editing a resource in its index grid row.
/// </summary>
public sealed class ResourceInlineEditOptions
{
    /// <summary>
    ///     Whether the other rows are dimmed while a row is edited. Defaults to <c>true</c>.
    /// </summary>
    public bool DimOtherRows { get; set; } = true;
}
