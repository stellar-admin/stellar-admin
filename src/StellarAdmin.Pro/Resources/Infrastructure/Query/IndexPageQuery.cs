using StellarAdmin.Pro.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.Resources.Infrastructure.Query;

/// <summary>
///     Runs the query behind an index page.
/// </summary>
public static class IndexPageQuery
{
    /// <summary>
    ///     Applies the page's configuration and the request to the query, and returns the
    ///     requested page of entities.
    /// </summary>
    public static IndexPageResult<TEntity> Execute<TEntity>(
        IndexPageOptions<TEntity> options,
        IQueryable<TEntity> query,
        IndexPageRequest request
    )
        where TEntity : class
    {
        return ExecuteAsync(
                options,
                query,
                request,
                q => Task.FromResult(q.Count()),
                q => Task.FromResult(q.ToList())
            )
            .GetAwaiter()
            .GetResult();
    }

    internal static async Task<IndexPageResult<TEntity>> ExecuteAsync<TEntity>(
        IndexPageOptions<TEntity> options,
        IQueryable<TEntity> query,
        IndexPageRequest request,
        Func<IQueryable<TEntity>, Task<int>> count,
        Func<IQueryable<TEntity>, Task<List<TEntity>>> materialize
    )
        where TEntity : class
    {
        // Allows the host to modify the initial query
        if (options.QueryTransform is { } transform)
        {
            query = transform(query);
        }

        // Applies the active scope filter. The scope counts as selected only when the
        // request named it and it resolved — an unknown slug heals to the default scope
        // but must not round-trip in links.
        var activeScope = options.ResolveScope(request.Scope);
        var selectedScopeSlug =
            activeScope is not null
            && string.Equals(activeScope.Slug, request.Scope, StringComparison.OrdinalIgnoreCase)
                ? activeScope.Slug
                : null;
        if (activeScope?.Predicate is { } predicate)
        {
            query = query.Where(predicate);
        }

        // Applies the search term through the configured search query; when search is
        // not enabled the request parameter is ignored entirely.
        var search = request.Search;
        if (options.Search is not { } searchOptions)
        {
            search = null;
        }
        else if (!string.IsNullOrEmpty(search))
        {
            query = searchOptions.Query(query, search);
        }

        // The sort counts as selected only when the requested field resolved to a
        // sortable column; the direction only when the request also carried one —
        // omitting it is lossless because ascending is the default.
        var sortColumn = options.ResolveSortableColumn(request.SortBy);
        string? activeSortField = null;
        string? selectedSortDirection = null;
        var sortDirection = DataGridSortDirection.Ascending;
        if (sortColumn is not null)
        {
            sortDirection = request.SortDirection;
            query = query.OrderByField(
                sortColumn.DisplayExpression ?? sortColumn.FieldExpression,
                sortDirection
            );
            activeSortField = sortColumn.FieldName;
            selectedSortDirection = string.IsNullOrEmpty(request.SortDir)
                ? null
                : sortDirection.GetQueryValueText();
        }
        else if (options.DefaultSort is { } defaultSort)
        {
            sortDirection = defaultSort.Descending
                ? DataGridSortDirection.Descending
                : DataGridSortDirection.Ascending;
            query = query.OrderByField(defaultSort.FieldExpression, sortDirection);
            // Mark the column the default sort corresponds to, if any, as the active
            // sort so its header shows the direction and toggles like an explicit sort.
            if (defaultSort.FieldName is { } defaultSortField)
            {
                activeSortField = options.ResolveSortableColumn(defaultSortField)?.FieldName;
            }
        }

        var total = await count(query);

        var pageSize = request.PageSize ?? options.DefaultPageSize;

        // Clamp to the last page when the request points past it - deleting the last
        // entity of the last page must not leave the user on an empty page.
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var pageNo = Math.Max(1, Math.Min(request.PageNo ?? 1, totalPages));

        var items = await materialize(query.Skip((pageNo - 1) * pageSize).Take(pageSize));

        return new IndexPageResult<TEntity>(
            items,
            total,
            pageNo,
            pageSize,
            activeScope?.Slug,
            activeSortField,
            sortDirection,
            search,
            new IndexPageSelection(
                selectedScopeSlug,
                sortColumn?.FieldName,
                selectedSortDirection,
                request.PageSize
            )
        );
    }
}
