using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Search;

public sealed class SearchAsYouTypeScenario : Scenario
{
    public override string Description =>
        "The bookings grid with a search box that searches the reference and the traveller's last name.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                    columns.Add(booking => booking.Status);
                });
                index.EnableSearch(term =>
                    booking =>
                        booking.Reference.Contains(term)
                        || booking.Traveller.LastName.Contains(term)
                );
                index.EnablePaging();
            })
        );
        Travellers.Add(admin);
    }
}
