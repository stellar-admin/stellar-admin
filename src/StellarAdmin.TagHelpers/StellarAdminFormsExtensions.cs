using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin;

/// <summary>
///     Extensions for configuring StellarAdmin forms.
/// </summary>
public static class StellarAdminFormsExtensions
{
    extension(StellarAdminBuilder builder)
    {
        /// <summary>
        ///     Configures the application-wide defaults for forms.
        /// </summary>
        public StellarAdminBuilder ConfigureForms(Action<StellarAdminFormsBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure<StellarAdminFormsOptions>(options =>
                configure(new StellarAdminFormsBuilder(options))
            );

            return builder;
        }
    }
}
