using StellarAdmin.Resources.Builders;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

internal static class RowDetailsBookings
{
    // The columns of the row details screenshots' bookings grid
    public static void AddColumns(ResourceColumnsBuilder<Booking> columns)
    {
        columns.Add(booking => booking.Reference);
        columns.Add(booking => booking.BookedAt);
        columns.Add(booking => booking.Status);
        columns.Add(booking => booking.TotalAmount);
    }
}
