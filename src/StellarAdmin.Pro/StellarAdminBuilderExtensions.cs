using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro;

public static class StellarAdminBuilderExtensions
{
    extension(StellarAdminBuilder builder)
    {
        /// <summary>
        ///     Adds StellarAdmin Pro: the admin shell, the resource infrastructure, and the
        ///     pro tag helpers.
        /// </summary>
        public StellarAdminProBuilder AddPro()
        {
            builder.Services.AddMvc();
            builder.AddTagHelpers();

            var pro = new StellarAdminProBuilder(builder.Services, GetOrCreateOptions(builder));
            pro.AddApplicationPart(typeof(StellarAdminBuilderExtensions).Assembly);
            pro.AddScript("~/_content/StellarAdmin.Pro/htmx.min.js");
            pro.AddStylesheet("~/_content/StellarAdmin.Pro/stellar-admin-pro.css");

            return pro;
        }

        /// <summary>
        ///     Adds StellarAdmin Pro and configures it.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration delegate used to configure StellarAdmin Pro.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public StellarAdminBuilder AddPro(Action<StellarAdminProBuilder> configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(builder.AddPro());

            return builder;
        }
    }

    // The options are built up-front and registered as an instance singleton rather than
    // through a lazy services.Configure callback, so packages and the consumer app all
    // configure the same instance during registration and the layout can inject it directly.
    private static StellarAdminProOptions GetOrCreateOptions(StellarAdminBuilder builder)
    {
        var options =
            builder
                .Services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(StellarAdminProOptions)
                )
                ?.ImplementationInstance as StellarAdminProOptions;
        if (options is null)
        {
            options = new StellarAdminProOptions();
            builder.Services.AddSingleton(options);
        }

        return options;
    }
}
