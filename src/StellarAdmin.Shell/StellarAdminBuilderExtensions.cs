using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Shell;

public static class StellarAdminBuilderExtensions
{
    extension(StellarAdminBuilder builder)
    {
        public StellarAdminBuilder AddShell()
        {
            builder
                .Services.AddMvc()
                .ConfigureApplicationPartManager(manager =>
                {
                    var assembly = typeof(StellarAdminBuilderExtensions).Assembly;
                    if (manager.ApplicationParts.Any(part => part.Name == assembly.GetName().Name))
                    {
                        return;
                    }

                    // Registers the controllers and compiled Razor views of the StellarAdmin.Shell assembly
                    // with MVC.
                    manager.ApplicationParts.Add(new AssemblyPart(assembly));
                    manager.ApplicationParts.Add(new CompiledRazorAssemblyPart(assembly));
                });

            builder.AddUI();
            GetOrCreateOptions(builder);

            return builder;
        }

        /// <summary>
        ///     Adds the StellarAdmin shell and configures it.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration delegate used to configure the StellarAdmin shell.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public StellarAdminBuilder AddShell(Action<StellarAdminShellBuilder> configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            builder.AddShell();
            configuration(new StellarAdminShellBuilder(GetOrCreateOptions(builder)));

            return builder;
        }
    }

    // The options are built up-front and registered as an instance singleton rather than through
    // a lazy services.Configure callback, so packages and the consumer app all configure the same
    // instance during registration and the layout can inject it directly.
    private static StellarAdminShellOptions GetOrCreateOptions(StellarAdminBuilder builder)
    {
        var options =
            builder
                .Services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(StellarAdminShellOptions)
                )
                ?.ImplementationInstance as StellarAdminShellOptions;
        if (options is null)
        {
            options = new StellarAdminShellOptions();
            builder.Services.AddSingleton(options);
        }

        return options;
    }
}
