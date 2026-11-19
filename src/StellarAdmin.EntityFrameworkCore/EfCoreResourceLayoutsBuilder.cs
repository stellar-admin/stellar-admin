using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Adds the data layouts of the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     The first data layout is the one the page shows by default, and the switcher lists the data
///     layouts in the order they are added.
/// </remarks>
public sealed class EfCoreResourceLayoutsBuilder<TEntity>
    where TEntity : class
{
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceCollection _services;

    internal EfCoreResourceLayoutsBuilder(IServiceCollection services) => _services = services;

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
    public ResourceCardsDataLayoutBuilder<TEntity> AddCards(string key)
    {
        AddLayout(key, index => index.AddCards(key));

        return new(_services, key);
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
    public EfCoreResourceLayoutsBuilder<TEntity> AddCards(
        string key,
        Action<ResourceCardsDataLayoutBuilder<TEntity>> configure
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
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the items: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the cards.</returns>
    /// <remarks>
    ///     The handler sorts and pages the items. Registers <typeparamref name="THandler" /> as a
    ///     scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface.
    /// </exception>
    public ResourceHandlerCardsDataLayoutBuilder<TEntity, TItem> AddCards<TItem, THandler>(
        string key
    )
        where THandler : class
    {
        var source = ConfigurationSource.Capture();
        var loader = (ResourceTableLoader)CreateLoader<TItem, THandler>(table: true);
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
    ///     Adds and configures a data layout that shows items a handler loads as cards.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the cards show.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the items: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the cards.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler sorts and pages the items. Registers <typeparamref name="THandler" /> as a
    ///     scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface.
    /// </exception>
    public EfCoreResourceLayoutsBuilder<TEntity> AddCards<TItem, THandler>(
        string key,
        Action<ResourceHandlerCardsDataLayoutBuilder<TEntity, TItem>> configure
    )
        where THandler : class
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
    public ResourcePartialDataLayoutBuilder<TEntity> AddPartial(string key, string partialName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        AddLayout(key, index => index.AddPartial(key, partialName));

        return new(_services, key);
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
    public EfCoreResourceLayoutsBuilder<TEntity> AddPartial(
        string key,
        string partialName,
        Action<ResourcePartialDataLayoutBuilder<TEntity>> configure
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
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the items: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <returns>A builder that configures the data layout.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface for
    ///     <typeparamref name="TItem" />.
    /// </exception>
    public ResourceHandlerPartialDataLayoutBuilder<TEntity, TItem> AddPartial<TItem, THandler>(
        string key,
        string partialName
    )
        where THandler : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partialName);
        var loader = (ResourceDataLayoutLoader)CreateLoader<TItem, THandler>(table: false);
        AddLayout(key, index => index.AddPartial(key, partialName, loader));

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that renders items a handler loads with a partial
    ///     view.
    /// </summary>
    /// <typeparam name="TItem">The type of the items the data layout shows.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the items: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="partialName">
    ///     The name of the partial view, which receives a
    ///     <see cref="ResourceDataLayoutModel{TItem}" /> of the items.
    /// </param>
    /// <param name="configure">A delegate that configures the data layout.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" />, <paramref name="partialName" /> or <paramref name="configure" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> or <paramref name="partialName" /> is empty or whitespace, or
    ///     another data layout already uses <paramref name="key" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface for
    ///     <typeparamref name="TItem" />.
    /// </exception>
    public EfCoreResourceLayoutsBuilder<TEntity> AddPartial<TItem, THandler>(
        string key,
        string partialName,
        Action<ResourceHandlerPartialDataLayoutBuilder<TEntity, TItem>> configure
    )
        where THandler : class
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
    public EfCoreResourceTableDataLayoutBuilder<TEntity> AddTable(string key)
    {
        AddLayout(key, index => index.GetOrAddTable(key));

        return new(_services, key);
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
    public EfCoreResourceLayoutsBuilder<TEntity> AddTable(
        string key,
        Action<EfCoreResourceTableDataLayoutBuilder<TEntity>> configure
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
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the rows: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <returns>A builder that configures the table.</returns>
    /// <remarks>
    ///     The handler sorts and pages the rows. Registers <typeparamref name="THandler" /> as a
    ///     scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface.
    /// </exception>
    public ResourceHandlerTableDataLayoutBuilder<TEntity, TItem> AddTable<TItem, THandler>(
        string key
    )
        where THandler : class
    {
        var loader = (ResourceTableLoader)CreateLoader<TItem, THandler>(table: true);
        AddLayout(key, index => index.GetOrAddTable(key).Loader = loader);

        return new(_services, key);
    }

    /// <summary>
    ///     Adds and configures a data layout that shows items a handler loads in a table.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows the table shows.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the rows: an <see
    ///     cref="IEfCoreDataLayoutHandler{TEntity, TItem}" />, which reshapes the entities the
    ///     scope and search select, or an <see cref="IResourceDataLayoutHandler{TItem}" />, which
    ///     loads its own data.
    /// </typeparam>
    /// <param name="key">The key that identifies the data layout in the page's URL.</param>
    /// <param name="configure">A delegate that configures the table.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler sorts and pages the rows. Registers <typeparamref name="THandler" /> as a
    ///     scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> is empty or whitespace, or another data layout already uses it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <typeparamref name="THandler" /> implements neither handler interface.
    /// </exception>
    public EfCoreResourceLayoutsBuilder<TEntity> AddTable<TItem, THandler>(
        string key,
        Action<ResourceHandlerTableDataLayoutBuilder<TEntity, TItem>> configure
    )
        where THandler : class
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable<TItem, THandler>(key));

        return this;
    }

    // An EF handler receives the entities the scope and search select; another handler loads its own data. A table gets
    // a ResourceTableLoader, and a partial a ResourceDataLayoutLoader
    private Delegate CreateLoader<TItem, THandler>(bool table)
        where THandler : class
    {
        string factory;
        if (typeof(IEfCoreDataLayoutHandler<TEntity, TItem>).IsAssignableFrom(typeof(THandler)))
        {
            factory = table ? nameof(CreateEfCoreTableLoader) : nameof(CreateEfCoreLoader);
        }
        else if (typeof(IResourceDataLayoutHandler<TItem>).IsAssignableFrom(typeof(THandler)))
        {
            factory = table ? nameof(CreateHandlerTableLoader) : nameof(CreateHandlerLoader);
        }
        else
        {
            throw new InvalidOperationException(
                $"The data layout handler {typeof(THandler).Name} must implement "
                    + $"IEfCoreDataLayoutHandler<{typeof(TEntity).Name}, {typeof(TItem).Name}> or "
                    + $"IResourceDataLayoutHandler<{typeof(TItem).Name}>."
            );
        }

        return (Delegate)
            typeof(EfCoreResourceLayoutsBuilder<TEntity>)
                .GetMethod(factory, BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(typeof(TItem), typeof(THandler))
                .Invoke(this, null)!;
    }

    private ResourceDataLayoutLoader CreateEfCoreLoader<TItem, THandler>()
        where THandler : class, IEfCoreDataLayoutHandler<TEntity, TItem> =>
        EfCoreDataLayoutLoader.Create<TEntity, TItem, THandler>(_services);

    private ResourceDataLayoutLoader CreateHandlerLoader<TItem, THandler>()
        where THandler : class, IResourceDataLayoutHandler<TItem> =>
        HandlerDataLayoutLoader.Create<TItem, THandler>(_services);

    private ResourceTableLoader CreateEfCoreTableLoader<TItem, THandler>()
        where THandler : class, IEfCoreDataLayoutHandler<TEntity, TItem> =>
        EfCoreDataLayoutLoader.CreateTable<TEntity, TItem, THandler>(_services);

    private ResourceTableLoader CreateHandlerTableLoader<TItem, THandler>()
        where THandler : class, IResourceDataLayoutHandler<TItem> =>
        HandlerDataLayoutLoader.CreateTable<TItem, THandler>(_services);

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

        _services.Configure<ResourceOptions<TEntity>>(options => add(options.Index));
    }
}
