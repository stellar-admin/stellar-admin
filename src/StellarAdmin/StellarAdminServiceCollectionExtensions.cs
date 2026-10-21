using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Areas.StellarAdmin.Controllers;
using StellarAdmin.Home;
using StellarAdmin.Infrastructure.Icons;
using StellarAdmin.Infrastructure.Routing;
using StellarAdmin.Resources;
using StellarAdmin.Sidebar;
using StellarAdmin.UI;

namespace StellarAdmin;

/// <summary>
///     Provides extension methods that add the admin panel to the services.
/// </summary>
public static class StellarAdminServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the admin panel and the StellarAdmin UI components.
        /// </summary>
        /// <returns>A builder that configures the admin panel.</returns>
        /// <remarks>
        ///     This calls
        ///     <see cref="StellarAdmin.UI.StellarAdminUIExtensions.AddStellarAdminUI(IServiceCollection)" />,
        ///     which adds a model binder, a value provider and a toast filter to the MVC options
        ///     for the whole app.
        /// </remarks>
        public StellarAdminBuilder AddStellarAdmin()
        {
            services.AddMvc();
            services.AddHttpContextAccessor();
            services.Configure<RouteOptions>(options =>
                options.ConstraintMap[LowercaseParameterTransformer.Name] =
                    typeof(LowercaseParameterTransformer)
            );
            services.AddStellarAdminUI();
            services.TryAddSingleton<BrandingResolver>();
            services.TryAddSingleton<UserMenuResolver>();
            services.TryAddSingleton<LinkChecker>();
            services.TryAddSingleton<SetupStatusResolver>();
            services.TryAddSingleton<IconCatalog>();
            services.TryAddSingleton<IRichTextSanitizer, RichTextSanitizer>();
            services.TryAddSingleton(new EntityCandidateScanner(services));

            var admin = new StellarAdminBuilder(services, GetOrCreateOptions(services));
            admin.AddApplicationPart(typeof(StellarAdminServiceCollectionExtensions).Assembly);
            admin.AddController(typeof(HomeController), "Home");
            admin.AddController(typeof(LogoutController), "Logout");
            admin.AddScript("~/_content/StellarAdmin/htmx.min.js");
            admin.AddScript("~/_content/StellarAdmin/stellar-admin-panel.js");
            admin.AddStylesheet("~/_content/StellarAdmin/stellar-admin-panel.css");

            return admin;
        }

        /// <summary>
        ///     Adds and configures the admin panel and the StellarAdmin UI components.
        /// </summary>
        /// <param name="configure">A delegate that configures the admin panel.</param>
        /// <returns>The services, so that calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     This changes the app's MVC options in the same way as
        ///     <see cref="AddStellarAdmin(IServiceCollection)" />.
        /// </remarks>
        public IServiceCollection AddStellarAdmin(Action<StellarAdminBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            configure(services.AddStellarAdmin());

            return services;
        }
    }

    // The options are built up-front and registered as an instance singleton rather than
    // through a lazy services.Configure callback, so packages and the consumer app all
    // configure the same instance during registration and the layout can inject it directly.
    private static StellarAdminOptions GetOrCreateOptions(IServiceCollection services)
    {
        var options =
            services
                .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(StellarAdminOptions))
                ?.ImplementationInstance as StellarAdminOptions;
        if (options is null)
        {
            options = new StellarAdminOptions();
            services.AddSingleton(options);
        }

        return options;
    }
}
