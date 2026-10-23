using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides extension methods that add resources that EF Core loads and saves.
/// </summary>
public static class StellarAdminBuilderExtensions
{
    /// <summary>
    ///     Adds a resource that EF Core loads and saves.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The builder to add the resource to.</param>
    /// <returns>A builder that configures the resource.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The URL slug of the resource is the kebab-case plural of the entity type's name, such as
    ///     <c>order-items</c> for <c>OrderItem</c>. <typeparamref name="TEntity" /> must be mapped
    ///     by one registered database context, or by the one <c>UseDbContext</c> picks, with a
    ///     single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />.
    ///     Fields of a form that binds the entity must be settable properties that aren't keys,
    ///     concurrency tokens or generated values, or many-to-many navigations without a join
    ///     entity type, which bind as the keys of the related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged.
    /// </remarks>
    public static EfCoreResourceBuilder<TEntity> AddEfCoreResource<TEntity>(
        this StellarAdminBuilder builder
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ConfigureEfCoreResource<TEntity>(builder, builder.AddResource<TEntity>());
    }

    /// <summary>
    ///     Adds a resource that EF Core loads and saves under a URL slug.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The builder to add the resource to.</param>
    /// <param name="slug">
    ///     The URL segment of the resource. It uses lowercase letters and digits, optionally
    ///     separated by single hyphens.
    /// </param>
    /// <returns>A builder that configures the resource.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" /> or <paramref name="slug" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     <typeparamref name="TEntity" /> must be mapped by one registered database context, or
    ///     by the one <c>UseDbContext</c> picks, with a single public primary key property of type
    ///     <see cref="int" />, <see cref="long" />, <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged. A slug that isn't valid, or that another resource
    ///     uses, also makes the resource unavailable.
    /// </remarks>
    public static EfCoreResourceBuilder<TEntity> AddEfCoreResource<TEntity>(
        this StellarAdminBuilder builder,
        string slug
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ConfigureEfCoreResource<TEntity>(builder, builder.AddResource<TEntity>(slug));
    }

    /// <summary>
    ///     Adds and configures a resource that EF Core loads and saves.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The builder to add the resource to.</param>
    /// <param name="configure">A delegate that configures the resource.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The URL slug of the resource is the kebab-case plural of the entity type's name, such as
    ///     <c>order-items</c> for <c>OrderItem</c>. <typeparamref name="TEntity" /> must be mapped
    ///     by one registered database context, or by the one <c>UseDbContext</c> picks, with a
    ///     single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />.
    ///     Fields of a form that binds the entity must be settable properties that aren't keys,
    ///     concurrency tokens or generated values, or many-to-many navigations without a join
    ///     entity type, which bind as the keys of the related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged.
    /// </remarks>
    public static StellarAdminBuilder AddEfCoreResource<TEntity>(
        this StellarAdminBuilder builder,
        Action<EfCoreResourceBuilder<TEntity>> configure
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(builder.AddEfCoreResource<TEntity>());

        return builder;
    }

    /// <summary>
    ///     Adds and configures a resource that EF Core loads and saves under a URL slug.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The builder to add the resource to.</param>
    /// <param name="slug">
    ///     The URL segment of the resource. It uses lowercase letters and digits, optionally
    ///     separated by single hyphens.
    /// </param>
    /// <param name="configure">A delegate that configures the resource.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" />, <paramref name="slug" /> or <paramref name="configure" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     <typeparamref name="TEntity" /> must be mapped by one registered database context, or
    ///     by the one <c>UseDbContext</c> picks, with a single public primary key property of type
    ///     <see cref="int" />, <see cref="long" />, <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged. A slug that isn't valid, or that another resource
    ///     uses, also makes the resource unavailable.
    /// </remarks>
    public static StellarAdminBuilder AddEfCoreResource<TEntity>(
        this StellarAdminBuilder builder,
        string slug,
        Action<EfCoreResourceBuilder<TEntity>> configure
    )
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(builder.AddEfCoreResource<TEntity>(slug));

