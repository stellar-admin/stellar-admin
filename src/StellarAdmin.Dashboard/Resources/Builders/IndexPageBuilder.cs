using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Infrastructure.Expressions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexPageOptions<TEntity> _options;

    /// <summary>Sets the number of rows per page when the user has not chosen a page size.</summary>
    /// <remarks>Defaults to 25.</remarks>
    public int DefaultPageSize
    {
        get => _options.DefaultPageSize;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

            _options.DefaultPageSize = value;
        }
    }

    /// <summary>Sets the page sizes offered by the pager's size selector, in order.</summary>
    /// <remarks>Defaults to 25, 50 and 100. An empty list renders no selector.</remarks>
    public IReadOnlyList<int> PageSizeOptions
    {
        get => _options.PageSizeOptions;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Any(size => size < 1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Page size options must be greater than or equal to 1."
                );
            }

            _options.PageSizeOptions = value;
        }
    }

    /// <summary>Sets the page subtitle, rendered below the title.</summary>
    /// <remarks>Defaults to <c>null</c>, which renders no subtitle.</remarks>
    public string? Subtitle
    {
        get => _options.Subtitle;
        set => _options.Subtitle = value;
    }

    /// <summary>Sets the page title.</summary>
    /// <remarks>Defaults to <c>null</c>, which uses the title of the resource.</remarks>
    public string? Title
    {
        get => _options.Title;
        set => _options.Title = value;
    }

    internal IndexPageBuilder(IndexPageOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>Configures the columns of the data grid.</summary>
    public IndexPageBuilder<TEntity> Columns(Action<IndexPageColumnsBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new IndexPageColumnsBuilder<TEntity>(_options));

        return this;
    }

    /// <summary>
    ///     Sorts the list ascending by the property selected by <paramref name="field" />
    ///     when the request does not specify a sort.
    /// </summary>
    /// <remarks>Repeat calls replace the default sort.</remarks>
    public IndexPageBuilder<TEntity> DefaultSortBy<TProp>(Expression<Func<TEntity, TProp>> field)
    {
        return SetDefaultSort(field, descending: false);
    }

    /// <summary>
    ///     Sorts the list descending by the property selected by <paramref name="field" />
    ///     when the request does not specify a sort.
    /// </summary>
    /// <remarks>Repeat calls replace the default sort.</remarks>
    public IndexPageBuilder<TEntity> DefaultSortByDescending<TProp>(
        Expression<Func<TEntity, TProp>> field
    )
    {
        return SetDefaultSort(field, descending: true);
    }

    /// <summary>
    ///     Enables the search box. Without this call the page renders no search box.
    /// </summary>
    /// <param name="query">
    ///     Applies the search term to the query, like
    ///     <c>(items, term) => items.Where(x => x.Email!.Contains(term))</c>.
    /// </param>
    /// <param name="configure">Configures the search settings, like the placeholder.</param>
    /// <remarks>Repeat calls replace the search configuration.</remarks>
    public IndexPageBuilder<TEntity> EnableSearch(
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> query,
        Action<IndexPageSearchBuilder<TEntity>>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var searchOptions = new IndexPageSearchOptions<TEntity>(query);
        configure?.Invoke(new IndexPageSearchBuilder<TEntity>(searchOptions));

        _options.Search = searchOptions;

        return this;
    }

    /// <summary>
    ///     Configures the scopes of the page — the named slices of the list rendered as
    ///     tabs above the data grid.
    /// </summary>
    public IndexPageBuilder<TEntity> Scopes(Action<IndexPageScopesBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new IndexPageScopesBuilder<TEntity>(_options));

        return this;
    }

    /// <summary>
    ///     Transforms the query before the page executes it, for example to eagerly load a
    ///     navigation property or to filter the listed entities.
    /// </summary>
    /// <remarks>Repeat calls compose: each transform wraps the previous one.</remarks>
    public IndexPageBuilder<TEntity> TransformQuery(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> transform
    )
    {
        ArgumentNullException.ThrowIfNull(transform);

        var existing = _options.QueryTransform;
        _options.QueryTransform = existing is null ? transform : q => transform(existing(q));

        return this;
    }

    private IndexPageBuilder<TEntity> SetDefaultSort(LambdaExpression field, bool descending)
    {
        ArgumentNullException.ThrowIfNull(field);

        _options.DefaultSort = new DataGridDefaultSortOptions(
            field,
            FieldExpressionHelper.ExtractFieldName(field),
            descending
        );

        return this;
    }
}
