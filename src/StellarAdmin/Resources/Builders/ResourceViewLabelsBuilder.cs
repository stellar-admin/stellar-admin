using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of every resource's view page.
/// </summary>
/// <remarks>
///     Text set on a resource's own view builder overrides these defaults.
/// </remarks>
public sealed class ResourceViewLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the delegate that returns the default title of the view page.
    /// </summary>
    /// <value>
    ///     A delegate that returns the text for a resource. The default returns "View" followed by
    ///     the resource's singular label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.View.Title = value);
        }
    }

    internal ResourceViewLabelsBuilder(IServiceCollection services) => _services = services;
}
