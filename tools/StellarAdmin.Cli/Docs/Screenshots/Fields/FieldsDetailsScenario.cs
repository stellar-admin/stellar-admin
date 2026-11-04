using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Fields;

public sealed class FieldsDetailsScenario : Scenario
{
    public override string Description =>
        "A booking's edit page with its status, total and special requests, and a narrow History card with when it was booked and its reference.";

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
                    fields.Columns(3);
                    fields.AddGroup(group =>
                    {
                        group.ColumnSpan(2);
                        group.Add(booking => booking.Status);
                        group.Add(booking => booking.TotalAmount);
                        group.Add(booking => booking.SpecialRequests);
                    });
                    fields.AddDetails(
                        "History",
                        details =>
                        {
                            details.Layout = FormSectionLayout.Card;
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
