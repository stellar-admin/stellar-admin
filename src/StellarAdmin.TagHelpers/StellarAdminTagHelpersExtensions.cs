using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.TagHelpers.Icons;

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
            stellarAdminBuilder.Services.AddSingleton<IIconManager>(_ =>
                DefaultIconManager.Instance
            );

            var builder = new StellarAdminTagHelpersBuilder(stellarAdminBuilder.Services);
            builder.AddIconPack<LucideIconPack>();
            return builder;
        }
    }
}
