using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the settings that every layout of a resource's index page shares.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceSharedIndexBuilder<TResource>
    : ResourceIndexBuilderBase<TResource, ResourceSharedIndexBuilder<TResource>>
{
    internal ResourceSharedIndexBuilder(IServiceCollection services)
        : base(services) { }

    /// <inheritdoc cref="ResourceIndexBuilder{TResource}.EnableScopes()" />
    public ResourceScopesBuilder<TResource> EnableScopes() => new(Services);

    /// <inheritdoc cref="ResourceIndexBuilder{TResource}.EnableScopes(Action{ResourceScopesBuilder{TResource}})" />
    public ResourceSharedIndexBuilder<TResource> EnableScopes(
        Action<ResourceScopesBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <inheritdoc cref="ResourceIndexBuilder{TResource}.EnableSearch()" />
    public ResourceSearchBuilder<TResource> EnableSearch() => CreateSearchBuilder(() => new());

    /// <inheritdoc cref="ResourceIndexBuilder{TResource}.EnableSearch(Action{ResourceSearchBuilder{TResource}})" />
    public ResourceSharedIndexBuilder<TResource> EnableSearch(
        Action<ResourceSearchBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch());

        return this;
    }
}
