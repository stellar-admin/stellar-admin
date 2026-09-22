using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures resource index searching.
/// </summary>
public sealed class ResourceSearchBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The search box placeholder.
    /// </summary>
    public string? Placeholder
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Search!.Placeholder = value
            );
    }

    internal ResourceSearchBuilder(IServiceCollection services) => _services = services;
}
