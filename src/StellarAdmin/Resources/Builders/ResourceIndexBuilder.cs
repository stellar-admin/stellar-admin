using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceIndexBuilder<TResource>
    : ResourceIndexBuilderBase<TResource, ResourceIndexBuilder<TResource>>
{
    // The index's one table, which the table members configure
    private readonly ResourceTableLayoutBuilder<TResource> _table;

    internal ResourceIndexBuilder(IServiceCollection services)
        : base(services) => _table = new(services, ResourceIndexOptions.ImplicitTableKey);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.Columns" />
    public ResourceIndexBuilder<TResource> Columns(
        Action<ResourceColumnsBuilder<TResource>> configure
    )
    {
        _table.Columns(configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.DefaultSortBy" />
    public ResourceIndexBuilder<TResource> DefaultSortBy<TProperty>(
        Expression<Func<TResource, TProperty>> field
    )
    {
        _table.DefaultSortBy(field);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.DefaultSortByDescending" />
    public ResourceIndexBuilder<TResource> DefaultSortByDescending<TProperty>(
        Expression<Func<TResource, TProperty>> field
    )
    {
        _table.DefaultSortByDescending(field);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnablePaging()" />
    public ResourcePagingBuilder<TResource> EnablePaging() => _table.EnablePaging();

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnablePaging(Action{ResourcePagingBuilder{TResource}})" />
    public ResourceIndexBuilder<TResource> EnablePaging(
        Action<ResourcePagingBuilder<TResource>> configure
    )
    {
        _table.EnablePaging(configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(string)" />
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails(string partialName) =>
        _table.EnableRowDetails(partialName);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(Action{ResourceRowDetailPanelsBuilder{TResource}})" />
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TResource>> panels
    ) => _table.EnableRowDetails(panels);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(string, Action{ResourceRowDetailsBuilder{TResource}})" />
    public ResourceIndexBuilder<TResource> EnableRowDetails(
        string partialName,
        Action<ResourceRowDetailsBuilder<TResource>> configure
    )
    {
        _table.EnableRowDetails(partialName, configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails(Action{ResourceRowDetailPanelsBuilder{TResource}}, Action{ResourceRowDetailsBuilder{TResource}})" />
    public ResourceIndexBuilder<TResource> EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TResource>> panels,
        Action<ResourceRowDetailsBuilder<TResource>> configure
    )
    {
        _table.EnableRowDetails(panels, configure);

        return this;
    }

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails{TModel, THandler}(string)" />
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails<TModel, THandler>(
        string partialName
    )
        where THandler : class, IResourceRowDetailHandler<TModel> =>
        _table.EnableRowDetails<TModel, THandler>(partialName);

    /// <inheritdoc cref="ResourceTableLayoutBuilderBase{TResource, TBuilder}.EnableRowDetails{TModel, THandler}(string, Action{ResourceRowDetailsBuilder{TResource}})" />
    public ResourceIndexBuilder<TResource> EnableRowDetails<TModel, THandler>(
        string partialName,
        Action<ResourceRowDetailsBuilder<TResource>> configure
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
    public ResourceScopesBuilder<TResource> EnableScopes() => new(Services);

    /// <summary>
    ///     Enables and configures scopes, which filter the index with tabs.
    /// </summary>
    /// <param name="configure">A delegate that configures the scopes.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceIndexBuilder<TResource> EnableScopes(
        Action<ResourceScopesBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <summary>
    ///     Enables searching on the index.
    /// </summary>
    /// <returns>A builder that configures searching.</returns>
    public ResourceSearchBuilder<TResource> EnableSearch() => CreateSearchBuilder(() => new());

    /// <summary>
    ///     Enables and configures searching on the index.
    /// </summary>
    /// <param name="configure">A delegate that configures searching.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceIndexBuilder<TResource> EnableSearch(
        Action<ResourceSearchBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch());

        return this;
    }
}
