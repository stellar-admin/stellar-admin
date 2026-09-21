using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures index paging.
/// </summary>
public sealed class ResourcePagingBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The default number of resources per page.
    /// </summary>
    public int PageSize
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Paging!.PageSize = value
            );
    }

    /// <summary>
    ///     The page sizes available for selection.
    /// </summary>
    public int[] PageSizes
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var sizes = value.ToArray();
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Paging!.PageSizes = sizes.ToArray()
            );
        }
    }

    internal ResourcePagingBuilder(IServiceCollection services) => _services = services;
}
