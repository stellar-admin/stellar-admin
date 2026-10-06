using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default delete confirmation text.
/// </summary>
public sealed class ResourceDeleteLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default delete cancellation label.
    /// </summary>
    public Func<ResourceLabelContext, string> CancelLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Delete.CancelLabel = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation button label.
    /// </summary>
    public Func<ResourceLabelContext, string> ConfirmLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Delete.ConfirmLabel = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation message.
    /// </summary>
    public Func<ResourceLabelContext, string> Message
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Delete.Message = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Delete.Title = value);
        }
    }

    internal ResourceDeleteLabelsBuilder(IServiceCollection services) => _services = services;
}
