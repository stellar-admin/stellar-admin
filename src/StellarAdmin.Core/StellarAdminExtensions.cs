using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin;

/// <summary>
///     Extensions for registering and configuring the StellarAdmin services.
/// </summary>
public static class StellarAdminExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Provides the common entry point for registering the StellarAdmin services.
        /// </summary>
        /// <returns>The <see cref="StellarAdminBuilder" /> instance.</returns>
        public StellarAdminBuilder AddStellarAdmin()
        {
            var builder = new StellarAdminBuilder(services);

            return builder;
        }

        /// <summary>
        ///     Provides the common entry point for registering the StellarAdmin services.
        /// </summary>
        /// <param name="configuration">The configuration delegate used to configure the StellarAdmin services.</param>
        /// <returns>The <see cref="IServiceCollection" /> instance.</returns>
        public IServiceCollection AddStellarAdmin(Action<StellarAdminBuilder> configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(services.AddStellarAdmin());

            return services;
        }
    }
}
