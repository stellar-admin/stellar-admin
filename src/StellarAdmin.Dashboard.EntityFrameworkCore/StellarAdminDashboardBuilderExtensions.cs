using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public static EfCoreResourceBuilder<TContext, TEntity> AddEfCoreResource<TContext, TEntity>(
        this StellarAdminDashboardBuilder builder
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        var resource = builder.AddResource<TEntity>();
        resource.UseDataSource<EfCoreResourceDataSource<TContext, TEntity>>();
        builder
            .Services.AddOptions<ResourceOptions<TEntity>>()
            .Configure<IServiceScopeFactory>(
                (options, scopeFactory) =>
                {
                    options.Index = new EfCoreResourceIndexOptions<TEntity>();

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

                    options.KeyPropertyName = property.Name;
                    options.KeySelector = item =>
                        Convert.ToString(property.GetValue(item), CultureInfo.InvariantCulture)!;
                }
            );

        return new(builder.Services);
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
}
