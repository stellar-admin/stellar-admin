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

            return builder;
        }
    }
}
