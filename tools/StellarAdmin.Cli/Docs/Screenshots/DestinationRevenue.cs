using Microsoft.EntityFrameworkCore;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The data from a handler page's item type and handler, for its table and cards examples
public sealed record DestinationRevenue(
    int DestinationId,
    string Destination,
    int Bookings,
    decimal Revenue
);

// The docs' handler, except that SQLite can't sum a decimal, so it sums the amounts as doubles
public sealed class DestinationRevenueHandler
    : IEfCoreDataLayoutHandler<Booking, DestinationRevenue>
{
    public async Task<ResourceListResult<DestinationRevenue>> ListAsync(
        IQueryable<Booking> bookings,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var rows = await bookings
            .GroupBy(booking => new
            {
                booking.Departure.Trip.DestinationId,
                booking.Departure.Trip.Destination.Name,
            })
            .OrderBy(destination => destination.Key.Name)
            .Select(destination => new
            {
                destination.Key.DestinationId,
                destination.Key.Name,
                Bookings = destination.Count(),
                Revenue = destination.Sum(booking => (double)booking.TotalAmount),
            })
            .ToListAsync(cancellationToken);
        var items = rows.Select(row => new DestinationRevenue(
                row.DestinationId,
                row.Name,
                row.Bookings,
                (decimal)row.Revenue
            ))
            .ToList();

        return new ResourceListResult<DestinationRevenue>(items, items.Count);
    }
}
