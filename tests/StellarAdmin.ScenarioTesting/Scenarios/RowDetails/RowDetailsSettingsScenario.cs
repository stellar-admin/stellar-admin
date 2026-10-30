using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsSettingsScenario : Scenario
{
    public override string Description =>
        "Row details that expand one row at a time when it's clicked, with a rail, a full-width divider, bled details, no animation and no toggle.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails(
                    "_BookingSummary",
                    details =>
                    {
                        details.RowClick = true;
                        details.ExpandMode = DataGridRowDetailExpandMode.Single;
                        details.Emphasis = DataGridRowDetailEmphasis.Rail;
                        details.Divider = DataGridRowDetailDivider.FullWidth;
                        details.Inset = DataGridRowDetailInset.Bleed;
                        details.Animate = false;
                        details.Toggle = DataGridRowDetailToggle.None;
                    }
                );
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
