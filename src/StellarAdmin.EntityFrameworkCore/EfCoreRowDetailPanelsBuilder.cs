using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the panels of an EF Core resource's row details, shown as tabs.
/// </summary>
public sealed class EfCoreRowDetailPanelsBuilder<TContext, TEntity>
    : ResourceRowDetailPanelsBuilder<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    internal EfCoreRowDetailPanelsBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds a panel that renders a partial view with a model projected from the row's entity when the panel is
    ///     first shown.
    /// </summary>
    public ResourceRowDetailPanelBuilder Add<TModel>(
        string title,
        string partialName,
        Expression<Func<TEntity, TModel>> select
    )
    {
        ArgumentNullException.ThrowIfNull(select);

        return AddPanel(
            title,
            partialName,
            EfCoreRowDetailLoader.Create<TContext, TEntity, TModel>(select)
        );
    }
}
