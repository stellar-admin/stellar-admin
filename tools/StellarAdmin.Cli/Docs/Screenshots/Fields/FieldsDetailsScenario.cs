using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Fields;

public sealed class FieldsDetailsScenario : Scenario
{
    public override string Description =>
        "A booking's edit page with its status, and a History section with when it was booked and its reference.";

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
                    fields.Add(booking => booking.Status);
                    fields.AddDetails(
                        "History",
                        details =>
                        {
                            details.Columns(2);
                            details.Variant = DetailsVariant.Divided;
                            details.Add(booking => booking.BookedAt);
                            details.Add(booking => booking.Reference);
                        }
                    );
                })
            );
        });
    }
}
