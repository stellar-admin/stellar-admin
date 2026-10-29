using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Fields;

public sealed class FieldsReadOnlyDisabledScenario : Scenario
{
    public override string Description =>
        "A booking's edit page with a read-only reference, a total that is read-only once confirmed, and disabled special requests.";

    public override string StartPath => "/stellaradmin/bookings/edit/7";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(booking => booking.Reference).ReadOnly();
                    fields
                        .Add(booking => booking.TotalAmount)
                        .ReadOnly(booking => booking.Status == BookingStatus.Confirmed);
                    fields.Add(booking => booking.SpecialRequests).Disabled();
                })
            );
        });
    }
}
