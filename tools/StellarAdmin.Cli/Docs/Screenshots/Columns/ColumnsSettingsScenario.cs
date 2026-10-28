using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Columns;

public sealed class ColumnsSettingsScenario : Scenario
{
    public override string Description =>
        "The bookings grid with four columns: a sortable Booked column and No requests for empty special requests.";

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
                    columns.Add(
                        booking => booking.BookedAt,
                        column =>
                        {
                            column.Title = "Booked";
                            column.Sortable();
                        }
                    );
                    columns.Add(booking => booking.TotalAmount);
                    columns.Add(
                        booking => booking.SpecialRequests,
                        column => column.EmptyText = "No requests"
                    );
                });
                // Ten rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
