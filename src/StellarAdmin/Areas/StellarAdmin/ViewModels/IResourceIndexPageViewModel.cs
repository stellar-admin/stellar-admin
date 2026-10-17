using System.Collections;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the model of an index page, without the resource type.
/// </summary>
/// <remarks>
///     Use it as the <c>@model</c> of an index view that you override for every resource. To
///     override one resource's index view, use
///     <see cref="ResourceIndexPageViewModel{TResource}" />.
/// </remarks>
public interface IResourceIndexPageViewModel
{
    /// <summary>
    ///     Gets a value that indicates whether the resource has a create page.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the resource has a create page; otherwise,
    ///     <see langword="false" />.
    /// </value>
    bool CanCreate { get; }

    /// <summary>
    ///     Gets a value that indicates whether the resource has an edit page.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the resource has an edit page; otherwise,
    ///     <see langword="false" />.
    /// </value>
    bool CanEdit { get; }

    /// <summary>
    ///     Gets a value that indicates whether the resource has a view page.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the resource has a view page; otherwise,
    ///     <see langword="false" />.
    /// </value>
    bool CanView { get; }

    /// <summary>
    ///     Gets the columns of the grid, in display order.
    /// </summary>
    IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>
    ///     Gets the label of the create button.
    /// </summary>
    string CreateLabel { get; }

    /// <summary>
    ///     Gets the delete confirmation dialog, or <see langword="null" /> when resources can't be
    ///     deleted.
    /// </summary>
    ResourceIndexDeleteDialogViewModel? Delete { get; }

    /// <summary>
    ///     Gets the label of the delete button.
    /// </summary>
    string? DeleteLabel { get; }

    /// <summary>
    ///     Gets the label of the edit link.
    /// </summary>
    string? EditLabel { get; }

    /// <summary>
    ///     Gets the settings of the inline rows, or <see langword="null" /> when resources are
    ///     neither edited nor created inline.
    /// </summary>
    ResourceIndexInlineRowsViewModel? InlineRows { get; }

    /// <summary>
    ///     Gets the resources on the current page.
    /// </summary>
    IEnumerable Items { get; }

    /// <summary>
    ///     Gets the paging controls, or <see langword="null" /> when paging is turned off.
    /// </summary>
    ResourceIndexPagingViewModel? Paging { get; }

    /// <summary>
    ///     Gets the index settings that the query string supplies.
    /// </summary>
    ResourceIndexQuery Query { get; }

    /// <summary>
    ///     Gets the row details settings, or <see langword="null" /> when rows don't expand.
    /// </summary>
    ResourceRowDetailsOptions? RowDetails { get; }

    /// <summary>
    ///     Gets a value that indicates whether only the grid's rows are rendered.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the request renders only the grid's rows, such as after an
    ///     inline save; otherwise, <see langword="false" />.
    /// </value>
    bool RowsOnly { get; }

    /// <summary>
    ///     Gets the scope tabs, or an empty list when the resource has no scopes.
    /// </summary>
    IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; }

    /// <summary>
    ///     Gets the search box, or <see langword="null" /> when search is turned off.
    /// </summary>
    ResourceIndexSearchViewModel? Search { get; }

    /// <summary>
    ///     Gets the sort order of the list, or <see langword="null" /> when the list is unsorted.
    /// </summary>
    ResourceIndexSortViewModel? Sort { get; }

    /// <summary>
    ///     Gets the page title.
    /// </summary>
    string Title { get; }

    /// <summary>
    ///     Gets the label of the view link.
    /// </summary>
    string? ViewLabel { get; }

    /// <summary>
    ///     Returns the key of a resource, for use in links.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>The key of the resource.</returns>
    string GetKey(object resource);
}
