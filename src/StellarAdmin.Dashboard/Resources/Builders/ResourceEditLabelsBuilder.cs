using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default edit page text.
/// </summary>
public sealed class ResourceEditLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default edit form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> SubmitLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Edit.SubmitLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default edit page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Edit.Title = value);
        }
    }

    internal ResourceEditLabelsBuilder(IServiceCollection services) => _services = services;
}
