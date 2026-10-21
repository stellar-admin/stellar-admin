using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Home;
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

    internal StellarAdminOptions Options => _options;

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
    /// <remarks>
    ///     This also adds the compiled Razor views of the controller's assembly. Adding the same
    ///     controller more than once adds it only once. A blank name, a name another controller
    ///     has, or a second name for the same controller is a configuration problem, and the
    ///     controller keeps its first registration.
    /// </remarks>
    public StellarAdminBuilder AddController(Type controllerType, string controllerName)
    {
        ArgumentNullException.ThrowIfNull(controllerType);
        ArgumentNullException.ThrowIfNull(controllerName);

        AddApplicationPart(controllerType.Assembly);

        var provider = ControllerProvider();
        var existingName = provider.FindName(controllerType);
        var existingType = provider.FindType(controllerName);
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            _options.Problems.Add(
                new(
                    ConfigurationProblemKind.InvalidValue,
                    ConfigurationProblemEffect.Ignored,
                    $"AddController gives {controllerType.Name} a blank name.",
                    "The controller isn't added."
                )
            );
        }
        else if (existingName is not null && existingName != controllerName)
        {
            _options.Problems.Add(
                new(
                    ConfigurationProblemKind.SlugInUse,
                    ConfigurationProblemEffect.Ignored,
                    $"AddController adds {controllerType.Name} as '{controllerName}', but it is already added as '{existingName}'.",
                    $"The controller keeps the name '{existingName}'."
                )
            );
        }
        else if (existingType is not null && existingType != controllerType)
        {
            _options.Problems.Add(
                new(
                    ConfigurationProblemKind.SlugInUse,
                    ConfigurationProblemEffect.Ignored,
                    $"AddController adds {controllerType.Name} as '{controllerName}', but {existingType.Name} already has that name.",
                    $"{controllerType.Name} isn't added."
                )
            );
        }
        else
        {
            provider.AddController(controllerType, controllerName);
        }

        return this;
    }

    // The type registered under a name, or the name a type is registered under, when adding a controller with this
    // type and name would conflict with it
    internal (Type ControllerType, string ControllerName)? FindControllerConflict(
        Type controllerType,
        string controllerName
    )
    {
        var provider = ControllerProvider();
        if (provider.FindName(controllerType) is { } name && name != controllerName)
        {
            return (controllerType, name);
        }

        return provider.FindType(controllerName) is { } type && type != controllerType
            ? (type, controllerName)
            : null;
    }

    private StellarAdminControllerFeatureProvider ControllerProvider()
    {
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

        return provider;
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
    /// <remarks>
    ///     Scripts are deferred and load in the order they are added. Adding the same path more
    ///     than once adds it only once. An empty or white space path is a configuration problem,
    ///     and the call is ignored.
    /// </remarks>
    public StellarAdminBuilder AddScript(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (string.IsNullOrWhiteSpace(path))
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "AddScript is called with a blank path.",
                "No script is added."
            );

            return this;
        }

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
    /// <remarks>
    ///     An empty or white space label, or a target created with an empty or white space value,
    ///     is a configuration problem, and the link is left out.
    /// </remarks>
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
    /// <remarks>
    ///     An empty or white space label, or a target created with an empty or white space value,
    ///     is a configuration problem, and the link is left out. An empty or white space group or
    ///     icon is ignored.
    /// </remarks>
    public StellarAdminBuilder AddSidebarLink(
        string label,
        SidebarLinkTarget target,
        Action<SidebarLinkBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(configure);

        var link = new SidebarLinkOptions(label, target);
        configure(new SidebarLinkBuilder(link));
        if (
            (string.IsNullOrWhiteSpace(label) ? "Its label is blank." : target.Problem) is
            { } problem
        )
        {
            Report(
                ConfigurationProblemKind.InvalidLink,
                ConfigurationProblemEffect.TurnedOff,
                $"The sidebar link '{label}' can't be shown. {problem}",
                "The link is left out."
            );

            return this;
        }

        if (link.Group is { } linkGroup && string.IsNullOrWhiteSpace(linkGroup))
        {
            link.Group = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                $"The sidebar link '{label}' has a blank group.",
                "The link is in the default group."
            );
        }

        if (link.Icon is { } icon && string.IsNullOrWhiteSpace(icon))
        {
            link.Icon = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                $"The sidebar link '{label}' has a blank icon.",
                "The link has no icon."
            );
        }

        link.Position = _options.NextSidebarPosition();
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
    /// <remarks>
    ///     Adding the same path more than once adds it only once. An empty or white space path is a
    ///     configuration problem, and the call is ignored.
    /// </remarks>
    public StellarAdminBuilder AddStylesheet(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (string.IsNullOrWhiteSpace(path))
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "AddStylesheet is called with a blank path.",
                "No stylesheet is added."
            );

            return this;
        }

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
    /// <remarks>
    ///     A wordmark logo with a subtitle is a configuration problem, and the subtitle isn't
    ///     shown.
    /// </remarks>
    public StellarAdminBuilder ConfigureBranding(Action<BrandingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new BrandingBuilder(_options.Branding));
        RepairBranding();

        // A wordmark replaces the title and subtitle, so it has nowhere to show a subtitle
        if (_options.Branding is { Logo: WordmarkBrandingLogo, Subtitle: not null })
        {
            _options.Branding.Subtitle = null;
            _options.Problems.Add(
                new(
                    ConfigurationProblemKind.WordmarkWithSubtitle,
                    ConfigurationProblemEffect.Ignored,
                    "A wordmark logo replaces the title and can't be combined with a subtitle. Remove the subtitle, or use another logo.",
                    "The subtitle isn't shown."
                )
            );
        }

        return this;
    }

    /// <summary>
    ///     Configures how the admin panel reports problems in its configuration.
    /// </summary>
    /// <param name="configure">A delegate that configures the diagnostics.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder ConfigureDiagnostics(Action<DiagnosticsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new DiagnosticsBuilder(_options.Diagnostics));

        return this;
    }

    /// <summary>
    ///     Configures the admin panel's home page.
    /// </summary>
    /// <param name="configure">A delegate that configures the home page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder ConfigureHomePage(Action<HomePageBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new HomePageBuilder(_options.HomePage));

        return this;
    }

    /// <summary>
    ///     Configures how the sidebar sorts its groups and items.
    /// </summary>
    /// <param name="configure">A delegate that configures the sidebar.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public StellarAdminBuilder ConfigureSidebar(Action<SidebarBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new SidebarBuilder(_options.Sidebar));

        return this;
    }

    /// <summary>
    ///     Configures the sidebar group with a label.
    /// </summary>
    /// <param name="label">The label of the group.</param>
    /// <param name="configure">A delegate that configures the group.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Configuring a group doesn't add it. The group appears when a resource or link is in it.
    /// </remarks>
    public StellarAdminBuilder ConfigureSidebarGroup(
        string label,
        Action<SidebarGroupBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(configure);
        if (string.IsNullOrWhiteSpace(label))
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "ConfigureSidebarGroup is called with a blank label.",
                "Its settings apply to no group."
            );

            return this;
        }

        if (!_options.Sidebar.Groups.TryGetValue(label, out var group))
        {
            group = new SidebarGroupOptions();
            _options.Sidebar.Groups.Add(label, group);
        }

        configure(new SidebarGroupBuilder(group));

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
        RepairUserMenu();

        return this;
    }

    /// <summary>
    ///     Shows which environment the application runs in on every admin panel page.
    /// </summary>
    /// <param name="indicator">
    ///     The indicator to show, created with <see cref="EnvironmentIndicator.Stripe(string)" />,
    ///     <see cref="EnvironmentIndicator.Frame(string)" /> or
    ///     <see cref="EnvironmentIndicator.Banner(string)" />.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="indicator" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Call this method only in the environments that should show an indicator.
    /// </remarks>
    public StellarAdminBuilder EnableEnvironmentIndicator(EnvironmentIndicator indicator)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        if (indicator.Problem is { } problem)
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.TurnedOff,
                $"The environment indicator can't be shown. {problem}",
                "No environment indicator is shown."
            );

            return this;
        }

        if (indicator.Color.Problem is { } colorProblem)
        {
            indicator.Color = EnvironmentColor.Neutral;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                $"The environment indicator's colour can't be used. {colorProblem}",
                "The indicator uses the neutral colour."
            );
        }

        if (
            indicator is FrameEnvironmentIndicator { TabSize: var frameTab }
                && !Enum.IsDefined(frameTab)
            || indicator is StripeEnvironmentIndicator { TabSize: var stripeTab }
                && !Enum.IsDefined(stripeTab)
        )
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "The environment indicator's TabSize isn't an EnvironmentIndicatorTabSize value.",
                "The indicator uses the medium tab."
            );
        }

        _options.EnvironmentIndicator = indicator;

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

    private void RepairBranding()
    {
        var branding = _options.Branding;
        if (branding.Title is { } title && string.IsNullOrWhiteSpace(title))
        {
            branding.Title = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "The branding Title is blank.",
                "The sidebar shows the default title."
            );
        }

        if (branding.Href is { } href && string.IsNullOrWhiteSpace(href))
        {
            branding.Href = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "The branding Href is blank.",
                "The sidebar header links to the home page."
            );
        }

        if (branding.Logo?.Problem is { } problem)
        {
            branding.Logo = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                $"The branding logo can't be shown. {problem}",
                "The sidebar shows no logo."
            );
        }
    }

    private void RepairUserMenu()
    {
        var menu = _options.UserMenu;
        foreach (var item in menu.Items.Where(item => item.Problem is not null).ToArray())
        {
            menu.Items.Remove(item);
            Report(
                ConfigurationProblemKind.InvalidLink,
                ConfigurationProblemEffect.TurnedOff,
                $"A user menu item can't be shown. {item.Problem}",
                "The item is left out."
            );
        }

        menu.NameClaimType = RepairClaimType(menu.NameClaimType, "NameClaimType");
        menu.DescriptionClaimType = RepairClaimType(
            menu.DescriptionClaimType,
            "DescriptionClaimType"
        );
        menu.AvatarClaimType = RepairClaimType(menu.AvatarClaimType, "AvatarClaimType");

        var logout = menu.Logout;
        if (logout.Path is { } path && string.IsNullOrWhiteSpace(path))
        {
            logout.Path = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "The log out Path is blank.",
                "Log out uses the admin panel's own log out."
            );
        }

        if (logout.RedirectPath is { } redirect && string.IsNullOrWhiteSpace(redirect))
        {
            logout.RedirectPath = null;
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "The log out RedirectPath is blank.",
                "Log out redirects to the default path."
            );
        }

        if (logout.AuthenticationSchemes.RemoveAll(string.IsNullOrWhiteSpace) > 0)
        {
            Report(
                ConfigurationProblemKind.InvalidValue,
                ConfigurationProblemEffect.Ignored,
                "A log out authentication scheme is blank.",
                "Log out signs out of the other schemes."
            );
        }
    }

    private string? RepairClaimType(string? claimType, string setting)
    {
        if (claimType is null || !string.IsNullOrWhiteSpace(claimType))
        {
            return claimType;
        }

        Report(
            ConfigurationProblemKind.InvalidValue,
            ConfigurationProblemEffect.Ignored,
            $"The user menu's {setting} is blank.",
            "The user menu uses the default claim."
        );

        return null;
    }

    private void Report(
        ConfigurationProblemKind kind,
        ConfigurationProblemEffect effect,
        string message,
        string outcome
    ) => _options.Problems.Add(new(kind, effect, message, outcome));
}
