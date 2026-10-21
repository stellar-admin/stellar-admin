using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class EfCoreResourceDataSource<TContext, TEntity>(
    TContext db,
    IOptions<ResourceOptions<TEntity>> options,
    ILogger<EfCoreResourceDataSource<TContext, TEntity>> logger
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
        // The database refused the delete, usually because other records still refer to this one
        catch (DbUpdateException exception)
        {
            db.Entry(entity).State = EntityState.Detached;
            logger.LogWarning(
                exception,
                "The database refused to delete {Entity} {Key}.",
                typeof(TEntity).Name,
                id
            );

            return ResourceOperationResult.ValidationFailed(
                null,
                "This record couldn't be deleted. Other records may still refer to it."
            );
        }

        return ResourceOperationResult.Success();
    }

    public Task<TEntity?> FindAsync(string id, CancellationToken cancellationToken) =>
        FindEntityByKeyAsync(id, cancellationToken, noTracking: true, includeLookups: true);

    public async Task<ResourceListResult<TEntity>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = db.Set<TEntity>().AsNoTracking();
        if (request.Keys is { } keys)
        {
            query = query.Where(EfCoreResourceKey.CreatePredicate<TEntity>(db, keys));
        }

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

        // Columns read through the references on their paths, such as Category for p => p.Category.Name,
        // and load the collections they show, such as Tags for p => p.Tags. A column that computes its
        // value loads what each path it reads needs.
        var entityType = db.Model.FindEntityType(typeof(TEntity))!;
        query = Include(
            query,
            _resourceOptions.Index.Columns.SelectMany(column =>
                EfCorePropertyPaths.FindDisplayIncludePaths(entityType, column.FieldExpression)
            )
        );

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var keyProperty = entityType.FindPrimaryKey()!.Properties.Single();
        var key = Expression.Lambda(Expression.Property(parameter, keyProperty.Name), parameter);

        if (request.Sort is { } sort)
        {
            var column = _resourceOptions.Index.Columns.Single(column =>
                column.Sortable && column.SortField == sort.Field
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
            if (field.Binder is IManyToManyFieldBinder binder)
            {
                await binder.CopyAsync(db, entity, model, cancellationToken);
                continue;
            }

            var properties = field.PropertyPath;
            object target = entity;
            object source = model;
            for (var i = 0; i < properties.Count - 1; i++)
            {
                var segment = properties[i];
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
        bool noTracking = false,
        bool includeLookups = false
    )
    {
        if (!EfCoreResourceKey.TryCreatePredicate<TEntity>(db, id, out var predicate))
        {
            return Task.FromResult<TEntity?>(null);
        }

        var entityType = db.Model.FindEntityType(typeof(TEntity))!;
        IQueryable<TEntity> query = db.Set<TEntity>();
        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        // Lookup editors display the selected item from the reference loaded with the entity, with
        // the references its text reads through, many-to-many fields show the related entities' keys,
        // and display fields read through the references on their paths and load the collections they
        // show, or those their computed values read. The edit and view pages load through the same method, so the entity carries the
        // navigations either page needs
        if (includeLookups)
        {
            var edit = _resourceOptions.Edit;
            var view = _resourceOptions.View;
            var lookupIncludes = (edit?.Fields ?? []).SelectMany(field =>
                field.Editor is LookupSheetEditor { Items: IEfCoreLookupReference reference }
                    ? reference.FindIncludePaths(entityType, field.FieldName)
                    : []
            );
            var displayFields = (
                edit?.ModelType == typeof(TEntity) ? edit.DisplayFields : []
            ).Concat(view?.ModelType == typeof(TEntity) ? view.Fields : []);
            var collections = (edit?.Fields ?? []).Select(field =>
                (field.Binder as IManyToManyFieldBinder)?.NavigationName
            );
            query = Include(
                query,
                lookupIncludes
                    .Concat(collections)
                    .Concat(
                        displayFields.SelectMany(field =>
                            EfCorePropertyPaths.FindDisplayIncludePaths(
                                entityType,
                                field.FieldExpression
                            )
                        )
                    )
            );
        }

        return query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    private static IQueryable<TEntity> Include(
        IQueryable<TEntity> query,
        IEnumerable<string?> navigations
    )
    {
        foreach (var path in navigations.OfType<string>().Distinct(StringComparer.Ordinal))
        {
            query = query.Include(path);
        }

        return query;
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
