using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default view page text.
/// </summary>
public sealed class ResourceViewLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default view page title.
    /// </summary>
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
