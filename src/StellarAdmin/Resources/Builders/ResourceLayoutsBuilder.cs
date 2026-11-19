using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Adds the layouts of a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     The first layout is the one the page shows by default, and the switcher lists the layouts in
///     the order they are added.
/// </remarks>
public sealed class ResourceLayoutsBuilder<TResource>
{
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceCollection _services;

    internal ResourceLayoutsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a layout that shows the resources as cards.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <returns>A builder that configures the cards.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceCardsLayoutBuilder<TResource> AddCards(string key)
    {
        AddLayout(key, index => index.AddCards(key));

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that shows the resources as cards.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the cards.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddCards(
        string key,
        Action<ResourceCardsLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddCards(key));

        return this;
    }

    /// <summary>
    ///     Adds a layout that shows items a handler loads as cards.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the cards show.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
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
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceHandlerCardsLayoutBuilder<TResource, TItem> AddCards<TItem, THandler>(string key)
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        var source = ConfigurationSource.Capture();
        var loader = HandlerLayoutLoader.CreateTable<TItem, THandler>(_services);
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

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that shows items a handler loads as cards.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the cards show.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
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
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddCards<TItem, THandler>(
        string key,
        Action<ResourceHandlerCardsLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddCards<TItem, THandler>(key));

        return this;
    }

    /// <summary>
    ///     Adds a layout that renders the resources with a partial view.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceLayoutModel{TItem}" /> of the resources.
    /// </param>
    /// <returns>A builder that configures the layout.</returns>
    /// <remarks>
    ///     The partial view receives every resource the scope and search select; a partial has no
    ///     paging.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another layout already uses <paramref name="key" />.
    /// </exception>
    public ResourcePartialLayoutBuilder<TResource> AddPartial(string key, string partialName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        AddLayout(key, index => index.AddPartial(key, partialName));

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that renders the resources with a partial view.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceLayoutModel{TItem}" /> of the resources.
    /// </param>
    /// <param name="configure">A delegate that configures the layout.</param>
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
    ///     another layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddPartial(
        string key,
        string partialName,
        Action<ResourcePartialLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddPartial(key, partialName));

        return this;
    }

    /// <summary>
    ///     Adds a layout that renders items a handler loads with a partial view.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the layout shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <returns>A builder that configures the layout.</returns>
    /// <remarks>
    ///     The handler receives the scope and search and applies them itself. Registers
    ///     <typeparamref name="THandler" /> as a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceHandlerPartialLayoutBuilder<TResource, TItem> AddPartial<TItem, THandler>(
        string key,
        string partialName
    )
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        var loader = HandlerLayoutLoader.Create<TItem, THandler>(_services);
        AddLayout(key, index => index.AddPartial(key, partialName, loader));

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that renders items a handler loads with a partial view.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the layout shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the items.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <param name="configure">A delegate that configures the layout.</param>
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
    ///     another layout already uses <paramref name="key" />.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddPartial<TItem, THandler>(
        string key,
        string partialName,
        Action<ResourceHandlerPartialLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddPartial<TItem, THandler>(key, partialName));

        return this;
    }

    /// <summary>
    ///     Adds a layout that shows the resources in a table.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <returns>A builder that configures the table.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceTableLayoutBuilder<TResource> AddTable(string key)
    {
        AddLayout(key, index => index.GetOrAddTable(key));

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that shows the resources in a table.
    /// </summary>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the table.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddTable(
        string key,
        Action<ResourceTableLayoutBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable(key));

        return this;
    }

    /// <summary>
    ///     Adds a layout that shows items a handler loads in a table.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows the table shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the rows.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
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
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> AddTable<TItem, THandler>(string key)
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        var loader = HandlerLayoutLoader.CreateTable<TItem, THandler>(_services);
        AddLayout(key, index => index.GetOrAddTable(key).Loader = loader);

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a layout that shows items a handler loads in a table.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows the table shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the rows.</typeparam>
    /// <param name="key">The key that identifies the layout in the page's URL.</param>
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
    ///     <paramref name="key" /> is empty or whitespace, or another layout already uses it.
    /// </exception>
    public ResourceLayoutsBuilder<TResource> AddTable<TItem, THandler>(
        string key,
        Action<ResourceHandlerTableLayoutBuilder<TResource, TItem>> configure
    )
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable<TItem, THandler>(key));

        return this;
    }

    // Adds the layout now, so the switcher's order is the order of the calls
    private void AddLayout(string key, Action<ResourceIndexOptions> add)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!_keys.Add(key))
        {
            throw new ArgumentException(
                $"Another layout already uses the key '{key}'.",
                nameof(key)
            );
        }

        _services.Configure<ResourceOptions<TResource>>(options => add(options.Index));
    }
}
