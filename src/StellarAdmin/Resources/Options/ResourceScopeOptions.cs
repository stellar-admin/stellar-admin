using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Describes a scope, which is shown as a tab that filters a resource's index page.
/// </summary>
public class ResourceScopeOptions
{
    /// <summary>
    ///     Gets the identifier passed to the data source when the scope is selected.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     Gets the label of the scope's tab.
    /// </summary>
    public required string Title { get; init; }

    internal ConfigurationSource? Source { get; set; }
}
