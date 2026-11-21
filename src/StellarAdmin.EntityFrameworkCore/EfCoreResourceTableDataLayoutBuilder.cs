using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures a table data layout of the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceTableDataLayoutBuilder<TEntity>
    : ResourceTableDataLayoutBuilderBase<TEntity, EfCoreResourceTableDataLayoutBuilder<TEntity>>
    where TEntity : class
{
    internal EfCoreResourceTableDataLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }

    /// <summary>
    ///     Shows one or more panels when a row is expanded.
    /// </summary>
    /// <param name="panels">A delegate that adds the panels.</param>
    /// <returns>A builder that configures the rows.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="panels" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     No panels are added, a panel has no name, or two panels have the same name.
    /// </exception>
    /// <remarks>
    ///     Two or more panels are shown in an accordion unless <c>PanelLayout</c> sets another layout.
    /// </remarks>
    public ResourceTableRowDetailsBuilder<TEntity> EnableRowDetails(
        Action<EfCoreResourceTableRowDetailPanelsBuilder<TEntity>> panels
    )
    {
        ArgumentNullException.ThrowIfNull(panels);
        var builder = new EfCoreResourceTableRowDetailPanelsBuilder<TEntity>(Services);
        panels(builder);

        return RegisterRowDetails(builder);
    }

    /// <summary>
    ///     Shows a partial view with a model projected from the row's entity when a row is
    ///     expanded.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="select">
    ///     An expression that projects the row's entity to the partial's model.
    /// </param>
    /// <returns>A builder that configures the rows.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> or <paramref name="select" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public ResourceTableRowDetailsBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select
    )
    {
        ArgumentNullException.ThrowIfNull(select);

        return RegisterRowDetails(partialName, EfCoreRowDetailLoader.Create(select));
    }

    /// <summary>
    ///     Shows a partial view with a model projected from the row's entity when a row is
    ///     expanded, and configures the rows.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="select">
    ///     An expression that projects the row's entity to the partial's model.
    /// </param>
    /// <param name="configure">A delegate that configures the rows.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" />, <paramref name="select" /> or
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public EfCoreResourceTableDataLayoutBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select,
        Action<ResourceTableRowDetailsBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(partialName, select));

        return this;
    }

    /// <summary>
    ///     Shows one or more panels when a row is expanded, and configures the rows.
    /// </summary>
    /// <param name="panels">A delegate that adds the panels.</param>
    /// <param name="configure">A delegate that configures the rows.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="panels" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     No panels are added, a panel has no name, or two panels have the same name.
    /// </exception>
    /// <remarks>
    ///     Two or more panels are shown in an accordion unless <c>PanelLayout</c> sets another layout.
    /// </remarks>
    public EfCoreResourceTableDataLayoutBuilder<TEntity> EnableRowDetails(
        Action<EfCoreResourceTableRowDetailPanelsBuilder<TEntity>> panels,
        Action<ResourceTableRowDetailsBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(panels));

        return this;
    }
}
