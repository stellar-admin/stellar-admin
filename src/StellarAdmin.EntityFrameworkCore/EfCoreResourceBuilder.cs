using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures a resource that EF Core loads and saves.
/// </summary>
/// <typeparam name="TContext">The type of the database context that holds the entities.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceBuilder<TContext, TEntity>
    : ResourceBuilderBase<TEntity, EfCoreResourceBuilder<TContext, TEntity>>
    where TContext : DbContext
    where TEntity : class
{
    internal EfCoreResourceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    /// <param name="configure">A delegate that configures the index page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public EfCoreResourceBuilder<TContext, TEntity> Index(
        Action<EfCoreResourceIndexBuilder<TContext, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services));

        return this;
    }

    /// <summary>
    ///     Configures how the resource's records look, and which may be picked, when another record
    ///     refers to them, such as in a lookup.
    /// </summary>
    /// <param name="configure">A delegate that configures the records' look.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Each record is named by its title, which <c>UseTitle</c> sets.
    /// </remarks>
    public EfCoreResourceBuilder<TContext, TEntity> Reference(
        Action<EfCoreResourceReferenceBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services));

        return this;
    }
}
