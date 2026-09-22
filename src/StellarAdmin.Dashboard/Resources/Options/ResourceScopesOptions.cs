namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures resource index scopes.
/// </summary>
public sealed class ResourceScopesOptions
{
    /// <summary>
    ///     The scope applied when no recognized scope is requested.
    /// </summary>
    public string? DefaultScope { get; set; }

    /// <summary>
    ///     The scopes displayed in order.
    /// </summary>
    public IList<ResourceScopeOptions> Items { get; } = new List<ResourceScopeOptions>();
}
