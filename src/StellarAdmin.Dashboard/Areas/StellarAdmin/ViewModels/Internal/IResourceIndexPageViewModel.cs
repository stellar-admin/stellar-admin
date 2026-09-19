using System.ComponentModel;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels.Internal;

/// <summary>
///     For internal use to support default views.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResourceIndexPageViewModel
{
    /// <summary>The columns of the data grid, in order.</summary>
    IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>The label of the button that opens the create page.</summary>
    string CreateLabel { get; }

    /// <summary>The labels of the delete confirmation dialog.</summary>
    ResourceIndexDeleteDialogViewModel DeleteDialog { get; }

    /// <summary>The empty state rendered when the list is empty.</summary>
    ResourceIndexEmptyViewModel Empty { get; }

    /// <summary>The current page of rows and its paging state.</summary>
    IPagedListViewModel Page { get; }

    /// <summary>
    ///     The page sizes offered by the pager's size selector, in order; empty renders
    ///     no selector.
    /// </summary>
    IReadOnlyList<int> PageSizeOptions { get; }

    /// <summary>The scope tabs, in order; empty when no scopes are configured.</summary>
    IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; }

    /// <summary>The search box, or <c>null</c> when search is not enabled.</summary>
    ResourceIndexSearchViewModel? Search { get; }

    /// <summary>
    ///     The values the user explicitly selected. Links carry these — never effective
    ///     values — so URLs only pin what the user chose; a <c>null</c> member renders no
    ///     query parameter.
    /// </summary>
    IndexPageSelection Selection { get; }

    /// <summary>The sort applied to the list, including a configured default sort.</summary>
    ResourceIndexSortViewModel Sort { get; }

    /// <summary>The page subtitle, or <c>null</c> to render no subtitle.</summary>
    string? Subtitle { get; }

    /// <summary>The page title.</summary>
    string Title { get; }

    /// <summary>Returns the delete confirmation message for a row's entity.</summary>
    string GetDeleteMessage(object row);

    /// <summary>Returns the id of a row's entity as a string, for building row links.</summary>
    string GetRowId(object row);
}
