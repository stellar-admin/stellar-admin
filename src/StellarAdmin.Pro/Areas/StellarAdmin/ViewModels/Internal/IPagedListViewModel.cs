using System.ComponentModel;

namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels.Internal;

/// <summary>
///     For internal use to support default views.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPagedListViewModel
{
    /// <summary>The rows of the current page.</summary>
    IReadOnlyList<object> Items { get; }

    /// <summary>The one-based number of the current page.</summary>
    long PageNo { get; }

    /// <summary>The number of rows on a page.</summary>
    int PageSize { get; }

    /// <summary>The total number of rows across all pages.</summary>
    long Total { get; }

    /// <summary>The total number of pages.</summary>
    long TotalPages { get; }
}
