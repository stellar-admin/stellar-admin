using Microsoft.Extensions.DependencyInjection;
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
        AddLayout(key);

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
    private void AddLayout(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!_keys.Add(key))
        {
            throw new ArgumentException(
                $"Another layout already uses the key '{key}'.",
                nameof(key)
            );
        }

        _services.Configure<ResourceOptions<TEntity>>(options => options.Index.GetOrAddTable(key));
    }
}
