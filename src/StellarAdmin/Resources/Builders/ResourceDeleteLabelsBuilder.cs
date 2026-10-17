using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of delete confirmations.
/// </summary>
public sealed class ResourceDeleteLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the delete confirmation's default cancel button
    ///     label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Cancel</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
    ///     Sets the callback that generates the delete confirmation's default confirm button
    ///     label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Delete</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
    ///     Sets the callback that generates the delete confirmation's default message.
    /// </summary>
    /// <value>
    ///     The default returns a question with the singular resource label, such as
    ///     <c>Are you sure you want to delete this product?</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<ResourceLabelContext, string> Message
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Delete.Message = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the delete confirmation's default title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Delete</c> followed by the singular resource label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
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
