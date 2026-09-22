using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures resource index scopes.
/// </summary>
public sealed class ResourceScopesBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The scope applied when no recognized scope is requested.
    /// </summary>
    public string? DefaultScope
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Scopes!.DefaultScope = value
            );
    }

    internal ResourceScopesBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a named resource filter.
    /// </summary>
    public ResourceScopesBuilder<TResource> Add(string id, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Scopes!.Items.Add(new(id, title))
        );

        return this;
    }
}
