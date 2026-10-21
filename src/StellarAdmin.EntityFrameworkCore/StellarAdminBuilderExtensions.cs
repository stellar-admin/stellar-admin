using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
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
    /// <typeparam name="TContext">
    ///     The type of the database context that holds the entities.
    /// </typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The builder to add the resource to.</param>
    /// <returns>A builder that configures the resource.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The URL slug of the resource is the kebab-case plural of the entity type's name, such as
    ///     <c>order-items</c> for <c>OrderItem</c>. <typeparamref name="TEntity" /> must be mapped
    ///     in <typeparamref name="TContext" />, with a single public primary key property of type
    ///     <see cref="int" />, <see cref="long" />, <see cref="Guid" /> or <see cref="string" />.
    ///     Fields of a form that binds the entity must be settable properties that aren't keys,
    ///     concurrency tokens or generated values, or many-to-many navigations without a join
    ///     entity type, which bind as the keys of the related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged.
    /// </remarks>
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminBuilder builder
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ConfigureEfCoreResource<TContext, TEntity>(builder, builder.AddResource<TEntity>());
    }

    /// <summary>
    ///     Adds a resource that EF Core loads and saves under a URL slug.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The type of the database context that holds the entities.
    /// </typeparam>
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
    ///     <typeparamref name="TEntity" /> must be mapped in <typeparamref name="TContext" />, with
    ///     a single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged. A slug that isn't valid, or that another resource
    ///     uses, also makes the resource unavailable.
    /// </remarks>
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminBuilder builder,
        string slug
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ConfigureEfCoreResource<TContext, TEntity>(
            builder,
            builder.AddResource<TEntity>(slug)
        );
    }

    /// <summary>
    ///     Adds and configures a resource that EF Core loads and saves.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The type of the database context that holds the entities.
    /// </typeparam>
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
    ///     in <typeparamref name="TContext" />, with a single public primary key property of type
    ///     <see cref="int" />, <see cref="long" />, <see cref="Guid" /> or <see cref="string" />.
    ///     Fields of a form that binds the entity must be settable properties that aren't keys,
    ///     concurrency tokens or generated values, or many-to-many navigations without a join
    ///     entity type, which bind as the keys of the related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged.
    /// </remarks>
    public static StellarAdminBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminBuilder builder,
        Action<EfCoreResourceBuilder<TContext, TEntity>> configure
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(builder.AddEfCoreResource<TContext, TEntity>());

        return builder;
    }

    /// <summary>
    ///     Adds and configures a resource that EF Core loads and saves under a URL slug.
    /// </summary>
    /// <typeparam name="TContext">
    ///     The type of the database context that holds the entities.
    /// </typeparam>
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
    ///     <typeparamref name="TEntity" /> must be mapped in <typeparamref name="TContext" />, with
    ///     a single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the resource is unavailable or the field is left out, and the problem is logged. A slug that isn't valid, or that another resource
    ///     uses, also makes the resource unavailable.
    /// </remarks>
    public static StellarAdminBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminBuilder builder,
        string slug,
        Action<EfCoreResourceBuilder<TContext, TEntity>> configure
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(builder.AddEfCoreResource<TContext, TEntity>(slug));

        return builder;
    }

    private static EfCoreResourceBuilder<TContext, TEntity> ConfigureEfCoreResource<
        TContext,
        TEntity
    >(this StellarAdminBuilder builder, ResourceBuilder<TEntity> resource)
        where TContext : DbContext
        where TEntity : class
    {
        resource.UseDataSource<EfCoreResourceDataSource<TContext, TEntity>>();
        builder
            .Services.AddOptions<ResourceOptions<TEntity>>()
            .PostConfigure<IServiceScopeFactory>(
                (options, scopeFactory) =>
                {
                    // Options are cached. Use a separate scope to read metadata without retaining a DbContext.
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TContext>();
                    if (db.Model.FindEntityType(typeof(TEntity)) is not { } entity)
                    {
                        options.ReportUnavailable(
                            EfCoreConfigurationEvents.EntityNotMapped,
                            $"{typeof(TEntity).Name} is not mapped in {typeof(TContext).Name}. Add it to the context's model."
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
                            $"{typeof(TEntity).Name} has no supported primary key. EF resources require one public CLR primary-key property of type int, long, Guid, or string."
                        );

                        return;
                    }

                    options.UseKey(property);
                    DisableCollectionSorting(options, entity);

                    if (options.Create is { } create && create.ModelType == typeof(TEntity))
                    {
                        UseManyToManyBinders<TContext, TEntity>(options, entity, create);
                        RemoveUnsupportedFormFields(options, entity, create);
                    }
                    if (options.Edit is { } edit && edit.ModelType == typeof(TEntity))
                    {
                        UseManyToManyBinders<TContext, TEntity>(options, entity, edit);
                        RemoveUnsupportedFormFields(options, entity, edit);
                    }
                }
            );

        return new(builder.Services);
    }

    // A many-to-many navigation without an entity type for its join table binds as the keys of its related entities.
    // A navigation that can't be bound, or an editor that selects one item, leaves the field out.
    private static void UseManyToManyBinders<TContext, TEntity>(
        ResourceOptions<TEntity> options,
        IEntityType entity,
        ResourceFormOptions form
    )
        where TContext : DbContext
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
                    $"{typeof(TEntity).Name}.{field.FieldName} cannot be used as an EF resource form field. A many-to-many field requires a CLR navigation property and related entities with one CLR primary-key property.",
                    "The field is left out.",
                    field
                );
                continue;
            }

            // A built-in single-value editor would show nothing selected and save at most one related entity
            if (
                field.Editor is null
                || field.Editor.GetType().Assembly == typeof(FieldEditor).Assembly
                    && field.Editor
                        is not (CheckboxGroupEditor or ToggleGroupEditor or MultiLookupSheetEditor)
            )
            {
                form.RemoveField(field);
                options.ReportProblem(
                    EfCoreConfigurationEvents.SingleValueManyToManyEditor,
                    $"{typeof(TEntity).Name}.{field.FieldName} is a many-to-many field, so it needs an editor that selects several items. Call UseEditor<CheckboxGroupEditor>, UseEditor<ToggleGroupEditor> or UseEditor<MultiLookupSheetEditor>.",
                    "The field is left out.",
                    field
                );
                continue;
            }

            var binderType = typeof(ManyToManyFieldBinder<,,>).MakeGenericType(
                typeof(TContext),
                navigation.TargetEntityType.ClrType,
                key.PropertyType
            );
            field.UseBinder((FieldBinder)Activator.CreateInstance(binderType, property, key)!);
        }
    }

    // A collection has no single value to order rows by, unless the column sorts by an expression of its own
    private static void DisableCollectionSorting<TEntity>(
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
            if (
                EfCorePropertyPaths.GetPropertyPath(column.FieldExpression) is { } path
                && !EfCorePropertyPaths.CanInclude(entity, path)
            )
            {
                column.DisableSorting();
                options.ReportProblem(
                    EfCoreConfigurationEvents.SortableCollectionColumn,
                    $"The {typeof(TEntity).Name} column '{column.FieldName}' reads a collection, so it can't be sortable. Remove Sortable(), or sort it by an expression with Sortable(selector).",
                    "The column isn't sortable.",
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
            if (
                property?.PropertyInfo is not { SetMethod.IsPublic: true }
                || property.IsPrimaryKey()
                || property.IsConcurrencyToken
                || property.ValueGenerated != ValueGenerated.Never
            )
            {
                form.RemoveField(field);
                options.ReportProblem(
                    EfCoreConfigurationEvents.UnsupportedFormField,
                    $"{typeof(TEntity).Name}.{field.FieldName} cannot be used as an EF resource form field: it is a key, a concurrency token or a generated value, or has no public setter.",
                    "The field is left out.",
                    field
                );
            }
        }
    }
}
