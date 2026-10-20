using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of create pages.
/// </summary>
public sealed class ResourceCreateLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the default label of the button that saves a new
    ///     resource and starts the next one.
    /// </summary>
    /// <value>
    ///     The default returns <c>Save and add another</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> AddAnotherLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Create.AddAnotherLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the create form's default submit button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Create</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
    ///     Sets the callback that generates the create page's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Create</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
