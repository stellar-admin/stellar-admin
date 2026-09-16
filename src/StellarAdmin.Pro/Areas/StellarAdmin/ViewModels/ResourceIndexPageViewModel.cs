using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Pro.Resources.Infrastructure.Query;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The view model of a resource index page. A view overriding one of the default
///     index views can declare this as its model to work with the entity type directly.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class ResourceIndexPageViewModel<TEntity> : IResourceIndexPageViewModel
    where TEntity : class
{
    private readonly DeleteOptions<TEntity> _deleteOptions;
    private readonly Func<TEntity, string> _rowId;

    /// <summary>The columns of the data grid, in order.</summary>
    public IReadOnlyList<DataGridColumnOptions> Columns { get; }

    /// <summary>The label of the button that opens the create page.</summary>
    public string CreateLabel { get; }

    /// <summary>The labels of the delete confirmation dialog.</summary>
    public ResourceIndexDeleteDialogViewModel DeleteDialog { get; }

    /// <summary>The empty state rendered when the list is empty.</summary>
    public ResourceIndexEmptyViewModel Empty { get; }

    /// <summary>The current page of rows and its paging state.</summary>
    public PagedListViewModel<TEntity> Page { get; }

    /// <summary>
    ///     The page sizes offered by the pager's size selector, in order; empty renders
    ///     no selector.
    /// </summary>
    public IReadOnlyList<int> PageSizeOptions { get; }

    /// <summary>The scope tabs, in order; empty when no scopes are configured.</summary>
    public IReadOnlyList<ResourceIndexScopeViewModel> Scopes { get; }

    /// <summary>The search box, or <c>null</c> when search is not enabled.</summary>
    public ResourceIndexSearchViewModel? Search { get; }

    /// <summary>
    ///     The values the user explicitly selected. Links carry these — never effective
    ///     values — so URLs only pin what the user chose; a <c>null</c> member renders no
    ///     query parameter.
    /// </summary>
    public IndexPageSelection Selection { get; }

    /// <summary>The sort applied to the list, including a configured default sort.</summary>
    public ResourceIndexSortViewModel Sort { get; }

    /// <summary>The page subtitle, or <c>null</c> to render no subtitle.</summary>
    public string? Subtitle { get; }

    /// <summary>The page title.</summary>
    public string Title { get; }

    IPagedListViewModel IResourceIndexPageViewModel.Page => Page;

    public ResourceIndexPageViewModel(
        IndexPageResult<TEntity> result,
        IndexPageOptions<TEntity> indexPageOptions,
        DeleteOptions<TEntity> deleteOptions,
        Func<TEntity, string> rowId
    )
    {
        _deleteOptions = deleteOptions;
        _rowId = rowId;

        Columns = indexPageOptions.Columns;
        CreateLabel = indexPageOptions.EffectiveCreateLabel;
        DeleteDialog = new ResourceIndexDeleteDialogViewModel(
            deleteOptions.Title,
            deleteOptions.ConfirmLabel,
            deleteOptions.CancelLabel
        );
        Empty = new ResourceIndexEmptyViewModel(
            indexPageOptions.EffectiveEmptyIcon,
            indexPageOptions.EffectiveEmptyTitle,
            indexPageOptions.EffectiveEmptyDescription
        );
        Page = new PagedListViewModel<TEntity>(
            result.Items,
            result.Total,
            result.PageNo,
            result.PageSize
        );
        PageSizeOptions = indexPageOptions.PageSizeOptions;

        var defaultScope = indexPageOptions.EffectiveDefaultScope;
        Scopes = indexPageOptions
            .Scopes.Select(scope => new ResourceIndexScopeViewModel(
                scope.Slug,
                scope.Title,
                string.Equals(
                    scope.Slug,
                    result.ActiveScopeSlug,
                    StringComparison.OrdinalIgnoreCase
                ),
                ReferenceEquals(scope, defaultScope) ? null : scope.Slug
            ))
            .ToList();

        Search = indexPageOptions.Search is { } searchOptions
            ? new ResourceIndexSearchViewModel(result.Search, searchOptions.EffectivePlaceholder)
            : null;
        Selection = result.Selection;
        Sort = new ResourceIndexSortViewModel(result.ActiveSortField, result.SortDirection);
        Subtitle = indexPageOptions.Subtitle;
        Title = indexPageOptions.EffectiveTitle;
    }

    /// <summary>Returns the delete confirmation message for an entity.</summary>
    public string GetDeleteMessage(TEntity row)
    {
        return _deleteOptions.FormatMessage(row);
    }

    /// <summary>Returns the id of an entity as a string, for building row links.</summary>
    public string GetRowId(TEntity row)
    {
        return _rowId(row);
    }

    string IResourceIndexPageViewModel.GetDeleteMessage(object row)
    {
        return _deleteOptions.FormatMessage((TEntity)row);
    }

    string IResourceIndexPageViewModel.GetRowId(object row)
    {
        return _rowId((TEntity)row);
    }
}
