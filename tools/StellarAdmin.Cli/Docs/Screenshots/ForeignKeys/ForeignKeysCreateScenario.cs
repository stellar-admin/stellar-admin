using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.ForeignKeys;

public sealed class ForeignKeysCreateScenario : Scenario
{
    public override string Description =>
        "The booking create page with a traveller lookup that can create a traveller, in one column of a two-column form.";

    public override string StartPath => "/stellaradmin/bookings/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Traveller>(resource =>
        {
            resource.UseTitle(traveller => traveller.FirstName + " " + traveller.LastName);
            resource.Reference(reference => reference.UseDescription(traveller => traveller.Email));
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(traveller => traveller.FirstName);
                    fields.Add(traveller => traveller.LastName);
                    fields.Add(traveller => traveller.Email);
                })
            );
        });
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            });
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    // One column of a two-column form, so the lookup isn't full width
                    fields.Columns(2);
                    fields
                        .Add(booking => booking.TravellerId)
                        .UseLookupSheetEditor(lookup => lookup.EnableCreate());
                })
            );
        });
    }
}
