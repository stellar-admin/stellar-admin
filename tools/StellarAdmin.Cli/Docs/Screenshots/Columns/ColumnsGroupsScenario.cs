using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Columns;

public sealed class ColumnsGroupsScenario : Scenario
{
    public override string Description =>
        "The bookings grid with a Departure group holding the trip and a nested Dates group with the start and end dates.";

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
                    columns.AddGroup(
                        "Departure",
                        departure =>
                        {
                            departure.Add(
                                booking => booking.Departure.Trip.Title,
                                column => column.Title = "Trip"
                            );
                            departure.AddGroup(
                                "Dates",
                                dates =>
                                {
                                    dates.Add(
                                        booking => booking.Departure.StartDate,
                                        column =>
                                        {
                                            column.Title = "Starts";
                                            column.Sortable();
                                        }
                                    );
                                    dates.Add(
                                        booking => booking.Departure.EndDate,
                                        column => column.Title = "Ends"
                                    );
                                }
                            );
                        }
                    );
                    columns.Add(booking => booking.TotalAmount);
                });

                // Five rows, so the grid crop stays short
                index.EnablePaging(paging =>
                {
                    paging.PageSizes = [5, 10, 25];
                    paging.PageSize = 5;
                });
            })
        );
    }
}
