using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.HomePage;

public sealed class HomePageResourceLinksScenario : Scenario
{
    public override string Description =>
        "Five resources with icons and the setup checklist off, so the home page links to the resources in the sidebar's order.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
        AddResource<Trip>(admin, "plane");
        AddResource<Destination>(admin, "map-pin");
        AddResource<Booking>(admin, "ticket");
        AddResource<Traveller>(admin, "users");
        AddResource<Review>(admin, "star");
    }

    private static void AddResource<T>(StellarAdminBuilder admin, string icon)
        where T : class
    {
        admin.AddEfCoreResource<T>(resource =>
        {
            resource.SidebarItem(item => item.Icon = icon);
            resource.Index(index => index.EnablePaging());
        });
    }
}
