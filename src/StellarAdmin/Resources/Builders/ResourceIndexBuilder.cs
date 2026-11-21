using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's index page and adds its data layouts.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     The first data layout is the one the page shows by default, and the switcher lists the data
///     layouts in the order they are added.
/// </remarks>
public sealed class ResourceIndexBuilder<TResource>
    : ResourceIndexBuilderBase<TResource, ResourceIndexBuilder<TResource>>
{
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);

    internal ResourceIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds a data layout that shows the resources as cards.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the cards.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceCardsDataLayoutBuilder<TResource> AddCards(string key)
    {
        AddLayout(key, index => index.AddCards(key));

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that shows the resources as cards.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the cards.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddCards(
        string key,
        Action<ResourceCardsDataLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddCards(key));

        return this;
    }

    /// <summary>
    ///     Adds a data layout that shows items a handler loads as cards.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the cards show.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the cards.</returns>
    /// <remarks>
    ///     The handler receives the scope, search, sort and page, and applies them itself.
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceHandlerCardsDataLayoutBuilder<TResource, TItem> AddCards<TItem, THandler>(
        string key
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        var source = ConfigurationSource.Capture();
        var loader = HandlerDataLayoutLoader.CreateTable<TItem, THandler>(Services);
        AddLayout(
            key,
            index =>
            {
                index.AddCards(key);
                var cards = index.GetCards(key);
                cards.Loader = loader;
                cards.Source = source;
            }
        );

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that shows items a handler loads as cards.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the cards show.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the cards.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler receives the scope, search, sort and page, and applies them itself.
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddCards<TItem, THandler>(
        string key,
        Action<ResourceHandlerCardsDataLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddCards<TItem, THandler>(key));

        return this;
    }

    /// <summary>
    ///     Adds a data layout that renders the resources with a partial view.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the resources.
    /// </param>
    /// <returns>A builder that configures the data layout.</returns>
    /// <remarks>
    ///     The partial view receives every resource the scope and search select; a partial has no
    ///     paging.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    public ResourcePartialDataLayoutBuilder<TResource> AddPartial(string key, string partialName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        AddLayout(key, index => index.AddPartial(key, partialName));

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that renders the resources with a partial view.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the resources.
    /// </param>
    /// <param name="configure">A delegate that configures the data layout.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The partial view receives every resource the scope and search select; a partial has no
    ///     paging.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" />, <paramref name="partialName" /> or <paramref name="configure" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddPartial(
        string key,
        string partialName,
        Action<ResourcePartialDataLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddPartial(key, partialName));

        return this;
    }

    /// <summary>
    ///     Adds a data layout that renders items a handler loads with a partial view.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the data layout shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <returns>A builder that configures the data layout.</returns>
    /// <remarks>
    ///     The handler receives the scope and search and applies them itself. Registers
    ///     <typeparamref name="THandler" /> as a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceHandlerPartialDataLayoutBuilder<TResource, TItem> AddPartial<TItem, THandler>(
        string key,
        string partialName
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        var loader = HandlerDataLayoutLoader.Create<TItem, THandler>(Services);
        AddLayout(key, index => index.AddPartial(key, partialName, loader));

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that renders items a handler loads with a partial
    ///     view.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the data layout shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <param name="configure">A delegate that configures the data layout.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler receives the scope and search and applies them itself. Registers
    ///     <typeparamref name="THandler" /> as a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" />, <paramref name="partialName" /> or <paramref name="configure" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddPartial<TItem, THandler>(
        string key,
        string partialName,
        Action<ResourceHandlerPartialDataLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddPartial<TItem, THandler>(key, partialName));

        return this;
    }

    /// <summary>
    ///     Adds a data layout that shows the resources in a table.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the table.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceTableDataLayoutBuilder<TResource> AddTable(string key)
    {
        AddLayout(key, index => index.GetOrAddTable(key));

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that shows the resources in a table.
    /// </summary>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the table.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddTable(
        string key,
        Action<ResourceTableDataLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable(key));

        return this;
    }

    /// <summary>
    ///     Adds a data layout that shows items a handler loads in a table.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows the table shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the rows.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the table.</returns>
    /// <remarks>
    ///     The handler receives the scope, search, sort and page, and applies them itself.
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceHandlerTableDataLayoutBuilder<TResource, TItem> AddTable<TItem, THandler>(
        string key
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        var loader = HandlerDataLayoutLoader.CreateTable<TItem, THandler>(Services);
        AddLayout(key, index => index.GetOrAddTable(key).Loader = loader);

        return new(Services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that shows items a handler loads in a table.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows the table shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the rows.</typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the table.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler receives the scope, search, sort and page, and applies them itself.
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    public ResourceIndexBuilder<TResource> AddTable<TItem, THandler>(
        string key,
        Action<ResourceHandlerTableDataLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceDataLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable<TItem, THandler>(key));

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

    // Adds the data layout now, so the switcher's order is the order of the calls
    private void AddLayout(string key, Action<ResourceIndexOptions> add)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!_keys.Add(key))
        {
            throw new ArgumentException(
                $"Another data layout already uses the key '{key}'.",
                nameof(key)
            );
        }

        Services.Configure<ResourceOptions<TResource>>(options => add(options.Index));
    }
}
