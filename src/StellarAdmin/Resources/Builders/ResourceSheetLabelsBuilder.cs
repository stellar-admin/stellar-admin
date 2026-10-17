using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of the sheet that pages load content into.
/// </summary>
public sealed class ResourceSheetLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the description shown when the sheet's content can't be loaded.
    /// </summary>
    /// <value>
    ///     The text. The default is "The content could not be loaded.".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
    ///     Sets the title shown when the sheet's content can't be loaded.
    /// </summary>
    /// <value>
    ///     The text. The default is "Something went wrong".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public string ErrorTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Sheet.ErrorTitle = value);
        }
    }

    /// <summary>
    ///     Sets the label of the button that loads the sheet's content again.
    /// </summary>
    /// <value>
    ///     The text. The default is "Try again".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public string RetryLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Sheet.RetryLabel = value);
        }
    }

    /// <summary>
    ///     Sets the description shown when a form in the sheet can't be saved.
    /// </summary>
    /// <value>
    ///     The text. The default is "Your changes could not be saved.".
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public string SaveErrorDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Sheet.SaveErrorDescription = value
            );
        }
    }

    internal ResourceSheetLabelsBuilder(IServiceCollection services) => _services = services;
}
