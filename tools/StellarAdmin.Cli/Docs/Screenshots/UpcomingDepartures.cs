using Microsoft.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The next departures of every trip, or of one trip, for the widget scenarios' Upcoming departures widget
internal static class UpcomingDepartures
{
    public static async Task<IReadOnlyList<UpcomingDeparture>> LoadAsync(
        ApplicationDbContext db,
        int? tripId,
        CancellationToken cancellationToken
    )
    {
        var departures = db.Departures.Where(departure =>
            departure.StartDate >= SeedDate.Today && departure.Status == DepartureStatus.Scheduled
        );
        if (tripId is { } id)
            departures = departures.Where(departure => departure.TripId == id);

        return await departures
            .OrderBy(departure => departure.StartDate)
            .Take(5)
            .Select(departure => new UpcomingDeparture(
                departure.Trip.Title,
                departure.StartDate,
                departure
                    .Bookings.Where(booking => booking.Status != BookingStatus.Cancelled)
                    .SelectMany(booking => booking.Passengers)
                    .Count(),
                departure.Capacity
            ))
            .ToListAsync(cancellationToken);
    }
}
