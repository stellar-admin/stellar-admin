using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;
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
        FindEntityByKeyAsync(id, cancellationToken, noTracking: true, includeLookups: true);

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
        var keyProperty = db
            .Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!
            .Properties.Single();
        var key = Expression.Lambda(Expression.Property(parameter, keyProperty.Name), parameter);

        if (request.Sort is { } sort)
        {
            var column = _resourceOptions.Index.Columns.Single(column =>
                column.Sortable && column.FieldName == sort.Field
            );
            query = Order(
                query,
                column.SortExpression ?? column.DisplayExpression ?? column.FieldExpression,
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

        // Lookup editors display the selected item from the reference loaded with the entity, and
        // display fields read through the references on their paths
        if (includeLookups && _resourceOptions.Edit is { } edit)
        {
            var navigations = edit
                .Fields.Select(field =>
                    field.Editor is LookupSheetEditor { Items: IEfCoreLookupReference reference }
                        ? reference.FindNavigation(entityType, field.FieldName)?.Name
                        : null
                )
                .Concat(
                    edit.ModelType == typeof(TEntity)
                        ? edit.DisplayFields.Select(field =>
                            FindReferencePath(entityType, field.PropertyPath)
                        )
                        : []
                )
                .OfType<string>()
                .Distinct(StringComparer.Ordinal);
            foreach (var navigation in navigations)
            {
                query = query.Include(navigation);
            }
        }

        return query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    // The include path of the reference navigations a property path reads through, such as
    // Supplier.Country for Supplier.Country.Name. Owned types load with their owner, so they extend
    // the path without being included themselves; a collection or plain property ends it.
    private static string? FindReferencePath(
        IEntityType entityType,
        IReadOnlyList<PropertyInfo> propertyPath
    )
    {
        var segments = new List<string>();
        string? path = null;
        IEntityType current = entityType;
        foreach (var property in propertyPath)
        {
            if (current.FindNavigation(property.Name) is not { IsCollection: false } navigation)
            {
                break;
            }

            segments.Add(navigation.Name);
            current = navigation.TargetEntityType;
            if (!current.IsOwned())
            {
                path = string.Join('.', segments);
            }
        }

        return path;
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
