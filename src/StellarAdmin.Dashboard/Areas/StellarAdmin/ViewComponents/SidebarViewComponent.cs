using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Dashboard.Sidebar;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewComponents;

public class SidebarViewComponent(IEnumerable<ISidebarItemsProvider> sidebarItemsProviders)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var sidebarItems = new List<SidebarItem>();
        foreach (var provider in sidebarItemsProviders)
        {
            sidebarItems.AddRange(await provider.GetItemsAsync(HttpContext));
        }

        // A full path, so the sidebar renders from host pages that use the Dashboard layout.
        return View(
            "/Areas/StellarAdmin/Views/Shared/Components/Sidebar/Default.cshtml",
            SidebarItemsMerger.Merge(sidebarItems)
        );
    }
}
