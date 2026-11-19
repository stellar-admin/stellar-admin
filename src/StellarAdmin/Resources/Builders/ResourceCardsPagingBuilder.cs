using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures paging on a cards layout.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceCardsPagingBuilder<TResource>
{
    private readonly string _layoutKey;
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the number of cards per page when the user hasn't chosen a page size.
    /// </summary>
    /// <value>
    ///     The number of cards per page. The default is 25.
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
                var paging = options.Index.GetCards(_layoutKey).Paging!;
                paging.PageSize = value;
                paging.PageSizeSource = source;
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
                var paging = options.Index.GetCards(_layoutKey).Paging!;
                paging.PageSizes = sizes.ToArray();
                paging.PageSizesSource = source;
            });
        }
    }

    internal ResourceCardsPagingBuilder(IServiceCollection services, string layoutKey)
    {
        _services = services;
        _layoutKey = layoutKey;
    }
}
