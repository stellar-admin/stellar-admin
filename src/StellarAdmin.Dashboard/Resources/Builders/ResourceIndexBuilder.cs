using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public sealed class ResourceIndexBuilder<TResource>
    : ResourceIndexBuilderBase<TResource, ResourceIndexBuilder<TResource>>
{
    internal ResourceIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Enables index scopes.
    /// </summary>
    public ResourceScopesBuilder<TResource> EnableScopes()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Scopes = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures index scopes.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnableScopes(
        Action<ResourceScopesBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <summary>
    ///     Enables index searching.
    /// </summary>
    public ResourceSearchBuilder<TResource> EnableSearch()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Search = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures index searching.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnableSearch(
        Action<ResourceSearchBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch());

        return this;
    }
}
