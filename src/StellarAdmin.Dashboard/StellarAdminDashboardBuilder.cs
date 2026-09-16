using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Infrastructure.Mvc;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Provides a fluent API to configure StellarAdmin Dashboard.
/// </summary>
public class StellarAdminDashboardBuilder
{
    private readonly StellarAdminDashboardOptions _options;

    /// <summary>
    ///     Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminDashboardBuilder" />.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="options">The options the builder configures.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminDashboardBuilder(
        IServiceCollection services,
        StellarAdminDashboardOptions options
    )
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    ///     Registers an assembly's controllers and compiled Razor views with MVC.
    /// </summary>
    /// <param name="assembly">
    ///     The assembly to register. Registering the same assembly more than once adds it
    ///     only once.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminDashboardBuilder AddApplicationPart(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Services
            .AddMvcCore()
            .ConfigureApplicationPartManager(manager =>
            {
                if (manager.ApplicationParts.Any(part => part.Name == assembly.GetName().Name))
                {
                    return;
                }

                // The StellarAdmin dashboard assemblies opt out of MVC's automatic application part
                // discovery (see their AssemblyInfo.cs), so their controllers and compiled
                // views are registered here.
                manager.ApplicationParts.Add(new AssemblyPart(assembly));
                manager.ApplicationParts.Add(new CompiledRazorAssemblyPart(assembly));
            });

        return this;
    }

    /// <summary>
    ///     Registers a controller with MVC under the given name, including its assembly's compiled Razor views.
    /// </summary>
    /// <param name="controllerType">
    ///     The controller type. Registering the same type more than once adds it only once.
    /// </param>
    /// <param name="controllerName">The name the controller routes and resolves views under.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminDashboardBuilder AddController(Type controllerType, string controllerName)
    {
        ArgumentNullException.ThrowIfNull(controllerType);
        ArgumentException.ThrowIfNullOrWhiteSpace(controllerName);

        AddApplicationPart(controllerType.Assembly);

        var mvc = Services.AddMvcCore();

        // ConfigureApplicationPartManager runs its delegate immediately, so the provider
        // exists below and the controller list is complete when MVC builds its features.
        StellarAdminControllerFeatureProvider provider = null!;
        mvc.ConfigureApplicationPartManager(manager =>
        {
            provider = manager
                .FeatureProviders.OfType<StellarAdminControllerFeatureProvider>()
                .FirstOrDefault()!;
            if (provider is not null)
            {
                return;
            }

            provider = new StellarAdminControllerFeatureProvider();
            manager.FeatureProviders.Add(provider);

            // The provider is also the convention that assigns the controller names, so it
            // is hooked into the MVC options once, when it is first created.
            mvc.AddMvcOptions(options => options.Conventions.Add(provider));
        });

        provider.AddController(controllerType, controllerName);

        return this;
    }

    /// <summary>
    ///     Links an additional script on every shell page. The script loads deferred, in
    ///     registration order.
    /// </summary>
    /// <param name="path">
    ///     The app-relative path of the script, e.g. <c>~/js/admin.js</c>. Registering the
    ///     same path more than once links it only once.
    /// </param>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminDashboardBuilder AddScript(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_options.Scripts.Contains(path))
        {
            _options.Scripts.Add(path);
        }

        return this;
    }

    /// <summary>
    ///     Links an additional stylesheet on every shell page.
    /// </summary>
    /// <param name="path">
    ///     The app-relative path of the stylesheet, e.g. <c>~/css/admin.css</c>. Registering the
    ///     same path more than once links it only once.
    /// </param>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminDashboardBuilder AddStylesheet(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_options.Stylesheets.Contains(path))
        {
            _options.Stylesheets.Add(path);
        }

        return this;
    }
}
