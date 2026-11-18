using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Layouts;

public sealed class LayoutsSwitcherScenario : Scenario
{
    public override string Description =>
        "The bookings index with Bookings, Finance and Cards layouts, a search and All and Pending scopes.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(LayoutsBookings.Configure);
        Travellers.Add(admin);
    }
}
