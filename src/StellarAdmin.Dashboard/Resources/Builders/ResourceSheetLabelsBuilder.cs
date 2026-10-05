using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default text for the sheet that pages load content into.
/// </summary>
public sealed class ResourceSheetLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The description shown when the sheet's content could not be loaded.
    /// </summary>
    public string ErrorDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Sheet.ErrorDescription = value
            );
        }
    }

    /// <summary>
    ///     The title shown when the sheet's content could not be loaded.
    /// </summary>
    public string ErrorTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Sheet.ErrorTitle = value);
        }
    }

    /// <summary>
    ///     The label of the button that loads the sheet's content again.
    /// </summary>
    public string RetryLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Sheet.RetryLabel = value);
        }
    }

    internal ResourceSheetLabelsBuilder(IServiceCollection services) => _services = services;
}
