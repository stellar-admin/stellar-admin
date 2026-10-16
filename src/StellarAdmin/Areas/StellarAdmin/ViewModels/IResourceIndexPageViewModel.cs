using System.Collections;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The index page model, without the resource type.
///     Use it as the <c>@model</c> of a shared index view that you override for every resource.
///     To override one resource's index view, use <see cref="ResourceIndexPageViewModel{TResource}" />.
/// </summary>
public interface IResourceIndexPageViewModel
{
    /// <summary>
    ///     Whether resources can be created.
    /// </summary>
    bool CanCreate { get; }

    /// <summary>
    ///     Whether resources can be edited.
    /// </summary>
    bool CanEdit { get; }

    /// <summary>
    ///     Whether resources can be viewed.
    /// </summary>
    bool CanView { get; }

    /// <summary>
    ///     The columns displayed in order.
    /// </summary>
    IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>
    ///     The create button label.
    /// </summary>
    string CreateLabel { get; }

    /// <summary>
    ///     The delete confirmation dialog.
    /// </summary>
    ResourceIndexDeleteDialogViewModel? Delete { get; }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    string? DeleteLabel { get; }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    string? EditLabel { get; }

    /// <summary>
    ///     The inline rows, or null when resources are neither edited nor created inline.
    /// </summary>
    ResourceIndexInlineRowsViewModel? InlineRows { get; }

    /// <summary>
    ///     The resources to display.
    /// </summary>
    IEnumerable Items { get; }

    /// <summary>
    ///     The paging controls, or null when paging is disabled.
    /// </summary>
    ResourceIndexPagingViewModel? Paging { get; }

    /// <summary>
    ///     The index settings explicitly supplied in the query string.
    /// </summary>
    ResourceIndexQuery Query { get; }

    /// <summary>
    ///     The row details settings, or null when rows do not expand.
    /// </summary>
    ResourceRowDetailsOptions? RowDetails { get; }

    /// <summary>
    ///     Whether only the grid's rows are rendered, for a row request.
    /// </summary>
    bool RowsOnly { get; }

    /// <summary>
    ///     The scope tabs.
    /// </summary>
    IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; }

    /// <summary>
    ///     The search box, or null when searching is disabled.
    /// </summary>
    ResourceIndexSearchViewModel? Search { get; }

    /// <summary>
    ///     The selected ordering.
    /// </summary>
    ResourceIndexSortViewModel? Sort { get; }

    /// <summary>
    ///     The page title.
    /// </summary>
    string Title { get; }

    /// <summary>
    ///     The view link label.
    /// </summary>
    string? ViewLabel { get; }

    /// <summary>
    ///     Returns the key used in resource links.
    /// </summary>
    string GetKey(object resource);
}
