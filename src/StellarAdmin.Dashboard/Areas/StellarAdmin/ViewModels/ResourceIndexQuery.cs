namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The index settings supplied in the query string.
/// </summary>
public sealed class ResourceIndexQuery
{
    /// <summary>
    ///     The requested one-based page number.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    ///     The requested number of resources per page.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    ///     The selected scope identifier, or null when no scope is selected.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    ///     The search term, or null when the list is unfiltered.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    ///     The requested sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     The requested sort direction: asc or desc.
    /// </summary>
    public string? SortDirection { get; set; }
}
