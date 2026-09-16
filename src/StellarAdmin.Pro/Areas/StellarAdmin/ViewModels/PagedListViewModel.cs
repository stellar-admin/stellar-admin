using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels.Internal;

namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;

/// <summary>
///     A page of a paged list, together with its paging state.
/// </summary>
/// <param name="Items">The rows of the current page.</param>
/// <param name="Total">The total number of rows across all pages.</param>
/// <param name="PageNo">The one-based number of the current page.</param>
/// <param name="PageSize">The number of rows on a page.</param>
public record PagedListViewModel<TModel>(List<TModel> Items, long Total, long PageNo, int PageSize)
    : IPagedListViewModel
    where TModel : class
{
    /// <summary>The total number of pages.</summary>
    public long TotalPages => (long)Math.Ceiling(Total / (double)PageSize);

    IReadOnlyList<object> IPagedListViewModel.Items => Items;
}
