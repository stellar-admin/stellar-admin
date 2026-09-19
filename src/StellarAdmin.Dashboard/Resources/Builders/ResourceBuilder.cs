using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.PluralLabel = value);
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SingularLabel = value
            );
    }

    internal ResourceBuilder(IServiceCollection services) => _services = services;
}
