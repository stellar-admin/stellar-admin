using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a table data layout of a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceTableDataLayoutBuilder<TResource>
    : ResourceTableDataLayoutBuilderBase<TResource, ResourceTableDataLayoutBuilder<TResource>>
{
    internal ResourceTableDataLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }
}
