using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the deletion of a resource.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public sealed class ResourceDeleteBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the cancel button label.
    /// </summary>
    /// <value>
    ///     The default is the label set with
    ///     <see cref="ResourceDeleteLabelsBuilder.CancelLabel" />.
    /// </value>
    public string? CancelLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.CancelLabel = value
            );
    }

    /// <summary>
    ///     Sets the confirm button label.
    /// </summary>
    /// <value>
    ///     The default is the label set with
    ///     <see cref="ResourceDeleteLabelsBuilder.ConfirmLabel" />.
    /// </value>
    public string? ConfirmLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.ConfirmLabel = value
            );
    }

    /// <summary>
    ///     Sets the confirmation message.
    /// </summary>
    /// <value>
    ///     The default is the message set with <see cref="ResourceDeleteLabelsBuilder.Message" />.
    /// </value>
    public string? Message
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.Message = value
            );
    }

    /// <summary>
    ///     Sets the confirmation title.
    /// </summary>
    /// <value>
    ///     The default is the title set with <see cref="ResourceDeleteLabelsBuilder.Title" />.
    /// </value>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.Title = value
            );
    }

    internal ResourceDeleteBuilder(IServiceCollection services) => _services = services;
}
