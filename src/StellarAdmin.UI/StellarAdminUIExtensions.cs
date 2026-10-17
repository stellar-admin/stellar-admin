using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Toasts;

namespace StellarAdmin.UI;

/// <summary>
///     Provides extension methods that register the StellarAdmin UI services.
/// </summary>
public static class StellarAdminUIExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the StellarAdmin UI services: the tag helpers, icons, toasts and form binding.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This also changes the MVC options for the whole app, not just pages that use
        ///         StellarAdmin:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             A model binder, placed before MVC's collection binders, binds a checkbox group or
        ///             a multiple-select toggle group with nothing selected to an empty collection.
        ///             Fields without the group's hidden marker use MVC's own binder.
        ///         </item>
        ///         <item>
        ///             A value provider reads the form on every form post to find those markers.
        ///         </item>
        ///         <item>
        ///             A global result filter sends queued toasts in the <c>SA-Toasts</c> response
        ///             header for fetch and XHR requests. Redirects and page navigations keep them in
        ///             TempData.
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <returns>A builder that configures the StellarAdmin UI services.</returns>
        public StellarAdminUIBuilder AddStellarAdminUI()
        {
            // Freezing after every Configure call, and resolving at startup, makes a bad icon
            // registration fail when the app starts rather than on the first render
            services
                .AddOptions<IconOptions>()
                .PostConfigure(options => options.Freeze())
                .ValidateOnStart();
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
        /// <remarks>
        ///     This changes the app's MVC options in the same way as <see cref="AddStellarAdminUI(IServiceCollection)" />.
        /// </remarks>
        /// <param name="configure">A delegate that configures the StellarAdmin UI services.</param>
        /// <returns>The service collection, so that calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        public IServiceCollection AddStellarAdminUI(Action<StellarAdminUIBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            configure(services.AddStellarAdminUI());

            return services;
        }
    }
}
