using StellarAdmin.Pro.Resources.Infrastructure.Expressions;

namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured options for an index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageOptions<TEntity>
    where TEntity : class
{
    private readonly List<DataGridColumnOptions> _columns = [];
    private readonly IndexPageDefaults _defaults;
    private readonly List<IndexPageScopeOptions<TEntity>> _scopes = [];

    /// <summary>The columns of the data grid, in order.</summary>
    public IReadOnlyList<DataGridColumnOptions> Columns => _columns;

    /// <summary>The number of rows per page when the request does not specify a page size.</summary>
    public int DefaultPageSize { get; internal set; } = 25;

    /// <summary>The default sort that is applied when none is specified.</summary>
    public DataGridDefaultSortOptions? DefaultSort { get; internal set; }

    /// <summary>The label of the button that opens the create page.</summary>
    public string EffectiveCreateLabel => _defaults.CreateLabel;

    /// <summary>The scope applied when the request does not specify one.</summary>
    public IndexPageScopeOptions<TEntity>? EffectiveDefaultScope =>
        _scopes.FirstOrDefault(scope => scope.IsDefault) ?? _scopes.FirstOrDefault();

    /// <summary>The description rendered when the list is empty.</summary>
    public string EffectiveEmptyDescription => _defaults.EmptyDescription;

    /// <summary>The name of the icon rendered when the list is empty.</summary>
    public string EffectiveEmptyIcon => _defaults.EmptyIcon;

    /// <summary>The title rendered when the list is empty.</summary>
    public string EffectiveEmptyTitle => _defaults.EmptyTitle;

    /// <summary>The page title to display.</summary>
    public string EffectiveTitle => Title ?? _defaults.Title;

    /// <summary>
    ///     The page sizes offered by the pager's size selector, in order; empty renders
    ///     no selector.
    /// </summary>
    public IReadOnlyList<int> PageSizeOptions { get; internal set; } = [25, 50, 100];

    /// <summary>Transforms the query before any other filter is applied.</summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? QueryTransform { get; internal set; }

    /// <summary>The scopes of the page, in order.</summary>
    public IReadOnlyList<IndexPageScopeOptions<TEntity>> Scopes => _scopes;

    /// <summary>The search options, or <c>null</c> when search is not enabled.</summary>
    public IndexPageSearchOptions<TEntity>? Search { get; internal set; }

    /// <summary>The page subtitle, or <c>null</c> to render no subtitle.</summary>
    public string? Subtitle { get; internal set; }

    /// <summary>The page title.</summary>
    /// <remarks>Defaults to the title of the resource.</remarks>
    public string? Title { get; internal set; }

    public IndexPageOptions(IndexPageDefaults defaults)
    {
        _defaults = defaults;

        foreach (var propertyName in defaults.Columns)
        {
            if (FieldExpressionHelper.BuildFieldExpression<TEntity>(propertyName) is not { } field)
            {
                continue;
            }

            _columns.Add(new DataGridColumnOptions(field, propertyName) { Sortable = true });

            if (propertyName == defaults.SortBy)
            {
                DefaultSort = new DataGridDefaultSortOptions(
                    field,
                    propertyName,
                    descending: false
                );
            }
        }
    }

    /// <summary>
    ///     Returns the scope for the slug, or the default scope when the slug matches none.
    /// </summary>
    public IndexPageScopeOptions<TEntity>? ResolveScope(string? slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return EffectiveDefaultScope;
        }

        // An unknown scope falls back to the default scope (rather than an unfiltered
        // list) so bookmarked URLs keep working when a scope is removed.
        return _scopes.FirstOrDefault(scope =>
                string.Equals(scope.Slug, slug, StringComparison.OrdinalIgnoreCase)
            ) ?? EffectiveDefaultScope;
    }

    /// <summary>
    ///     Returns the sortable column for the field name, or <c>null</c> when there is none.
    /// </summary>
    public DataGridColumnOptions? ResolveSortableColumn(string? fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return null;
        }

        // An unknown or non-sortable sort field is ignored (the page renders unsorted)
        // so bookmarked URLs keep working when a column is removed.
        return _columns.FirstOrDefault(column =>
            column.Sortable
            && string.Equals(column.FieldName, fieldName, StringComparison.OrdinalIgnoreCase)
        );
    }

    internal void AddColumn(DataGridColumnOptions column)
    {
        _columns.Add(column);
    }

    internal void AddScope(IndexPageScopeOptions<TEntity> scope)
    {
        _scopes.Add(scope);
    }

    internal void ClearColumns()
    {
        _columns.Clear();
    }

    internal void ClearScopes()
    {
        _scopes.Clear();
    }
}
