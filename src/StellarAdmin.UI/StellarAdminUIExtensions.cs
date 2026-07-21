using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.UI.Icons;
using TailwindMerge;

namespace StellarAdmin.UI;

/// <summary>
/// Exposes extensions allowing to register the StellarAdmin UI services.
/// </summary>
public static class StellarAdminUIExtensions
{
    extension(StellarAdminBuilder stellarAdminBuilder)
    {
        public StellarAdminUIBuilder AddUI()
        {
            stellarAdminBuilder.Services.AddOptions<StellarAdminUIOptions>();
            stellarAdminBuilder
                .Services.AddSingleton<TwMerge>()
                .AddSingleton<ICssClassMerger, DefaultCssClassMerger>()
                .AddSingleton<IIconManager>(_ => DefaultIconManager.Instance);

            var builder = new StellarAdminUIBuilder(stellarAdminBuilder.Services);
            builder.AddIconPack<LucideIconPack>();
            return builder;
        }
    }
}
