using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceIndexBuilder<TResource>
    : ResourceIndexBuilderBase<TResource, ResourceIndexBuilder<TResource>>
{
    internal ResourceIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Enables scopes, which filter the index with tabs.
    /// </summary>
    /// <returns>A builder that configures the scopes.</returns>
    public ResourceScopesBuilder<TResource> EnableScopes()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Scopes = new());

        return new(Services);
    }

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
    public ResourceSearchBuilder<TResource> EnableSearch()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Search = new());

        return CreateSearchBuilder();
    }

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
}
