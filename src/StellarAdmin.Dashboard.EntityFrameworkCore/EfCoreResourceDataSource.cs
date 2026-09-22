using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreResourceDataSource<TContext, TEntity>(
    TContext db,
    IOptions<ResourceOptions<TEntity>> options
) : IResourceDataSource<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly ResourceOptions<TEntity> _resourceOptions = options.Value;

    public async Task<ResourceListResult<TEntity>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = db.Set<TEntity>().AsNoTracking();
        if (
            request.Search is { } term
            && _resourceOptions.Index.Search is EfCoreResourceSearchOptions<TEntity> search
        )
        {
            query = query.Where(search.Predicate(term));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var key = Expression.Lambda(
            Expression.Property(parameter, _resourceOptions.KeyPropertyName!),
            parameter
        );

        if (request.Sort is { } sort)
        {
            var column = _resourceOptions.Index.Columns.Single(column =>
                column.Sortable && column.FieldName == sort.Field
            );
            query = Order(
                query,
                column.FieldExpression,
                sort.Direction == ResourceSortDirection.Descending
                    ? nameof(Queryable.OrderByDescending)
                    : nameof(Queryable.OrderBy)
            );
            query = Order(query, key, nameof(Queryable.ThenBy));
        }
        else
        {
            query = Order(query, key, nameof(Queryable.OrderBy));
        }

        if (request.Paging is { } paging)
        {
            query = query.Skip(checked((paging.Page - 1) * paging.PageSize)).Take(paging.PageSize);
        }

        return new(await query.ToListAsync(cancellationToken), totalCount);
    }

    private static IQueryable<TEntity> Order(
        IQueryable<TEntity> query,
        LambdaExpression selector,
        string method
    ) =>
        query.Provider.CreateQuery<TEntity>(
            Expression.Call(
                typeof(Queryable),
                method,
                [typeof(TEntity), selector.ReturnType],
                query.Expression,
                Expression.Quote(selector)
            )
        );
}
