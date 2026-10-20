using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
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
    ///     entity type, which bind as the keys of the related entities. Otherwise, the app fails
    ///     to start.
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="slug" /> is empty, white space or not a valid slug.
    /// </exception>
    /// <remarks>
    ///     <typeparamref name="TEntity" /> must be mapped in <typeparamref name="TContext" />, with
    ///     a single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the app fails to start.
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
    ///     entity type, which bind as the keys of the related entities. Otherwise, the app fails
    ///     to start.
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
    /// <exception cref="ArgumentException">
    ///     <paramref name="slug" /> is empty, white space or not a valid slug.
    /// </exception>
    /// <remarks>
    ///     <typeparamref name="TEntity" /> must be mapped in <typeparamref name="TContext" />, with
    ///     a single public primary key property of type <see cref="int" />, <see cref="long" />,
    ///     <see cref="Guid" /> or <see cref="string" />. Fields of a form that binds the entity
    ///     must be settable properties that aren't keys, concurrency tokens or generated values,
    ///     or many-to-many navigations without a join entity type, which bind as the keys of the
    ///     related entities. Otherwise, the app fails to start.
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
                    var entity =
                        db.Model.FindEntityType(typeof(TEntity))
                        ?? throw new InvalidOperationException(
                            $"{typeof(TEntity).Name} is not mapped in {typeof(TContext).Name}."
                        );
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
                        throw new InvalidOperationException(
                            "EF resources require one public CLR primary-key property of type int, long, Guid, or string."
                        );
                    }

                    options.UseKey(property);
                    ValidateSortableColumns<TEntity>(entity, options.Index);

                    if (options.Create is { } create && create.ModelType == typeof(TEntity))
                    {
                        UseManyToManyBinders<TContext, TEntity>(entity, create);
                        ValidateEntityFormFields<TEntity>(entity, create);
                    }
                    if (options.Edit is { } edit && edit.ModelType == typeof(TEntity))
                    {
                        UseManyToManyBinders<TContext, TEntity>(entity, edit);
                        ValidateEntityFormFields<TEntity>(entity, edit);
                    }
                }
            );

        return new(builder.Services);
    }

    // A many-to-many navigation without an entity type for its join table binds as the keys of its related entities
    private static void UseManyToManyBinders<TContext, TEntity>(
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
                throw new InvalidOperationException(
                    $"{typeof(TEntity).Name}.{field.FieldName} cannot be used as an EF resource form field. A many-to-many field requires a CLR navigation property and related entities with one CLR primary-key property."
                );
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
    private static void ValidateSortableColumns<TEntity>(
        IEntityType entity,
        ResourceIndexOptions index
    )
    {
        foreach (
            var column in index.Columns.Where(column =>
                column is { Sortable: true, SortExpression: null }
            )
        )
        {
            if (
                EfCorePropertyPaths.GetPropertyPath(column.FieldExpression) is { } path
                && !EfCorePropertyPaths.CanInclude(entity, path)
            )
            {
                throw new InvalidOperationException(
                    $"The {typeof(TEntity).Name} column '{column.FieldName}' reads a collection, so it can't be sortable. Remove Sortable(), or sort it by an expression with Sortable(selector)."
                );
            }
        }
    }

    private static void ValidateEntityFormFields<TEntity>(
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
                throw new InvalidOperationException(
                    $"{typeof(TEntity).Name}.{field.FieldName} cannot be used as an EF resource form field."
                );
            }
        }
    }
}
