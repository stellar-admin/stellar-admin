using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Home;

// Works out which setup steps the app has done, for the home page's setup checklist
internal sealed class SetupStatusResolver(
    StellarAdminOptions options,
    IHostEnvironment environment,
    IServiceProvider services
)
{
    private const string DefaultLogoutIcon = "log-out";
    private const string DefaultSidebarGroup = "Resources";
    private const string PanelAssetPrefix = "~/_content/StellarAdmin/";

    public bool IsEnabled => options.HomePage.ShowSetupChecklist && environment.IsDevelopment();

    public async Task<SetupStatus?> ResolveAsync(HttpContext httpContext)
    {
        // Leave before resolving or scanning anything, so production and a turned-off checklist
        // do no work
        if (!IsEnabled)
        {
            return null;
        }

        var resources = services.GetServices<ResourceRegistration>().ToArray();
        var hasAuthentication = await HasAuthenticationAsync();

        var candidates = services.GetRequiredService<EntityCandidateScanner>().Candidates;
        var resourceTypes = resources.Select(resource => resource.ResourceType).ToHashSet();
        var entities = candidates with
        {
            Contexts = candidates
                .Contexts.Select(context =>
                    context with
                    {
                        EntityTypes = context
                            .EntityTypes.Where(type => !resourceTypes.Contains(type))
                            .ToArray(),
                    }
                )
                .Where(context => context.EntityTypes.Count > 0)
                .ToArray(),
        };

        return new(
            hasAuthentication,
            resources.Length,
            IsProtected(httpContext),
            options.Branding.Title is not null || options.Branding.Logo is not null,
            HasUserMenu(options.UserMenu),
            ThemeStylesheet(),
            HasSidebarLayout(resources),
            entities
        );
    }

    private static bool HasUserMenu(UserMenuOptions menu)
    {
        var logout = menu.Logout;

        return menu.Items.Count > 0
            || menu.NameClaimType is not null
            || menu.DescriptionClaimType is not null
            || menu.AvatarClaimType is not null
            || logout.Path is not null
            || logout.RedirectPath is not null
            || logout.AuthenticationSchemes.Count > 0
            || !logout.Show
            || logout.Icon != DefaultLogoutIcon;
    }

    private async Task<bool> HasAuthenticationAsync()
    {
        // The scheme provider is only registered when the app adds authentication
        var schemes = services.GetService<IAuthenticationSchemeProvider>();

        return schemes is not null && (await schemes.GetAllSchemesAsync()).Any();
    }

    private bool HasSidebarLayout(IReadOnlyList<ResourceRegistration> resources)
    {
        return options.SidebarLinks.Count > 0
            || resources
                .Select(resource => resource.ResolveSidebarItem(services))
                .Any(item =>
                    item.Group != DefaultSidebarGroup || item.Order != 0 || item.Icon is not null
                );
    }

    private bool IsProtected(HttpContext httpContext)
    {
        // The home page's endpoint carries the panel's authorization metadata, whether it came
        // from admin.RequireAuthorization or MapStellarAdmin().RequireAuthorization
        var metadata = httpContext.GetEndpoint()?.Metadata;
        if (metadata?.GetMetadata<IAllowAnonymous>() is not null)
        {
            return false;
        }

        if (
            metadata?.GetMetadata<IAuthorizeData>() is not null
            || metadata?.GetMetadata<AuthorizationPolicy>() is not null
        )
        {
            return true;
        }

        return services.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy
                is not null
            || services
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value.Filters.OfType<AuthorizeFilter>()
                .Any();
    }

    // The app's own stylesheet, rather than a web font from another site, names the theme
    private string? ThemeStylesheet()
    {
        var stylesheets = options
            .Stylesheets.Where(path =>
                !path.StartsWith(PanelAssetPrefix, StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();

        return stylesheets.FirstOrDefault(path =>
                !Uri.IsWellFormedUriString(path, UriKind.Absolute)
            ) ?? stylesheets.FirstOrDefault();
    }
}
