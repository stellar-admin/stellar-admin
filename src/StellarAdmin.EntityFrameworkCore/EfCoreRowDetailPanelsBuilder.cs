using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Adds the panels shown in a row's details for a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     Two or more panels are shown in an accordion unless <c>PanelLayout</c> sets another layout.
/// </remarks>
public sealed class EfCoreRowDetailPanelsBuilder<TEntity> : ResourceRowDetailPanelsBuilder<TEntity>
    where TEntity : class
{
    internal EfCoreRowDetailPanelsBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds a panel that shows a partial view with a model projected from the row's entity.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <param name="title">The title of the panel's tab.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="select">
    ///     An expression that projects the row's entity to the partial's model.
    /// </param>
    /// <returns>A builder that configures the panel.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" />, <paramref name="partialName" /> or <paramref name="select" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    /// <remarks>
    ///     The model loads when the panel is first shown.
    /// </remarks>
    public ResourceRowDetailPanelBuilder Add<TModel>(
        string title,
        string partialName,
        Expression<Func<TEntity, TModel>> select
    )
    {
        ArgumentNullException.ThrowIfNull(select);

        return AddPanel(title, partialName, new EfCoreRowDetailLoader<TEntity, TModel>(select));
    }

    /// <summary>
    ///     Adds a panel that shows a partial view with a model projected from the row's entity,
    ///     and configures the panel.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <param name="title">The title of the panel.</param>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="select">
    ///     An expression that projects the row's entity to the partial's model.
    /// </param>
    /// <param name="configure">A delegate that configures the panel.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" />, <paramref name="partialName" />, <paramref name="select" />
    ///     or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="title" /> or <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    /// <remarks>
    ///     The model loads when the panel is first shown.
    /// </remarks>
    public EfCoreRowDetailPanelsBuilder<TEntity> Add<TModel>(
        string title,
        string partialName,
        Expression<Func<TEntity, TModel>> select,
        Action<ResourceRowDetailPanelBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Add(title, partialName, select));

        return this;
    }
}
