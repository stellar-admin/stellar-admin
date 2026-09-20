using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures resource deletion.
/// </summary>
public sealed class ResourceDeleteBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The cancel button label.
    /// </summary>
    public string? CancelLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.CancelLabel = value
            );
    }

    /// <summary>
    ///     The delete confirmation button label.
    /// </summary>
    public string? ConfirmLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.ConfirmLabel = value
            );
    }

    /// <summary>
    ///     The delete confirmation message.
    /// </summary>
    public string? Message
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.Message = value
            );
    }

    /// <summary>
    ///     The delete confirmation title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Delete!.Title = value
            );
    }

    internal ResourceDeleteBuilder(IServiceCollection services) => _services = services;
}
