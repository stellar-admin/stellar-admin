using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a table layout of a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceTableLayoutBuilder<TResource>
    : ResourceTableLayoutBuilderBase<TResource, ResourceTableLayoutBuilder<TResource>>
{
    internal ResourceTableLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }
}