        return builder;
    }

    private static EfCoreResourceBuilder<TEntity> ConfigureEfCoreResource<TEntity>(
        this StellarAdminBuilder builder,
        ResourceBuilder<TEntity> resource
    )
        where TEntity : class
    {
        resource.UseDataSource<EfCoreResourceDataSource<TEntity>>();
        builder.Services.TryAddSingleton<EfCoreContexts>();
        builder.Services.TryAddSingleton<EfCoreResourceTitles>();
        builder.Services.AddSingleton(
            new EfCoreResourceTitle(
                typeof(TEntity),
                services =>
                    services.GetRequiredService<IOptions<ResourceOptions<TEntity>>>().Value.Title
            )
        );
        builder
            .Services.AddOptions<ResourceOptions<TEntity>>()
            .PostConfigure<
                IServiceScopeFactory,
                EfCoreContexts,
                IOptions<EfCoreResourceOptions<TEntity>>,
                IOptions<EfCoreResourceReferenceOptions<TEntity>>
            >(
                (options, scopeFactory, contexts, efCore, reference) =>
                {
                    var name = typeof(TEntity).Name;
                    var contextType = efCore.Value.ContextType;
                    if (contextType is null)
                    {
                        var matches = contexts.Find(typeof(TEntity));
                        if (matches is not [var match])
                        {
                            options.ReportUnavailable(
                                matches.Count > 1
                                    ? EfCoreConfigurationEvents.AmbiguousDbContext
                                    : EfCoreConfigurationEvents.EntityNotMapped,
                                $"{options.PluralLabel} is hidden, and its pages return 404, because {contexts.DescribeMissing(typeof(TEntity), matches)}.",
                                matches.Count > 1
                                    ? $"Call `UseDbContext<{matches[0].Name}>()` on the resource to pick a context."
                                    : $"Add a `DbSet<{name}>` to a DbContext registered with `AddDbContext`."
                            );

                            return;
                        }

                        contextType = match;
                    }

                    // Options are cached. Use a separate scope to read metadata without retaining a DbContext.
                    using var scope = scopeFactory.CreateScope();
                    var db = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
                    if (db.Model.FindEntityType(typeof(TEntity)) is not { } entity)
                    {
                        options.ReportUnavailable(
                            EfCoreConfigurationEvents.EntityNotMapped,
                            $"{options.PluralLabel} is hidden, and its pages return 404, because `{contextType.Name}` has no `{name}` entity.",
                            $"Add a `DbSet<{name}>` to `{contextType.Name}`, or pick another context with `UseDbContext`."
                        );

                        return;
                    }

                    var keys = entity.FindPrimaryKey()?.Properties;
                    if (
                        keys is not { Count: 1 }
                        || keys[0].PropertyInfo is not { GetMethod.IsPublic: true } property
                        || !(
                            property.PropertyType == typeof(int)
                            || property.PropertyType == typeof(long)
                            || property.PropertyType == typeof(Guid)
                            || property.PropertyType == typeof(string)
                        )
                    )
                    {
                        options.ReportUnavailable(
                            EfCoreConfigurationEvents.UnsupportedPrimaryKey,
                            $"{options.PluralLabel} is hidden, and its pages return 404, because `{typeof(TEntity).Name}` has no primary key the admin panel supports.",
                            $"Give `{typeof(TEntity).Name}` one primary-key property with a public getter, of type `int`, `long`, `Guid` or `string`."
                        );

                        return;
                    }

                    options.UseKey(property);
                    DisableEntitySorting(options, entity);
                    EfCoreResourceReference.Configure(
                        options,
                        reference.Value,
                        db,
                        contextType,
                        property
                    );

                    if (options.Create is { } create && create.ModelType == typeof(TEntity))
                    {
                        InferReferencedResources(entity, create);
                        UseManyToManyBinders(options, entity, create, contextType);
                        RemoveUnsupportedFormFields(options, entity, create);
                    }
                    if (options.Edit is { } edit && edit.ModelType == typeof(TEntity))
                    {
                        InferReferencedResources(entity, edit);
                        UseManyToManyBinders(options, entity, edit, contextType);
                        RemoveUnsupportedFormFields(options, entity, edit);
                    }
                }
            );

        return new(builder.Services);
    }

    // A many-to-many navigation without an entity type for its join table binds as the keys of its related entities.
    // A navigation that can't be bound, or an editor that selects one item, leaves the field out. A field with no
    // editor gets a multi-lookup sheet once its related entity's resource is found.
    private static void UseManyToManyBinders<TEntity>(
        ResourceOptions<TEntity> options,
        IEntityType entity,
        ResourceFormOptions form,
        Type contextType
    )
    {
        foreach (var field in form.Fields)
        {
            if (
                field.PropertyPath.Count != 1
                || entity.FindSkipNavigation(field.FieldName) is not { } navigation
            )
            {
                continue;
            }

            if (
                navigation.PropertyInfo is not { } property
                || navigation.TargetEntityType.FindPrimaryKey()?.Properties
                    is not [{ PropertyInfo: { } key }]
            )
            {
                form.RemoveField(field);
                options.ReportProblem(
                    EfCoreConfigurationEvents.UnsupportedManyToManyField,
                    $"The `{field.FieldName}` field isn't on the {FormName(options, form)} form because `{typeof(TEntity).Name}.{field.FieldName}` is a many-to-many field without a navigation property, or its related entities don't have one primary-key property.",
                    $"Give `{typeof(TEntity).Name}` a navigation property for `{field.FieldName}`, and its related entities one primary-key property.",
                    field
                );
                continue;
            }

            // A built-in single-value editor would show nothing selected and save at most one related entity
            if (
                field.Editor is not null
                && field.Editor.GetType().Assembly == typeof(FieldEditor).Assembly
                && field.Editor
                    is not (CheckboxGroupEditor or ToggleGroupEditor or MultiLookupSheetEditor)
            )
            {
                form.RemoveField(field);
                options.ReportProblem(
                    EfCoreConfigurationEvents.SingleValueManyToManyEditor,
                    $"The `{field.FieldName}` field isn't on the {FormName(options, form)} form because `{typeof(TEntity).Name}.{field.FieldName}` is a many-to-many field, and its editor selects one item.",
                    "Call `UseCheckboxGroupEditor`, `UseToggleGroupEditor` or `UseMultiLookupSheetEditor` on the field.",
                    field
                );
                continue;
            }

            var binderType = typeof(ManyToManyFieldBinder<,>).MakeGenericType(
                navigation.TargetEntityType.ClrType,
                key.PropertyType
            );
            field.UseBinder(
                (FieldBinder)Activator.CreateInstance(binderType, contextType, property, key)!
            );
        }
    }

    // A choice or lookup editor without items on a foreign key or many-to-many field takes them from the related
    // entity's resource, and a field with no editor gets a lookup sheet for it
    private static void InferReferencedResources(IEntityType entity, ResourceFormOptions form)
    {
        foreach (var field in form.Fields.Where(field => field.PropertyPath.Count == 1))
        {
            var related =
                entity.FindSkipNavigation(field.FieldName)?.TargetEntityType
                ?? (
                    entity
                        .FindProperty(field.FieldName)
                        ?.GetContainingForeignKeys()
                        .Where(foreignKey => foreignKey.Properties.Count == 1)
                        .Select(foreignKey => foreignKey.PrincipalEntityType)
                        .Distinct()
                        .ToArray()
                        is [var principal]
                        ? principal
                        : null
                );
            if (related is { HasSharedClrType: false })
            {
                field.InferResource(related.ClrType);
            }
        }
    }

    // A collection or a related record has no single value to order rows by, unless the column sorts by an expression
    // of its own
    private static void DisableEntitySorting<TEntity>(
        ResourceOptions<TEntity> options,
        IEntityType entity
    )
    {
        foreach (
            var column in options.Index.Columns.Where(column =>
                column is { Sortable: true, SortExpression: null }
            )
        )
        {
            if (EfCorePropertyPaths.GetPropertyPath(column.FieldExpression) is not { } path)
            {
                continue;
            }

            if (!EfCorePropertyPaths.CanInclude(entity, path))
            {
                column.DisableSorting();
                options.ReportProblem(
                    EfCoreConfigurationEvents.SortableCollectionColumn,
                    $"The `{column.FieldName}` column isn't sortable because it reads a collection.",
                    "Remove `Sortable()`, or sort the column by an expression with `Sortable(selector)`.",
                    column
                );
            }
            else if (EfCorePropertyPaths.EndsAtNavigation(entity, path))
            {
                column.DisableSorting();
                options.ReportProblem(
                    EfCoreConfigurationEvents.SortableNavigationColumn,
                    $"The `{column.FieldName}` column isn't sortable because it shows a related record.",
                    "Sort the column by one of the record's properties with `Sortable(selector)`.",
                    column
                );
            }
        }
    }

    // Keys, concurrency tokens, generated values and properties without a public setter can't be saved from a form
    private static void RemoveUnsupportedFormFields<TEntity>(
        ResourceOptions<TEntity> options,
        IEntityType entity,
        ResourceFormOptions form
    )
    {
        foreach (
            var field in form.Fields.Where(field => field.Binder is not IManyToManyFieldBinder)
        )
        {
            var property = EfCoreFormFieldMetadata.FindProperty(entity, field.FieldName);
            var name = $"`{typeof(TEntity).Name}.{field.FieldName}`";
            var problem = property switch
            {
                null => (
                    $"{name} isn't a property that EF Core maps",
                    "Remove the field from the form."
                ),
                _ when property.IsPrimaryKey() => (
                    $"{name} is the primary key",
                    "Remove the field from the form."
                ),
                { IsConcurrencyToken: true } => (
                    $"{name} is a concurrency token",
                    "Remove the field from the form."
                ),
                _ when property.ValueGenerated != ValueGenerated.Never => (
                    $"the database generates {name}",
                    "Remove the field from the form."
                ),
                { PropertyInfo: not { SetMethod.IsPublic: true } } => (
                    $"{name} has no public setter",
                    $"Add a public setter to {name}, or remove the field from the form."
                ),
                _ => ((string Cause, string Fix)?)null,
            };
            if (problem is var (cause, fix))
            {
                form.RemoveField(field);
                options.ReportProblem(
                    EfCoreConfigurationEvents.UnsupportedFormField,
                    $"The `{field.FieldName}` field isn't on the {FormName(options, form)} form because {cause}.",
                    fix,
                    field
                );
            }
        }
    }

    private static string FormName<TEntity>(
        ResourceOptions<TEntity> options,
        ResourceFormOptions form
    ) => form == options.Create ? "create" : "edit";
}
