using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures paging on a cards data layout.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardsPagingBuilder<TResource>
{
    private readonly string _layoutKey;
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the number of cards per page.
    /// </summary>
    /// <value>
    ///     The number of cards per page. The default is 25.
    /// </value>
    public int PageSize
    {
        set
        {
            var source = ConfigurationSource.Capture();
            _services.Configure<ResourceOptions<TResource>>(options =>
            {
                var paging = options.Index.GetCards(_layoutKey).Paging!;
                paging.PageSize = value;
                paging.PageSizeSource = source;
            });
        }
    }

    internal ResourceCardsPagingBuilder(IServiceCollection services, string layoutKey)
    {
        _services = services;
        _layoutKey = layoutKey;
    }
}
