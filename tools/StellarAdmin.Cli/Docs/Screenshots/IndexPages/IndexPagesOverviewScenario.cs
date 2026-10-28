using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IndexPages;

public sealed class IndexPagesOverviewScenario : Scenario
{
    public override string Description =>
        "The bookings grid with three columns, a search box, All and Pending scopes, a default sort and paging.";

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
                    columns.Add(booking => booking.BookedAt, column => column.Sortable());
                });
                index.EnableSearch(term => booking => booking.Reference.Contains(term));
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All");
                    scopes.Add(
                        "pending",
                        "Pending",
                        booking => booking.Status == BookingStatus.Pending
                    );
                    scopes.DefaultScope = "all";
                });
                index.DefaultSortBy(booking => booking.BookedAt);
                index.EnablePaging();
            })
        );
        Travellers.Add(admin);
    }
}
