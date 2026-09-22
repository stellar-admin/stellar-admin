using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource.
/// </summary>
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
    public EfCoreResourceBuilder<TContext, TEntity> Index(
        Action<EfCoreResourceIndexBuilder<TContext, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services));

        return this;
    }
}
