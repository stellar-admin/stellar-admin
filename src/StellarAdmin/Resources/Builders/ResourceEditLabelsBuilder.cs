using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of edit pages.
/// </summary>
public sealed class ResourceEditLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the edit form's default submit button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Save</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> SubmitLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Edit.SubmitLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the edit page's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Edit</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
