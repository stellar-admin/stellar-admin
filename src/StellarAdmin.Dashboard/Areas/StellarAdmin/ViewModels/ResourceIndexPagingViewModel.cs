namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The current page and available paging controls.
/// </summary>
public sealed record ResourceIndexPagingViewModel(
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages,
    IReadOnlyList<int> PageSizes
);
