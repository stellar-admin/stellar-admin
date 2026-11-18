using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Adds the layouts of the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     The first layout is the one the page shows by default, and the switcher lists the layouts in
///     the order they are added.
/// </remarks>
public sealed class EfCoreResourceLayoutsBuilder<TEntity>
    where TEntity : class
{
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceCollection _services;

    internal EfCoreResourceLayoutsBuilder(IServiceCollection services) => _services = services;

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
    public ResourcePartialLayoutBuilder<TEntity> AddPartial(string key, string partialName)
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
    public EfCoreResourceLayoutsBuilder<TEntity> AddPartial(
        string key,
        string partialName,
        Action<ResourcePartialLayoutBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddPartial(key, partialName));

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
    public EfCoreResourceTableLayoutBuilder<TEntity> AddTable(string key)
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
    public EfCoreResourceLayoutsBuilder<TEntity> AddTable(
        string key,
        Action<EfCoreResourceTableLayoutBuilder<TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AddTable(key));

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

        _services.Configure<ResourceOptions<TEntity>>(options => add(options.Index));
    }
}
