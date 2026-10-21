using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures paging on a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourcePagingBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the number of resources per page when the user hasn't chosen a page size.
    /// </summary>
    /// <value>
    ///     The number of resources per page. The default is 25.
    /// </value>
    /// <remarks>
    ///     The page size must be one of <see cref="PageSizes" />.
    /// </remarks>
    public int PageSize
    {
        set
        {
            var source = ConfigurationSource.Capture();
            _services.Configure<ResourceOptions<TResource>>(options =>
            {
                options.Index.Paging!.PageSize = value;
                options.Index.Paging.PageSizeSource = source;
            });
        }
    }

    /// <summary>
    ///     Sets the page sizes the user can choose from.
    /// </summary>
    /// <value>
    ///     The page sizes. The default is 10, 25, 50 and 100.
    /// </value>
    /// <remarks>
    ///     The page sizes must be positive and distinct, and include <see cref="PageSize" />.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public IReadOnlyList<int> PageSizes
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var sizes = value.ToArray();
            var source = ConfigurationSource.Capture();
            _services.Configure<ResourceOptions<TResource>>(options =>
            {
                options.Index.Paging!.PageSizes = sizes.ToArray();
                options.Index.Paging.PageSizesSource = source;
            });
        }
    }

    internal ResourcePagingBuilder(IServiceCollection services) => _services = services;
}
