using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public sealed class ResourceIndexBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.Index.Title = value);
    }

    internal ResourceIndexBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the index columns.
    /// </summary>
    public ResourceIndexBuilder<TResource> Columns(
        Action<ResourceColumnsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }
}
