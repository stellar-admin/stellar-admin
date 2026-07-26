using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Shell.Sidebar;

namespace StellarAdmin.Shell.Areas.StellarAdmin.ViewComponents;

public class SidebarViewComponent(IEnumerable<ISidebarItemsProvider> sidebarItemsProviders)
    : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var sidebarItems = sidebarItemsProviders
            .SelectMany(provider => provider.GetItems())
            .ToList();

        return View(sidebarItems);
    }
}
