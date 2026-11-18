using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Layouts;

public sealed class LayoutsPartialScenario : Scenario
{
    public override string Description =>
        "The bookings index with Bookings, Finance and Cards layouts, the Cards partial view selected.";

    public override string StartPath => "/stellaradmin/bookings?layout=cards&search=VB-10000";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddStylesheet("~/css/booking-cards.css");
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(LayoutsBookings.Configure);
        Travellers.Add(admin);
    }
}
