namespace StellarAdmin.Resources;

/// <summary>
///     The requested resources and listing settings.
/// </summary>
public sealed record ResourceListRequest
{
    /// <summary>
    ///     The keys of the only resources to return, or null to return all matching resources.
    ///     Set to reload rows after an inline save, without a scope, search or paging.
    /// </summary>
    /// <remarks>
    ///     Keys are formatted in the invariant culture. A data source that ignores them still
    ///     works, since the rows are picked from the result by key, but returns more than needed.
    /// </remarks>
    public IReadOnlyList<string>? Keys { get; init; }

    /// <summary>
    ///     The requested page, or null to return all matching resources.
    /// </summary>
    public ResourcePaging? Paging { get; init; }

    /// <summary>
    ///     The selected scope identifier, or null when no scope is selected.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    ///     The search term, or null when the list is unfiltered.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    ///     The requested ordering, or null to use the data source's default.
    /// </summary>
    public ResourceSort? Sort { get; init; }
}
