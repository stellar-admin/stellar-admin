using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the scopes of a resource's index page.
/// </summary>
public sealed class ResourceScopesOptions
{
    /// <summary>
    ///     Gets the identifier of the scope applied when no known scope is requested.
    /// </summary>
    /// <value>
    ///     The identifier of one of the <see cref="Items" />, or <see langword="null" /> to apply
    ///     no scope. The default is <see langword="null" />.
    /// </value>
    public string? DefaultScope { get; internal set; }

    /// <summary>
    ///     Gets the scopes, in display order.
    /// </summary>
    public IReadOnlyList<ResourceScopeOptions> Items => MutableItems;

    internal ConfigurationSource? DefaultScopeSource { get; set; }

    internal List<ResourceScopeOptions> MutableItems { get; } = [];
}
