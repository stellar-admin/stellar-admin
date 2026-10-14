using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Infrastructure.Mvc;
using StellarAdmin.Sidebar;
using StellarAdmin.UI;

namespace StellarAdmin;

/// <summary>
///     Provides a fluent API to configure the StellarAdmin admin panel.
/// </summary>
public class StellarAdminBuilder
{
    private readonly StellarAdminOptions _options;

    /// <summary>
    ///     Gets the services collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StellarAdminBuilder" />.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="options">The options the builder configures.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder(IServiceCollection services, StellarAdminOptions options)
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
    public StellarAdminBuilder AddApplicationPart(Assembly assembly)
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

                // The StellarAdmin admin panel assemblies opt out of MVC's automatic application part
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
    public StellarAdminBuilder AddController(Type controllerType, string controllerName)
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
    public StellarAdminBuilder AddScript(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_options.Scripts.Contains(path))
        {
            _options.Scripts.Add(path);
        }

        return this;
    }

    /// <summary>
    ///     Adds a link to the sidebar.
    /// </summary>
    /// <param name="label">The link label.</param>
    /// <param name="target">The link destination.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminBuilder AddSidebarLink(string label, SidebarLinkTarget target)
    {
        return AddSidebarLink(label, target, _ => { });
    }

    /// <summary>
    ///     Adds and configures a link to the sidebar.
    /// </summary>
    /// <param name="label">The link label.</param>
    /// <param name="target">The link destination.</param>
    /// <param name="configure">Configures the link.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public StellarAdminBuilder AddSidebarLink(
        string label,
        SidebarLinkTarget target,
        Action<SidebarLinkBuilder> configure
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(configure);

        var link = new SidebarLinkOptions(label, target);
        configure(new SidebarLinkBuilder(link));
        _options.SidebarLinks.Add(link);

        Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ISidebarItemsProvider, SidebarLinkItemsProvider>()
        );

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
    public StellarAdminBuilder AddStylesheet(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!_options.Stylesheets.Contains(path))
        {
            _options.Stylesheets.Add(path);
        }

        return this;
    }

    /// <summary>
    ///     Configures the admin panel's name and the sidebar header: its title, subtitle and logo.
    /// </summary>
    /// <param name="configure">A callback for configuring the <see cref="BrandingBuilder" />.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">The logo is a wordmark and a subtitle is set.</exception>
    public StellarAdminBuilder ConfigureBranding(Action<BrandingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new BrandingBuilder(_options.Branding));

        // A wordmark replaces the title and subtitle, so it has nowhere to show a subtitle
        if (_options.Branding is { Logo: WordmarkBrandingLogo, Subtitle: not null })
        {
            throw new InvalidOperationException(
                "A wordmark logo replaces the title and cannot be combined with a subtitle."
            );
        }

        return this;
    }

    /// <summary>
    ///     Configures the StellarAdmin UI components the admin panel renders with: icons,
    ///     menu defaults and form defaults.
    /// </summary>
    /// <param name="configure">A callback for configuring the <see cref="StellarAdminUIBuilder" />.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder ConfigureUI(Action<StellarAdminUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new StellarAdminUIBuilder(Services));

        return this;
    }

    /// <summary>
    ///     Configures the sidebar's user menu: the claims it shows, its items and the logout endpoint.
    /// </summary>
    /// <remarks>
    ///     The user menu shows only for an authenticated user, so it requires ASP.NET Core authentication.
    /// </remarks>
    /// <param name="configure">A callback for configuring the <see cref="UserMenuBuilder" />.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder ConfigureUserMenu(Action<UserMenuBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new UserMenuBuilder(_options.UserMenu));

        return this;
    }

    /// <summary>
    ///     Requires an authenticated user for every admin panel page.
    /// </summary>
    public StellarAdminBuilder RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy an authorization policy.
    /// </summary>
    /// <param name="policy">The authorization policy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder RequireAuthorization(AuthorizationPolicy policy)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policy));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy an authorization policy built by a delegate.
    /// </summary>
    /// <param name="configurePolicy">The delegate that builds the authorization policy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder RequireAuthorization(
        Action<AuthorizationPolicyBuilder> configurePolicy
    )
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(configurePolicy));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy the given authorization data.
    /// </summary>
    /// <param name="authorizeData">
    ///     The authorization data. When empty, an authenticated user is required.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(authorizeData));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy the named authorization policies.
    /// </summary>
    /// <param name="policyNames">
    ///     The authorization policy names. When empty, an authenticated user is required.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public StellarAdminBuilder RequireAuthorization(params string[] policyNames)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policyNames));

        return this;
    }
}
