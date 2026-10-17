namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the index page settings that the query string supplies.
/// </summary>
public sealed class ResourceIndexQuery
{
    /// <summary>
    ///     Gets or sets the number of the requested page, starting at 1.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    ///     Gets or sets the requested number of resources on a page.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the selected scope, or <see langword="null" /> for the
    ///     default scope.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    ///     Gets or sets the search term, or <see langword="null" /> when the list isn't searched.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    ///     Gets or sets the name of the field to sort by, or <see langword="null" /> for the
    ///     default sort order.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     Gets or sets the requested sort direction.
    /// </summary>
    /// <value>
    ///     <c>asc</c> or <c>desc</c>, in any case, or <see langword="null" /> to sort in ascending
    ///     order.
    /// </value>
    public string? SortDirection { get; set; }
}
