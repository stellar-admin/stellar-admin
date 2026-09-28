using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Registers EF Core resources.
/// </summary>
public static class StellarAdminDashboardBuilderExtensions
{
    /// <summary>
    ///     Adds an EF Core resource.
    /// </summary>
    /// <remarks>
    ///     The resource's URL slug defaults to the kebab-case plural of the entity type's name,
    ///     e.g. <c>order-items</c> for <c>OrderItem</c>.
    /// </remarks>
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return ConfigureEfCoreResource<TContext, TEntity>(builder, builder.AddResource<TEntity>());
    }

    /// <summary>
    ///     Adds an EF Core resource under the specified URL slug.
    /// </summary>
    /// <param name="builder">The Dashboard builder.</param>
    /// <param name="slug">
    ///     The resource's URL segment: lowercase letters and digits, optionally separated by
    ///     single hyphens.
    /// </param>
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder,
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
    ///     Adds and configures an EF Core resource.
    /// </summary>
    public static StellarAdminDashboardBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder,
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
    ///     Adds and configures an EF Core resource under the specified URL slug.
    /// </summary>
    /// <param name="builder">The Dashboard builder.</param>
    /// <param name="slug">
    ///     The resource's URL segment: lowercase letters and digits, optionally separated by
    ///     single hyphens.
    /// </param>
    /// <param name="configure">Configures the resource.</param>
    public static StellarAdminDashboardBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder,
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
    >(this StellarAdminDashboardBuilder builder, ResourceBuilder<TEntity> resource)
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
