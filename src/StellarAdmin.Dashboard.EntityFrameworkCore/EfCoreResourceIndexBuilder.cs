using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource's index page.
/// </summary>
public sealed class EfCoreResourceIndexBuilder<TContext, TEntity>
    : ResourceIndexBuilderBase<TEntity, EfCoreResourceIndexBuilder<TContext, TEntity>>
    where TContext : DbContext
    where TEntity : class
{
    internal EfCoreResourceIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Enables index scopes with EF Core predicates.
    /// </summary>
    public EfCoreResourceScopesBuilder<TEntity> EnableScopes()
    {
        Services.Configure<ResourceOptions<TEntity>>(options => options.Index.Scopes = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures index scopes with EF Core predicates.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnableScopes(
        Action<EfCoreResourceScopesBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <summary>
    ///     Enables index searching with an EF Core predicate.
    /// </summary>
    public ResourceSearchBuilder<TEntity> EnableSearch(
        Func<string, Expression<Func<TEntity, bool>>> predicate
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        Services.Configure<ResourceOptions<TEntity>>(options =>
            options.Index.Search = new EfCoreResourceSearchOptions<TEntity>
            {
                Predicate = predicate,
            }
        );

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures index searching with an EF Core predicate.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnableSearch(
        Func<string, Expression<Func<TEntity, bool>>> predicate,
        Action<ResourceSearchBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch(predicate));

        return this;
    }
}
