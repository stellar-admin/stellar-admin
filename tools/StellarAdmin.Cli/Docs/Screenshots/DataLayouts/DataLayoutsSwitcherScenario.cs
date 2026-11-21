using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DataLayouts;

public sealed class DataLayoutsSwitcherScenario : Scenario
{
    public override string Description =>
        "The bookings index with Bookings and Finance tables and Cards data layouts, a search and All and Pending scopes.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(DataLayoutsBookings.Configure);
        Travellers.Add(admin);
    }
}
