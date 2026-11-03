using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IndexPages;

public sealed class IndexPagesRowActionsScenario : Scenario
{
    public override string Description =>
        "The bookings grid, five rows to a page, with the view, edit and delete actions on each row.";

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
                    columns.Add(booking => booking.BookedAt);
                });
                index.EnablePaging(paging =>
                {
                    paging.PageSizes = [5, 10, 25];
                    paging.PageSize = 5;
                });
            });
            resource.AllowView(view => view.Fields(fields => fields.Add(booking => booking.Reference)));
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(booking => booking.SpecialRequests))
            );
            resource.AllowDelete();
        });
        Travellers.Add(admin);
    }
}
