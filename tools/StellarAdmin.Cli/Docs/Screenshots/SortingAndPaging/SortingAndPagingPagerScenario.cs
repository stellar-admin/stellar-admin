using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.SortingAndPaging;

public sealed class SortingAndPagingPagerScenario : Scenario
{
    public override string Description =>
        "The bookings grid with sortable Booked At and Total Amount columns, and paging.";

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
                    columns.Add(booking => booking.BookedAt, column => column.Sortable());
                    columns.Add(booking => booking.TotalAmount, column => column.Sortable());
                });
                index.EnablePaging();
            })
        );
    }
}
