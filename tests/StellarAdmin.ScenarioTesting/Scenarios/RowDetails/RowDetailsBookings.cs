using System.Linq.Expressions;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.ScenarioTesting.Host.Models;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

// The bookings grid the row details scenarios share, and their row detail handler
internal static class RowDetailsBookings
{
    // The booking whose history the handler can't find, as if it was deleted after the grid loaded
    public const int MissingHistoryId = 2;

    public static void AddColumns(ResourceColumnsBuilder<Booking> columns)
    {
        columns.Add(booking => booking.Reference);
        columns.Add(booking => booking.BookedAt);
        columns.Add(booking => booking.Status);
        columns.Add(booking => booking.TotalAmount);
    }

    // The projection the Pricing details load when they're shown
    public static readonly Expression<Func<Booking, BookingPricing>> Pricing =
        booking => new BookingPricing(
            booking.TotalAmount,
            booking.Passengers.Count,
            booking.Departure.Trip.BasePrice
        );
}

public sealed class BookingHistoryHandler(BookingHistoryService history)
    : IResourceRowDetailHandler<BookingHistory>
{
    public Task<BookingHistory?> FindAsync(string id, CancellationToken cancellationToken) =>
        history.FindAsync(int.Parse(id), cancellationToken);
}

// The same changes for every booking except the missing one
public sealed class BookingHistoryService
{
    public Task<BookingHistory?> FindAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(
            id == RowDetailsBookings.MissingHistoryId
                ? null
                : new BookingHistory([
                    new BookingChange(new DateTime(2025, 3, 2), "Booked online"),
                    new BookingChange(new DateTime(2025, 3, 2), "Deposit paid"),
                    new BookingChange(new DateTime(2025, 4, 14), "Balance paid"),
                ])
        );
}
