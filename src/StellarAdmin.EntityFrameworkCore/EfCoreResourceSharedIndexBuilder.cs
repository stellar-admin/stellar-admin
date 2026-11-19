using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the settings that every data layout of the index page of a resource that EF Core
///     loads shares.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceSharedIndexBuilder<TEntity>
    : ResourceIndexBuilderBase<TEntity, EfCoreResourceSharedIndexBuilder<TEntity>>
    where TEntity : class
{
    internal EfCoreResourceSharedIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Enables scopes, which filter the index with tabs.
    /// </summary>
    /// <returns>A builder that configures the scopes.</returns>
    public EfCoreResourceScopesBuilder<TEntity> EnableScopes() => new(Services);

    /// <summary>
    ///     Enables and configures scopes, which filter the index with tabs.
    /// </summary>
    /// <param name="configure">A delegate that configures the scopes.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public EfCoreResourceSharedIndexBuilder<TEntity> EnableScopes(
        Action<EfCoreResourceScopesBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <summary>
    ///     Enables searching on the index.
    /// </summary>
    /// <param name="predicate">
    ///     A delegate that returns the filter expression for a search term.
    /// </param>
    /// <returns>A builder that configures searching.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    public ResourceSearchBuilder<TEntity> EnableSearch(
        Func<string, Expression<Func<TEntity, bool>>> predicate
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return CreateSearchBuilder(() =>
            new EfCoreResourceSearchOptions<TEntity> { Predicate = predicate }
        );
    }

    /// <summary>
    ///     Enables and configures searching on the index.
    /// </summary>
    /// <param name="predicate">
    ///     A delegate that returns the filter expression for a search term.
    /// </param>
    /// <param name="configure">A delegate that configures searching.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="predicate" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    public EfCoreResourceSharedIndexBuilder<TEntity> EnableSearch(
        Func<string, Expression<Func<TEntity, bool>>> predicate,
        Action<ResourceSearchBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch(predicate));

        return this;
    }
}
