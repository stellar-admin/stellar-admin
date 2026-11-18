using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures an index layout that renders the resources with a partial view.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     The partial view receives a <see cref="ResourceLayoutModel{TItem}" /> with every resource
///     the scope and search select; a partial has no paging.
/// </remarks>
public sealed class ResourcePartialLayoutBuilder<TResource>
    : ResourceLayoutBuilderBase<TResource, ResourcePartialLayoutBuilder<TResource>>
{
    internal ResourcePartialLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }
}
