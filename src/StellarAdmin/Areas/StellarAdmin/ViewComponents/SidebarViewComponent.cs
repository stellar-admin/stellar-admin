using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Areas.StellarAdmin.ViewComponents;

/// <summary>
///     Renders the admin panel sidebar.
/// </summary>
/// <param name="sidebarItemsProviders">The providers of the sidebar items.</param>
public sealed class SidebarViewComponent(IEnumerable<ISidebarItemsProvider> sidebarItemsProviders)
    : ViewComponent
{
    /// <summary>
    ///     Renders the sidebar with the items of every provider.
    /// </summary>
    /// <returns>A task that returns the result that renders the sidebar.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var sidebarItems = new List<SidebarItem>();
        foreach (var provider in sidebarItemsProviders)
        {
            sidebarItems.AddRange(
                await provider.GetItemsAsync(HttpContext, HttpContext.RequestAborted)
            );
        }

        // A full path, so the sidebar renders from host pages that use the admin panel layout.
        return View(
            "/Areas/StellarAdmin/Views/Shared/Components/Sidebar/Default.cshtml",
            SidebarItemsMerger.Merge(
                sidebarItems,
                HttpContext.RequestServices.GetRequiredService<StellarAdminOptions>().Sidebar
            )
        );
    }
}
