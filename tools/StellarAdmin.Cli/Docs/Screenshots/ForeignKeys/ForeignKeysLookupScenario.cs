using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ForeignKeys;

public sealed class ForeignKeysLookupScenario : Scenario
{
    public override string Description =>
        "The booking create page with a traveller lookup, whose travellers have their email address as the description.";

    public override string StartPath => "/stellaradmin/bookings/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Traveller>(resource =>
        {
            resource.UseTitle(traveller => traveller.FirstName + " " + traveller.LastName);
            resource.Reference(reference => reference.UseDescription(traveller => traveller.Email));
            resource.Index(index => index.EnablePaging());
        });
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            });
            resource.AllowCreate(create =>
                create.Fields(fields => fields.Add(booking => booking.TravellerId))
            );
        });
    }
}
