namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the paging controls of an index page.
/// </summary>
public sealed class ResourceIndexPagingViewModel
{
    /// <summary>
    ///     Gets the number of the current page, starting at 1.
    /// </summary>
    public int Page { get; }

    /// <summary>
    ///     Gets the number of resources on a page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     Gets the page sizes that the user can choose from.
    /// </summary>
    public IReadOnlyList<int> PageSizes { get; }

    /// <summary>
    ///     Gets the number of resources on all pages.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    ///     Gets the number of pages.
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
