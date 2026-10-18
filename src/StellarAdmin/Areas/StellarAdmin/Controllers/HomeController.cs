using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;
using StellarAdmin.Home;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Resources;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class HomeController(
    SetupStatusResolver setup,
    BrandingResolver branding,
    IEnumerable<ResourceRegistration> resources,
    StellarAdminOptions options,
    IServiceProvider services
) : Controller
{
    public async Task<IActionResult> Index()
    {
        // Null outside Development and when the checklist is turned off, without detecting anything
        if (await setup.ResolveAsync(HttpContext) is { } status)
        {
            var checklist = SetupSteps.Create(
                status,
                branding.Title,
                Url.Action("Index", "Home") ?? "/",
                status.ThemeStylesheet is { } path ? StylesheetName(path) : null
            );

            return View(new HomePageViewModel(branding.Title, checklist, []));
        }

        return View(new HomePageViewModel(branding.Title, null, await GetResourceLinksAsync()));
    }

    private static string StylesheetName(string path)
    {
        var withoutQuery = path.Split('?', '#')[0];

        return withoutQuery[(withoutQuery.LastIndexOf('/') + 1)..];
    }

    // The resources the sidebar shows the current user, in the sidebar's order
    private async Task<IReadOnlyList<HomeResourceLinkViewModel>> GetResourceLinksAsync()
    {
        var items = new List<SidebarItem>();
        foreach (var resource in resources)
        {
            var item = resource.ResolveSidebarItem(services);
            if (
                item.Visible
                && await AuthorizationMetadata.AuthorizeAsync(
                    resource.ResolveAuthorizationMetadata(services),
                    HttpContext
                )
            )
            {
                items.Add(
                    new SidebarGroupItem(
                        item.Group,
                        [ResourceSidebarItemsProvider.CreateLink(resource, item)]
                    )
                );
            }
        }

        return SidebarItemsMerger
            .Merge(items, options.Sidebar)
            .SelectMany(item => ((SidebarGroupItem)item).Items)
            .Select(link => new HomeResourceLinkViewModel(
                link.Label,
                link.Icon,
                ((SidebarActionLinkItem)link).Controller
            ))
            .ToArray();
    }
}
