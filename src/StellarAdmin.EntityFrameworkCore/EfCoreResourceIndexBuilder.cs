using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceIndexBuilder<TEntity>
    : ResourceIndexBuilderBase<TEntity, EfCoreResourceIndexBuilder<TEntity>>
    where TEntity : class
{
    // The index's one table, which the table members configure
    private readonly EfCoreResourceTableLayoutBuilder<TEntity> _table;

    internal EfCoreResourceIndexBuilder(IServiceCollection services)
        : base(services) => _table = new(services, ResourceIndexOptions.ImplicitTableKey);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.Columns" />
    public EfCoreResourceIndexBuilder<TEntity> Columns(
        Action<ResourceColumnsBuilder<TEntity>> configure
    )
    {
        _table.Columns(configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.DefaultSortBy" />
    public EfCoreResourceIndexBuilder<TEntity> DefaultSortBy<TProperty>(
        Expression<Func<TEntity, TProperty>> field
    )
    {
        _table.DefaultSortBy(field);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.DefaultSortByDescending" />
    public EfCoreResourceIndexBuilder<TEntity> DefaultSortByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> field
    )
    {
        _table.DefaultSortByDescending(field);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnablePaging()" />
    public ResourcePagingBuilder<TEntity> EnablePaging() => _table.EnablePaging();

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnablePaging(Action{ResourcePagingBuilder{TResource}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnablePaging(
        Action<ResourcePagingBuilder<TEntity>> configure
    )
    {
        _table.EnablePaging(configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(string)" />
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails(string partialName) =>
        _table.EnableRowDetails(partialName);

    /// <inheritdoc cref="EfCoreResourceTableLayoutBuilder{TEntity}.EnableRowDetails(Action{EfCoreRowDetailPanelsBuilder{TEntity}})" />
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails(
        Action<EfCoreRowDetailPanelsBuilder<TEntity>> panels
    ) => _table.EnableRowDetails(panels);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(Action{ResourceRowDetailPanelsBuilder{TResource}})" />
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TEntity>> panels
    ) => _table.EnableRowDetails(panels);

    /// <inheritdoc cref="EfCoreResourceTableLayoutBuilder{TEntity}.EnableRowDetails{TModel}(string, Expression{Func{TEntity, TModel}})" />
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select
    ) => _table.EnableRowDetails(partialName, select);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(string, Action{ResourceRowDetailsBuilder{TResource}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnableRowDetails(
        string partialName,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        _table.EnableRowDetails(partialName, configure);

        return this;
    }

    /// <inheritdoc cref="EfCoreResourceTableLayoutBuilder{TEntity}.EnableRowDetails(Action{EfCoreRowDetailPanelsBuilder{TEntity}}, Action{ResourceRowDetailsBuilder{TEntity}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnableRowDetails(
        Action<EfCoreRowDetailPanelsBuilder<TEntity>> panels,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        _table.EnableRowDetails(panels, configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(Action{ResourceRowDetailPanelsBuilder{TResource}}, Action{ResourceRowDetailsBuilder{TResource}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TEntity>> panels,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        _table.EnableRowDetails(panels, configure);

        return this;
    }

    /// <inheritdoc cref="EfCoreResourceTableLayoutBuilder{TEntity}.EnableRowDetails{TModel}(string, Expression{Func{TEntity, TModel}}, Action{ResourceRowDetailsBuilder{TEntity}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnableRowDetails<TModel>(
        string partialName,
        Expression<Func<TEntity, TModel>> select,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
    {
        _table.EnableRowDetails(partialName, select, configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails{TModel, THandler}(string)" />
    public ResourceRowDetailsBuilder<TEntity> EnableRowDetails<TModel, THandler>(string partialName)
        where THandler : class, IResourceRowDetailHandler<TModel> =>
        _table.EnableRowDetails<TModel, THandler>(partialName);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails{TModel, THandler}(string, Action{ResourceRowDetailsBuilder{TResource}})" />
    public EfCoreResourceIndexBuilder<TEntity> EnableRowDetails<TModel, THandler>(
        string partialName,
        Action<ResourceRowDetailsBuilder<TEntity>> configure
    )
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        _table.EnableRowDetails<TModel, THandler>(partialName, configure);

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
    public EfCoreResourceIndexBuilder<TEntity> EnableScopes(
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
    public EfCoreResourceIndexBuilder<TEntity> EnableSearch(
        Func<string, Expression<Func<TEntity, bool>>> predicate,
        Action<ResourceSearchBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch(predicate));

        return this;
    }
}
