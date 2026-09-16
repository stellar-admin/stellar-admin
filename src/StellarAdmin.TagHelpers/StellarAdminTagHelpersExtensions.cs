using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.TagHelpers;

/// <summary>
/// Exposes extensions allowing to register the StellarAdmin tag helper services.
/// </summary>
public static class StellarAdminTagHelpersExtensions
{
    extension(StellarAdminBuilder stellarAdminBuilder)
    {
        public StellarAdminTagHelpersBuilder AddTagHelpers()
        {
            stellarAdminBuilder.Services.AddOptions<StellarAdminTagHelpersOptions>();

            var builder = new StellarAdminTagHelpersBuilder(stellarAdminBuilder.Services);

            return builder;
        }
    }
}
