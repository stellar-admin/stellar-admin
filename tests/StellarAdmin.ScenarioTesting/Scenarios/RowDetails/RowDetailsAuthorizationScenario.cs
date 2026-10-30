using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsAuthorizationScenario : Scenario
{
    public override string Description =>
        "Bookings for signed-in users only, with row details that a handler loads when the row expands.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<BookingHistoryService>();

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.RequireAuthorization();
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails<BookingHistoryHandler>("_BookingHistory");
                index.EnablePaging(paging => paging.PageSize = 10);
            });
        });
    }
}
