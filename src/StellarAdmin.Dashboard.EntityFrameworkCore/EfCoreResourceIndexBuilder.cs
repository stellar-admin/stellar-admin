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
    ///     Lets rows expand to show a partial view, with a model projected from the row's entity when the row expands.
    /// </summary>
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select
    )
    {
        ArgumentNullException.ThrowIfNull(select);

        return RegisterRowDetails(
            partialName,
            async (services, id, cancellationToken) =>
            {
                var db = services.GetRequiredService<TContext>();
                if (!EfCoreResourceKey.TryCreatePredicate<TEntity>(db, id, out var predicate))
                {
                    return null;
                }

                // A list distinguishes a missing entity from a projection to a default value
                var models = await db.Set<TEntity>()
                    .AsNoTracking()
                    .Where(predicate)
                    .Select(select)
                    .Take(1)
                    .ToListAsync(cancellationToken);

                return models.Count == 0 ? null : models[0];
            }
        );
    }

    /// <summary>
    ///     Lets rows expand to show a partial view, with a model projected from the row's entity when the row expands,
    ///     and configures the rows.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(partialName, select));

        return this;
    }

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

        return CreateSearchBuilder();
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
