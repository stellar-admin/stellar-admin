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
///     Configures the admin panel.
/// </summary>
public sealed class StellarAdminBuilder
{
    private readonly StellarAdminOptions _options;

    /// <summary>
    ///     Gets the services that the admin panel is added to.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    internal StellarAdminBuilder(IServiceCollection services, StellarAdminOptions options)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    ///     Adds an assembly's controllers and compiled Razor views to MVC.
    /// </summary>
    /// <param name="assembly">The assembly to add.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="assembly" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Adding the same assembly more than once adds it only once.
    /// </remarks>
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
    ///     Adds a controller to MVC under a name.
    /// </summary>
    /// <param name="controllerType">The type of the controller.</param>
    /// <param name="controllerName">
    ///     The name that the controller is routed and finds its views under.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="controllerType" /> or <paramref name="controllerName" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="controllerName" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     This also adds the compiled Razor views of the controller's assembly. Adding the same
    ///     controller more than once adds it only once.
    /// </remarks>
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
    ///     Adds a script to every admin panel page.
    /// </summary>
    /// <param name="path">
    ///     The app-relative path of the script, such as <c>~/js/admin.js</c>.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     Scripts are deferred and load in the order they are added. Adding the same path more
    ///     than once adds it only once.
    /// </remarks>
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
    /// <param name="label">The label of the link.</param>
    /// <param name="target">The destination of the link.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="target" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> is empty or white space.
    /// </exception>
    public StellarAdminBuilder AddSidebarLink(string label, SidebarLinkTarget target)
    {
        return AddSidebarLink(label, target, _ => { });
    }

    /// <summary>
    ///     Adds a link to the sidebar and configures it.
    /// </summary>
    /// <param name="label">The label of the link.</param>
    /// <param name="target">The destination of the link.</param>
    /// <param name="configure">A delegate that configures the link.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" />, <paramref name="target" /> or <paramref name="configure" />
    ///     is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="label" /> is empty or white space, or <paramref name="configure" /> sets
    ///     an empty or white-space group or icon.
    /// </exception>
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
    ///     Adds a stylesheet to every admin panel page.
    /// </summary>
    /// <param name="path">
    ///     The app-relative path of the stylesheet, such as <c>~/css/admin.css</c>.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="path" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="path" /> is empty or white space.
    /// </exception>
    /// <remarks>
    ///     Adding the same path more than once adds it only once.
    /// </remarks>
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
    ///     Configures the admin panel's name and the title, subtitle and logo in the sidebar
    ///     header.
    /// </summary>
    /// <param name="configure">A delegate that configures the branding.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The logo is a wordmark and a subtitle is set.
    /// </exception>
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
    ///     Configures the StellarAdmin UI components, such as their icons, menu defaults and form
    ///     defaults.
    /// </summary>
    /// <param name="configure">A delegate that configures the UI components.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder ConfigureUI(Action<StellarAdminUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new StellarAdminUIBuilder(Services));

        return this;
    }

    /// <summary>
    ///     Configures the sidebar's user menu, such as the claims it shows, its items and log out.
    /// </summary>
    /// <param name="configure">A delegate that configures the user menu.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The user menu is shown only to an authenticated user, so it requires ASP.NET Core
    ///     authentication.
    /// </remarks>
    public StellarAdminBuilder ConfigureUserMenu(Action<UserMenuBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new UserMenuBuilder(_options.UserMenu));

        return this;
    }

    /// <summary>
    ///     Requires an authenticated user for every admin panel page.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public StellarAdminBuilder RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy an authorization policy.
    /// </summary>
    /// <param name="policy">The authorization policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policy" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder RequireAuthorization(AuthorizationPolicy policy)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policy));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy an authorization policy that a delegate
    ///     builds.
    /// </summary>
    /// <param name="configurePolicy">A delegate that builds the authorization policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configurePolicy" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder RequireAuthorization(
        Action<AuthorizationPolicyBuilder> configurePolicy
    )
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(configurePolicy));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy authorization data.
    /// </summary>
    /// <param name="authorizeData">
    ///     The authorization data. When it's empty, the user must be authenticated.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="authorizeData" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(authorizeData));

        return this;
    }

    /// <summary>
    ///     Requires every admin panel page to satisfy the named authorization policies.
    /// </summary>
    /// <param name="policyNames">
    ///     The names of the authorization policies. When it's empty, the user must be
    ///     authenticated.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policyNames" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder RequireAuthorization(params string[] policyNames)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policyNames));

        return this;
    }
}
