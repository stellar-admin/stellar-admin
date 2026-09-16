using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard;

public static class StellarAdminBuilderExtensions
{
    extension(StellarAdminBuilder builder)
    {
        /// <summary>
        ///     Adds the StellarAdmin Dashboard admin application and resource management services.
        /// </summary>
        public StellarAdminDashboardBuilder AddDashboard()
        {
            builder.Services.AddMvc();
            builder.AddTagHelpers();

            var dashboard = new StellarAdminDashboardBuilder(
                builder.Services,
                GetOrCreateOptions(builder)
            );
            dashboard.AddApplicationPart(typeof(StellarAdminBuilderExtensions).Assembly);
            dashboard.AddScript("~/_content/StellarAdmin.Dashboard/htmx.min.js");
            dashboard.AddStylesheet(
                "~/_content/StellarAdmin.Dashboard/stellar-admin-dashboard.css"
            );

            return dashboard;
        }

        /// <summary>
        ///     Adds StellarAdmin Dashboard and configures it.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration delegate used to configure StellarAdmin Dashboard.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public StellarAdminBuilder AddDashboard(Action<StellarAdminDashboardBuilder> configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(builder.AddDashboard());

            return builder;
        }
    }

    // The options are built up-front and registered as an instance singleton rather than
    // through a lazy services.Configure callback, so packages and the consumer app all
    // configure the same instance during registration and the layout can inject it directly.
    private static StellarAdminDashboardOptions GetOrCreateOptions(StellarAdminBuilder builder)
    {
        var options =
            builder
                .Services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(StellarAdminDashboardOptions)
                )
                ?.ImplementationInstance as StellarAdminDashboardOptions;
        if (options is null)
        {
            options = new StellarAdminDashboardOptions();
            builder.Services.AddSingleton(options);
        }

        return options;
    }
}
