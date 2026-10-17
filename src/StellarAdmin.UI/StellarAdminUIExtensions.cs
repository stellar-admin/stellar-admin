using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Toasts;

namespace StellarAdmin.UI;

/// <summary>
///     Extensions for registering the StellarAdmin UI services.
/// </summary>
public static class StellarAdminUIExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the StellarAdmin UI services: the tag helpers, icons, toasts and form binding.
        /// </summary>
        /// <returns>The <see cref="StellarAdminUIBuilder" /> instance.</returns>
        public StellarAdminUIBuilder AddStellarAdminUI()
        {
            services.AddOptions<IconOptions>();
            services.AddOptions<StellarAdminUIOptions>();
            services.AddHttpContextAccessor();
            services.TryAddScoped<IToastNotifier, TempDataToastNotifier>();
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
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

                if (!options.Filters.OfType<ToastResultFilter>().Any())
                {
                    options.Filters.Add(new ToastResultFilter());
                }
            });

            var builder = new StellarAdminUIBuilder(services);

            return builder;
        }

        /// <summary>
        ///     Adds the StellarAdmin UI services and configures them.
        /// </summary>
        /// <param name="configure">The configuration delegate used to configure the StellarAdmin UI services.</param>
        /// <returns>The <see cref="IServiceCollection" /> instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IServiceCollection AddStellarAdminUI(Action<StellarAdminUIBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            configure(services.AddStellarAdminUI());

            return services;
        }
    }
}
