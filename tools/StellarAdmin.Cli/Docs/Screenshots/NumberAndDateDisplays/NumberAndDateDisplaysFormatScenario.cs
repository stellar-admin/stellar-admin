using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.NumberAndDateDisplays;

public sealed class NumberAndDateDisplaysFormatScenario : Scenario
{
    public override string Description =>
        "The bookings grid with each booking's total as a currency, and its departure date with the day of the week.";

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
                        booking => booking.TotalAmount,
                        column => column.UseDisplay<NumberDisplay>(number => number.Format = "C")
                    );
                    columns.Add(
                        booking => booking.Departure.StartDate,
                        column =>
                            column.UseDisplay<DateDisplay>(date => date.Format = "ddd d MMM yyyy")
                    );
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
    }
}
