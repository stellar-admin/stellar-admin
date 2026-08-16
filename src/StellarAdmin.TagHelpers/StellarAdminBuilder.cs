using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin;

/// <summary>
///     Provides a shared entry point to configure the StellarAdmin services.
/// </summary>
public class StellarAdminBuilder
{
    /// <summary>
    ///     Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminBuilder" />.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
