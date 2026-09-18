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
            stellarAdminBuilder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                if (!options.ModelBinderProviders.OfType<CheckboxGroupModelBinderProvider>().Any())
                {
                    var index = options
                        .ModelBinderProviders.ToList()
                        .FindIndex(provider =>
                            provider
                            is Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ArrayModelBinderProvider
                        );
                    options.ModelBinderProviders.Insert(
                        index < 0 ? options.ModelBinderProviders.Count : index,
                        new CheckboxGroupModelBinderProvider()
                    );
                    options.ValueProviderFactories.Add(new CheckboxGroupValueProviderFactory());
                }
            });

            var builder = new StellarAdminTagHelpersBuilder(stellarAdminBuilder.Services);

            return builder;
        }
    }
}
