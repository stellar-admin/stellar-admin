namespace StellarAdmin.Resources;

/// <summary>
///     Specifies which resources a data source returns and how they are sorted and paged.
/// </summary>
public sealed record ResourceListRequest
{
    /// <summary>
    ///     Gets the keys of the only resources to return, or <see langword="null" /> to return all
    ///     matching resources.
    /// </summary>
    /// <remarks>
    ///     Keys are set to reload rows after an inline save, with no scope, search or paging. They
    ///     are formatted in the invariant culture. A data source that ignores them still works, but
    ///     returns more resources than needed.
    /// </remarks>
    public IReadOnlyList<string>? Keys { get; init; }

    /// <summary>
    ///     Gets the requested page, or <see langword="null" /> to return all matching resources.
    /// </summary>
    public ResourcePaging? Paging { get; init; }

    /// <summary>
    ///     Gets the identifier of the selected scope, or <see langword="null" /> when no scope is
    ///     selected.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    ///     Gets the search term, or <see langword="null" /> when the list isn't searched.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    ///     Gets the requested sort order, or <see langword="null" /> to use the data source's
    ///     default order.
    /// </summary>
    public ResourceSort? Sort { get; init; }
}
