using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

public sealed class RowDetailsHandlerScenario : Scenario
{
    public override string Description =>
        "Row details that a handler loads when the row expands, with one booking whose history it can't find.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<BookingHistoryService>();

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails<BookingHistoryHandler>("_BookingHistory");
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
