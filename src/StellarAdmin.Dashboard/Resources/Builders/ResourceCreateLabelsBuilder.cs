using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default create page text.
/// </summary>
public sealed class ResourceCreateLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default create form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> SubmitLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Create.SubmitLabel = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default create page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Create.Title = value);
        }
    }

    internal ResourceCreateLabelsBuilder(IServiceCollection services) => _services = services;
}
