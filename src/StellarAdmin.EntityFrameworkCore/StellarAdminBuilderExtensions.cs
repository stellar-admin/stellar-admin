using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
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
    ///     concurrency tokens or generated values. Otherwise, the app fails to start.
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
    ///     must be settable properties that aren't keys, concurrency tokens or generated values.
    ///     Otherwise, the app fails to start.
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
    ///     concurrency tokens or generated values. Otherwise, the app fails to start.
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
    ///     must be settable properties that aren't keys, concurrency tokens or generated values.
    ///     Otherwise, the app fails to start.
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

                    if (options.Create is { } create && create.ModelType == typeof(TEntity))
                    {
                        ValidateEntityFormFields<TEntity>(entity, create);
                    }
                    if (options.Edit is { } edit && edit.ModelType == typeof(TEntity))
                    {
                        ValidateEntityFormFields<TEntity>(entity, edit);
                    }
                }
            );

        return new(builder.Services);
    }

    private static void ValidateEntityFormFields<TEntity>(
        IEntityType entity,
        ResourceFormOptions form
    )
    {
        foreach (var field in form.Fields)
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
