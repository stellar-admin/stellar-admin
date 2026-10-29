using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RowDetails;

public sealed class RowDetailsTabsScenario : Scenario
{
    public override string Description =>
        "The bookings grid with row details in Summary, Pricing and History tabs.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<BookingHistoryService>();

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns => RowDetailsBookings.AddColumns(columns));
                index.EnableRowDetails(panels =>
                {
                    panels.Add("Summary", "_BookingSummary");
                    panels.Add(
                        "Pricing",
                        "_BookingPricing",
                        booking => new BookingPricing(
                            booking.TotalAmount,
                            booking.Passengers.Count,
                            booking.Departure.Trip.BasePrice
                        )
                    );
                    panels.Add<BookingHistoryHandler>("History", "_BookingHistory");
                });
                // Ten rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }

    public sealed class BookingHistoryHandler(BookingHistoryService history)
        : IResourceRowDetailHandler<BookingHistory>
    {
        public Task<BookingHistory?> FindAsync(string id, CancellationToken cancellationToken) =>
            history.FindAsync(int.Parse(id), cancellationToken);
    }

    // Stands in for the docs' history service with the same changes for every booking
    public sealed class BookingHistoryService
    {
        public Task<BookingHistory?> FindAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<BookingHistory?>(
                new BookingHistory([
                    new BookingChange(new DateTime(2025, 3, 2), "Booked online"),
                    new BookingChange(new DateTime(2025, 3, 2), "Deposit paid"),
                    new BookingChange(new DateTime(2025, 4, 14), "Balance paid"),
                ])
            );
    }
}
