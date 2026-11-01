using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures a resource that EF Core loads and saves.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceBuilder<TEntity>
    : ResourceBuilderBase<TEntity, EfCoreResourceBuilder<TEntity>>
    where TEntity : class
{
    internal EfCoreResourceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Enables the create page.
    /// </summary>
    /// <returns>A builder that configures the create page.</returns>
    public new EfCoreResourceCreateBuilder<TEntity, TEntity> AllowCreate() =>
        new(
            Services,
            AllowCreate<TEntity, EfCoreCreateHandler<TEntity, TEntity>>(),
            EfCoreFormEntity.Self(typeof(TEntity))
        );

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    /// <param name="configure">A delegate that configures the create page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public EfCoreResourceBuilder<TEntity> AllowCreate(
        Action<EfCoreResourceCreateBuilder<TEntity, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate());

        return this;
    }

    /// <summary>
    ///     Enables the edit page.
    /// </summary>
    /// <returns>A builder that configures the edit page.</returns>
    public new EfCoreResourceEditBuilder<TEntity, TEntity> AllowEdit() =>
        new(
            Services,
            AllowEdit<TEntity, EfCoreEditHandler<TEntity, TEntity>>(),
            EfCoreFormEntity.Self(typeof(TEntity))
        );

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    /// <param name="configure">A delegate that configures the edit page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public EfCoreResourceBuilder<TEntity> AllowEdit(
        Action<EfCoreResourceEditBuilder<TEntity, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowEdit());

        return this;
    }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    /// <param name="configure">A delegate that configures the index page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public EfCoreResourceBuilder<TEntity> Index(
        Action<EfCoreResourceIndexBuilder<TEntity>> configure
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
    public EfCoreResourceBuilder<TEntity> Reference(
        Action<EfCoreResourceReferenceBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services));

        return this;
    }

    /// <summary>
    ///     Loads and saves the resource's entities with a database context.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     By default, the registered database context whose model maps the entity is used. Pick
    ///     one when several contexts map it.
    /// </remarks>
    public EfCoreResourceBuilder<TEntity> UseDbContext<TContext>()
        where TContext : DbContext
    {
        Services.Configure<EfCoreResourceOptions<TEntity>>(options =>
            options.ContextType = typeof(TContext)
        );

        return this;
    }
}
