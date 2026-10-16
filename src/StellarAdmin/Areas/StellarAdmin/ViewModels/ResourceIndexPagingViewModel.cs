namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The current page and available paging controls.
/// </summary>
public sealed class ResourceIndexPagingViewModel
{
    /// <summary>
    ///     The current page number.
    /// </summary>
    public int Page { get; }

    /// <summary>
    ///     The number of resources per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     The page sizes the user can choose from.
    /// </summary>
    public IReadOnlyList<int> PageSizes { get; }

    /// <summary>
    ///     The number of resources across all pages.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    ///     The number of pages.
    /// </summary>
    public long TotalPages { get; }

    internal ResourceIndexPagingViewModel(
        int page,
        int pageSize,
        long totalCount,
        long totalPages,
        IReadOnlyList<int> pageSizes
    )
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
        PageSizes = pageSizes;
    }
}
