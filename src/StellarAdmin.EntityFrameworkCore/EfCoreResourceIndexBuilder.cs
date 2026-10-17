using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TContext">The type of the database context that holds the entities.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceIndexBuilder<TContext, TEntity>
    : ResourceIndexBuilderBase<TEntity, EfCoreResourceIndexBuilder<TContext, TEntity>>
    where TContext : DbContext
    where TEntity : class
{
    internal EfCoreResourceIndexBuilder(IServiceCollection services)
        : base(services) { }

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
    ///     Two or more panels are shown as tabs.
    /// </remarks>
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails(
        Action<EfCoreRowDetailPanelsBuilder<TContext, TEntity>> panels
    )
    {
        ArgumentNullException.ThrowIfNull(panels);
        var builder = new EfCoreRowDetailPanelsBuilder<TContext, TEntity>(Services);
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
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select
    )
    {
        ArgumentNullException.ThrowIfNull(select);

        return RegisterRowDetails(
            partialName,
            new EfCoreRowDetailLoader<TContext, TEntity, TModel>(select)
        );
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
    ///     Two or more panels are shown as tabs.
    /// </remarks>
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnableRowDetails(
        Action<EfCoreRowDetailPanelsBuilder<TContext, TEntity>> panels,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(panels));

        return this;
    }

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
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnableScopes(
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
