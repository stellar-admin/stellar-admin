using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures an index data layout that renders the resources with a partial view.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     The partial view receives a <see cref="ResourceDataLayoutModel{TItem}" /> with every
///     resource the scope and search select; a partial has no paging.
/// </remarks>
public sealed class ResourcePartialDataLayoutBuilder<TResource>
    : ResourceDataLayoutBuilderBase<TResource, ResourcePartialDataLayoutBuilder<TResource>>
{
    internal ResourcePartialDataLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }
}
