using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class EfCoreResourceDataSource<TContext, TEntity>(
    TContext db,
    IOptions<ResourceOptions<TEntity>> options
) : IResourceCrudDataSource<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly ResourceOptions<TEntity> _resourceOptions = options.Value;

    public async Task<ResourceOperationResult> CreateAsync(
        TEntity model,
        CancellationToken cancellationToken
    )
    {
        db.Set<TEntity>().Add(model);
        await db.SaveChangesAsync(cancellationToken);

        return ResourceOperationResult.Success();
    }

    public async Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var entity = await FindEntityByKeyAsync(id, cancellationToken);
        if (entity is null)
        {
            return ResourceOperationResult.NotFound();
        }

        db.Set<TEntity>().Remove(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await GetConcurrencyFailureResultAsync(id, cancellationToken);
        }

        return ResourceOperationResult.Success();
    }

    public Task<TEntity?> FindAsync(string id, CancellationToken cancellationToken) =>
        FindEntityByKeyAsync(id, cancellationToken, noTracking: true);

    public async Task<ResourceListResult<TEntity>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = db.Set<TEntity>().AsNoTracking();
        if (
            _resourceOptions.Index.Scopes?.Items.FirstOrDefault(scope => scope.Id == request.Scope)
            is EfCoreResourceScopeOptions<TEntity> { Predicate: { } predicate }
        )
        {
            query = query.Where(predicate);
        }

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
                column.SortExpression ?? column.FieldExpression,
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

    public async Task<ResourceOperationResult> UpdateAsync(
        string id,
        TEntity model,
        CancellationToken cancellationToken
    )
    {
        var edit =
            _resourceOptions.Edit
            ?? throw new InvalidOperationException(
                "Edit is not configured for the EF entity model."
            );
        var entity = await FindEntityByKeyAsync(id, cancellationToken);
        if (entity is null)
        {
            return ResourceOperationResult.NotFound();
        }

        var entityType = db.Model.FindEntityType(typeof(TEntity))!;
        foreach (var field in edit.Fields)
        {
            var properties = ResourcePropertyPath.GetProperties(field.FieldExpression)!;
            object target = entity;
            object source = model;
            foreach (var segment in properties[..^1])
            {
                target =
                    segment.GetValue(target)
                    ?? throw new InvalidOperationException(
                        $"{field.FieldName} has a null parent value on the stored entity."
                    );
                source =
                    segment.GetValue(source)
                    ?? throw new InvalidOperationException(
                        $"{field.FieldName} has a null parent value on the submitted model."
                    );
            }

            var property = EfCoreFormFieldMetadata
                .FindProperty(entityType, field.FieldName)!
                .PropertyInfo!;
            property.SetValue(target, property.GetValue(source));
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await GetConcurrencyFailureResultAsync(id, cancellationToken);
        }

        return ResourceOperationResult.Success();
    }

    private async Task<ResourceOperationResult> GetConcurrencyFailureResultAsync(
        string id,
        CancellationToken cancellationToken
    ) =>
        await FindEntityByKeyAsync(id, cancellationToken, noTracking: true) is null
            ? ResourceOperationResult.NotFound()
            : ResourceOperationResult.ValidationFailed(
                null,
                "This record changed while you were working. Reload it and try again."
            );

    private Task<TEntity?> FindEntityByKeyAsync(
        string id,
        CancellationToken cancellationToken,
        bool noTracking = false
    )
    {
        var property = db
            .Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties.Single();
        if (!TryParseResourceId(id, property.ClrType, out var key))
        {
            return Task.FromResult<TEntity?>(null);
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var predicate = Expression.Lambda<Func<TEntity, bool>>(
            Expression.Equal(
                Expression.Property(parameter, _resourceOptions.KeyPropertyName!),
                Expression.Constant(key, property.ClrType)
            ),
            parameter
        );
        IQueryable<TEntity> query = db.Set<TEntity>();
        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(predicate, cancellationToken);
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

    private static bool TryParseResourceId(string id, Type underlyingType, out object? parsedId)
    {
        if (
            underlyingType == typeof(int)
            && int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
        )
        {
            parsedId = number;
            return true;
        }
        if (
            underlyingType == typeof(long)
            && long.TryParse(
                id,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var longNumber
            )
        )
        {
            parsedId = longNumber;
            return true;
        }
        if (underlyingType == typeof(Guid) && Guid.TryParse(id, out var guid))
        {
            parsedId = guid;
            return true;
        }
        if (underlyingType == typeof(string))
        {
            parsedId = id;
            return true;
        }

        parsedId = null;
        return false;
    }
}
