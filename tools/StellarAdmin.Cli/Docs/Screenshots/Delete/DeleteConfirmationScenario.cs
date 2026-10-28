using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Delete;

// delete-confirmation-default with the default text, and delete-confirmation-custom with the docs' own
[ScenarioArguments("default")]
[ScenarioArguments("custom")]
public sealed class DeleteConfirmationScenario(string text) : Scenario
{
    public override string Description =>
        text == "custom"
            ? "The bookings grid's delete confirmation with its own title, message and labels."
            : "The bookings grid's delete confirmation with the default text.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                    columns.Add(booking => booking.Status);
                });
                index.EnablePaging();
            });
            if (text == "custom")
            {
                resource.AllowDelete(delete =>
                {
                    delete.Title = "Cancel this booking?";
                    delete.Message = "The booking and its payment history will be removed.";
                    delete.ConfirmLabel = "Delete booking";
                    delete.CancelLabel = "Keep it";
                });
            }
            else
            {
                resource.AllowDelete();
            }
        });
        Travellers.Add(admin);
    }
}
