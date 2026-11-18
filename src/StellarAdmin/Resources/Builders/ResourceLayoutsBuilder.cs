using Microsoft.Extensions.DependencyInjection;
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
    public ResourceLayoutsBuilder<TResource> AddTable(
        string key,
        Action<ResourceTableLayoutBuilder<TResource>> configure
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

        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.GetOrAddTable(key)
        );
    }
}
