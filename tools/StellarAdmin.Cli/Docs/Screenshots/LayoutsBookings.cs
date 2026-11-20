using StellarAdmin.EntityFrameworkCore;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The layouts page's first example: the Bookings and Finance tables and the Cards partial view, with a search and scopes
internal static class LayoutsBookings
{
    public static void Configure(EfCoreResourceBuilder<Booking> resource)
    {
        resource.AllowView(view => view.Fields(fields => fields.Add(booking => booking.Reference)));
        resource.Index(index =>
        {
            index.EnableSearch(term => booking => booking.Reference.Contains(term));
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add(
                    "pending",
                    "Pending",
                    booking => booking.Status == BookingStatus.Pending
                );
            });
            index.AddTable(
                "bookings",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(booking => booking.Reference);
                        columns.Add(booking => booking.Traveller);
                        columns.Add(booking => booking.BookedAt, column => column.Sortable());
                    });
                    table.DefaultSortBy(booking => booking.BookedAt);
                    table.EnablePaging();
                }
            );
            index.AddTable(
                "finance",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(booking => booking.Reference);
                        columns.Add(booking => booking.Status);
                        columns.Add(booking => booking.TotalAmount, column => column.Sortable());
                    });
                    table.DefaultSortByDescending(booking => booking.TotalAmount);
                    table.EnablePaging(paging => paging.PageSize = 50);
                }
            );
            index.AddPartial("cards", "_BookingCards");
        });
    }
}
