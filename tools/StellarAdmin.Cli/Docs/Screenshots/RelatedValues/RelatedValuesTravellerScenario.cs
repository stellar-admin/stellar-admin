using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RelatedValues;

public sealed class RelatedValuesTravellerScenario : Scenario
{
    public override string Description =>
        "The bookings grid with each booking's traveller.";

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
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
        Travellers.Add(admin);
    }
}
