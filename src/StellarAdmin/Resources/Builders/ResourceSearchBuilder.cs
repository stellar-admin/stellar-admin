using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures searching on a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceSearchBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the placeholder of the search box.
    /// </summary>
    /// <value>
    ///     The placeholder, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.SearchPlaceholder" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Placeholder
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Search!.Placeholder = value
            );
    }

    internal ResourceSearchBuilder(IServiceCollection services) => _services = services;
}
