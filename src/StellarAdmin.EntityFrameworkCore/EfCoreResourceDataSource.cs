using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class EfCoreResourceDataSource<TEntity>(
    IServiceProvider services,
    EfCoreContexts contexts,
    IOptions<ResourceOptions<TEntity>> options,
    ResourceReferences references,
    EfCoreResourceTitles titles,
    IOptions<EfCoreResourceOptions<TEntity>> efCoreOptions,
    ILogger<EfCoreResourceDataSource<TEntity>> logger
) : IResourceCrudDataSource<TEntity>
    where TEntity : class
{
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";

    private readonly ResourceOptions<TEntity> _resourceOptions = options.Value;
    private DbContext? _db;

    // Found on first use, since an unavailable resource's data source is still created and may have no context
    private DbContext Db => _db ??= contexts.Get<TEntity>(services);

    public Task<ResourceOperationResult> CreateAsync(
        TEntity model,
        CancellationToken cancellationToken
    ) => CreateAsync(model, EfCoreFormEntity.Self(typeof(TEntity)), null, cancellationToken);

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

        Db.Set<TEntity>().Remove(entity);
        try
        {
            await Db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await GetConcurrencyFailureResultAsync(id, cancellationToken);
        }
        // The database refused the delete, usually because other records still refer to this one
        catch (DbUpdateException exception)
        {
            Db.Entry(entity).State = EntityState.Detached;
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
        var query = Db.Set<TEntity>().AsNoTracking();
        if (request.Keys is { } keys)
        {
            query = query.Where(EfCoreResourceKey.CreatePredicate<TEntity>(Db, keys));
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
        // value loads what each path it reads needs, and one that shows related records loads what their
        // title reads, such as Category.Parent.
        var entityType = Db.Model.FindEntityType(typeof(TEntity))!;
        query = Include(
            query,
            _resourceOptions.Index.Columns.SelectMany(column =>
                FindDisplayIncludePaths(entityType, column.FieldExpression)
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

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        TEntity model,
        CancellationToken cancellationToken
    ) => UpdateAsync(id, model, EfCoreFormEntity.Self(typeof(TEntity)), null, cancellationToken);

    // Adds the entity the form's model holds, runs the create events around saving it, and returns its key
    internal async Task<ResourceOperationResult> CreateAsync<TModel>(
        TModel model,
        EfCoreFormEntity form,
        EfCoreCreateEvents<TEntity, TModel>? events,
        CancellationToken cancellationToken
    )
    {
        var entity = (TEntity)form.GetEntity(model!);
        Db.Set<TEntity>().Add(entity);

        var context = new EfCoreSaveContext<TEntity, TModel>(Db, entity, model, services);
        if (
            await SaveAsync(
                context,
                events is null ? null : events.CreatingAsync,
                cancellationToken
            ) is
            { } failed
        )
        {
            return failed;
        }

        if (events is not null)
        {
            await RunSavedEventAsync(
                () => events.CreatedAsync(new(Db, entity, model, services), cancellationToken),
                "created",
                cancellationToken
            );
        }

        return _resourceOptions.KeySelector is { } key
            ? ResourceOperationResult.Success(key(entity))
            : ResourceOperationResult.Success();
    }

    // The edit form's model for a stored entity, which isn't tracked, after the loaded event
    internal async Task<TModel?> FindAsync<TModel>(
        string id,
        EfCoreFormEntity form,
        EfCoreEditEvents<TEntity, TModel>? events,
        CancellationToken cancellationToken
    )
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity is null)
        {
            return default;
        }

        var model = form.Property is null ? (TModel)(object)entity : CreateModel<TModel>();
        form.SetEntity(model!, entity);
        if (events is not null)
        {
            await events.LoadedAsync(new(Db, entity, model, services), cancellationToken);
        }

        return model;
    }

    // Runs the created event's delegate for a new form's model
    internal Task InitializeAsync<TModel>(
        TModel model,
        EfCoreFormEntity form,
        EfCoreCreateEvents<TEntity, TModel> events,
        CancellationToken cancellationToken
    ) =>
        events.InitializedAsync(
            new(Db, (TEntity)form.GetEntity(model!), model, services),
            cancellationToken
        );

    // Copies the form's fields of the entity onto the stored one, and runs the update events around saving it. While
    // the events run, the model holds the stored entity; a failed save gives it back the submitted one, which the form
    // shows again
    internal async Task<ResourceOperationResult> UpdateAsync<TModel>(
        string id,
        TModel model,
        EfCoreFormEntity form,
        EfCoreEditEvents<TEntity, TModel>? events,
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

        var submitted = (TEntity)form.GetEntity(model!);
        await CopyFieldsAsync(edit, form, entity, submitted, cancellationToken);

        form.SetEntity(model!, entity);
        var context = new EfCoreSaveContext<TEntity, TModel>(Db, entity, model, services);
        ResourceOperationResult? failed;
        try
        {
            failed = await SaveAsync(
                context,
                events is null ? null : events.UpdatingAsync,
                cancellationToken
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            failed = await GetConcurrencyFailureResultAsync(id, cancellationToken);
        }

        if (failed is not null)
        {
            form.SetEntity(model!, submitted);

            return failed;
        }

        if (events is not null)
        {
            await RunSavedEventAsync(
                () => events.UpdatedAsync(new(Db, entity, model, services), cancellationToken),
                "updated",
                cancellationToken
            );
        }

        return ResourceOperationResult.Success();
    }

    private static TModel CreateModel<TModel>() => Activator.CreateInstance<TModel>();

    private async Task CopyFieldsAsync(
        ResourceEditOptions edit,
        EfCoreFormEntity form,
        TEntity target,
        TEntity source,
        CancellationToken cancellationToken
    )
    {
        var entityType = Db.Model.FindEntityType(typeof(TEntity))!;
        foreach (var field in edit.Fields)
        {
            if (form.FindEntityPath(field) is not { } properties)
            {
                continue;
            }

            if (field.Binder is IManyToManyFieldBinder binder)
            {
                await binder.CopyAsync(Db, target, source, cancellationToken);
                continue;
            }

            object targetValue = target;
            object sourceValue = source;
            for (var i = 0; i < properties.Count - 1; i++)
            {
                var segment = properties[i];
                targetValue =
                    segment.GetValue(targetValue)
                    ?? throw new InvalidOperationException(
                        $"{field.FieldName} has a null parent value on the stored entity."
                    );
                sourceValue =
                    segment.GetValue(sourceValue)
                    ?? throw new InvalidOperationException(
                        $"{field.FieldName} has a null parent value on the submitted model."
                    );
            }

            var property = EfCoreFormFieldMetadata
                .FindProperty(
                    entityType,
                    string.Join('.', properties.Select(segment => segment.Name))
                )!
                .PropertyInfo!;
            property.SetValue(targetValue, property.GetValue(sourceValue));
        }
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
        if (!EfCoreResourceKey.TryCreatePredicate<TEntity>(Db, id, out var predicate))
        {
            return Task.FromResult<TEntity?>(null);
        }

        var entityType = Db.Model.FindEntityType(typeof(TEntity))!;
        IQueryable<TEntity> query = Db.Set<TEntity>();
        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        // Lookup editors display the selected item from the reference loaded with the entity, with
        // the references its text reads through, many-to-many fields show the related entities' keys,
        // and display fields read through the references on their paths and load the collections they
        // show, or those their computed values read, and what the title of related records they show reads. The edit and view pages load through the same method, so the entity carries the
        // navigations either page needs
        if (includeLookups)
        {
            // A model that wraps the entity names its fields and reads its display fields through the entity's property
            var edit = _resourceOptions.Edit;
            var view = _resourceOptions.View;
            var editEntity = EfCoreResourceOptions<TEntity>.Find(
                edit,
                efCoreOptions.Value.EditEntity
            );
            var editFields = editEntity is null ? [] : edit!.Fields;
            var lookupIncludes = editFields.SelectMany(field =>
                field.Editor is LookupSheetEditor lookup
                && (
                    lookup.Items
                    ?? (lookup.Resource is { } resource ? references.GetItems(resource) : null)
                )
                    is IEfCoreLookupReference reference
                && editEntity!.FindEntityFieldName(field) is { } fieldName
                    ? reference.FindIncludePaths(entityType, fieldName)
                    : []
            );
            var displayExpressions = (editEntity is null ? [] : edit!.DisplayFields)
                .Select(field => editEntity!.FindEntityExpression(field.FieldExpression))
                .Concat(
                    (view?.ModelType == typeof(TEntity) ? view.Fields : []).Select(field =>
                        field.FieldExpression
                    )
                )
                .OfType<LambdaExpression>();
            var collections = editFields.Select(field =>
                (field.Binder as IManyToManyFieldBinder)?.NavigationName
            );
            query = Include(
                query,
                lookupIncludes
                    .Concat(collections)
                    .Concat(
                        displayExpressions.SelectMany(expression =>
                            FindDisplayIncludePaths(entityType, expression)
                        )
                    )
            );
        }

        return query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    // An after-save event can't undo the save, so its exception is logged, and the form continues as saved
    private async Task RunSavedEventAsync(
        Func<Task> saved,
        string operation,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await saved();
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException
                || !cancellationToken.IsCancellationRequested
            )
        {
            logger.LogError(
                exception,
                "The {Operation} event of {Entity} failed after the record was saved.",
                operation,
                typeof(TEntity).Name
            );
        }
    }

    // Runs the saving event, then saves unless the event saved the record itself. With an event, both run in one
    // transaction, through the context's execution strategy, so a failed event rolls back what it saved. A failed save
    // forgets the tracked changes, so nothing in them is saved later in the request
    private async Task<ResourceOperationResult?> SaveAsync<TModel>(
        EfCoreSaveContext<TEntity, TModel> context,
        Func<EfCoreSaveContext<TEntity, TModel>, CancellationToken, Task>? saving,
        CancellationToken cancellationToken
    )
    {
        if (saving is null)
        {
            await Db.SaveChangesAsync(cancellationToken);

            return null;
        }

        try
        {
            return await Db
                .Database.CreateExecutionStrategy()
                .ExecuteAsync(
                    async cancellationToken =>
                    {
                        // The in-memory provider has no transactions, and a transaction the app started includes the save
                        await using var transaction =
                            Db.Database.CurrentTransaction is null
                            && Db.Database.ProviderName != InMemoryProviderName
                                ? await Db.Database.BeginTransactionAsync(cancellationToken)
                                : null;

                        await saving(context, cancellationToken);
                        if (context.Errors.Count > 0)
                        {
                            Db.ChangeTracker.Clear();

                            return ResourceOperationResult.ValidationFailed(context.Errors);
                        }

                        if (!context.IsHandled)
                        {
                            await Db.SaveChangesAsync(cancellationToken);
                        }

                        if (transaction is not null)
                        {
                            await transaction.CommitAsync(cancellationToken);
                        }

                        return null;
                    },
                    cancellationToken
                );
        }
        catch
        {
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private IEnumerable<string> FindDisplayIncludePaths(
        IEntityType entityType,
        LambdaExpression expression
    ) =>
        EfCorePropertyPaths
            .FindDisplayIncludePaths(entityType, expression)
            .Concat(
                EfCorePropertyPaths.FindTitleIncludePaths(
                    entityType,
                    expression,
                    titles.FindIncludePaths
                )
            );

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
