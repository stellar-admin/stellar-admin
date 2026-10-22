using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures how a resource's records look when another record refers to them, such as in a
///     lookup.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceReferenceBuilder<TResource>
    : ResourceReferenceBuilderBase<TResource, ResourceReferenceBuilder<TResource>>
{
    internal ResourceReferenceBuilder(IServiceCollection services)
        : base(services) { }
}
