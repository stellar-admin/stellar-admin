using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Infrastructure.Query;

/// <summary>
///     The query string values of an index page request. A <c>null</c> member was not
///     specified in the request, so defaults apply.
/// </summary>
public sealed class IndexPageRequest
{
    /// <summary>The one-based number of the requested page, or <c>null</c> when not specified.</summary>
    public int? PageNo
    {
        get;
        set => field = value is { } pageNo ? Math.Max(1, pageNo) : null;
    }

    /// <summary>The number of rows on a page, or <c>null</c> when not specified.</summary>
    public int? PageSize
    {
        get;
        set => field = value is { } pageSize ? Math.Max(1, pageSize) : null;
    }

    /// <summary>The slug of the requested scope, or <c>null</c> for the default scope.</summary>
    public string? Scope { get; set; }

    /// <summary>The search term, or <c>null</c> when the list is unfiltered.</summary>
    public string? Search { get; set; }

    /// <summary>The field to sort by, or <c>null</c> for the default sort.</summary>
    public string? SortBy { get; set; }

    /// <summary>The requested sort direction, either "asc" or "desc".</summary>
    public string? SortDir { get; set; }

    /// <summary>The requested sort direction. Anything other than "desc" sorts ascending.</summary>
    public DataGridSortDirection SortDirection => DataGridSortDirection.ParseQueryValue(SortDir);
}
