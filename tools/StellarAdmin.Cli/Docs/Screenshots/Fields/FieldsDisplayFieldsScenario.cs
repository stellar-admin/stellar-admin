using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Fields;

public sealed class FieldsDisplayFieldsScenario : Scenario
{
    public override string Description =>
        "A booking's edit page with display fields for its reference, booking date, traveller's email and total, and an editable status.";

    public override string StartPath => "/stellaradmin/bookings/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.AddDisplayField(booking => booking.Reference);
                    fields.AddDisplayField(booking => booking.BookedAt, field => field.Title = "Booked on");
                    fields.AddDisplayField(booking => booking.Traveller.Email);
                    fields
                        .AddDisplayField(booking => booking.TotalAmount)
                        .UseDisplay<NumberDisplay>(number => number.Format = "C");
                    fields.Add(booking => booking.Status);
                })
            );
        });
    }
}
