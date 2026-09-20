using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's create page.
/// </summary>
public sealed class ResourceCreateBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The submit button label.
    /// </summary>
    public string? SubmitLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Create.SubmitLabel = value
            );
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Create.Title = value
            );
    }

    internal ResourceCreateBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the form fields.
    /// </summary>
    public ResourceCreateBuilder<TResource> Fields(
        Action<ResourceFieldsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }
}
