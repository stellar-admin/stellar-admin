using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Builders;

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
}
