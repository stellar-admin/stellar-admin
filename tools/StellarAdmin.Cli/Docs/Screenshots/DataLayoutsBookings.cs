using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots;

// The data layouts page's first example: the Bookings and Finance tables and the Cards, with a search and scopes
internal static class DataLayoutsBookings
{
    public static void Configure(EfCoreResourceBuilder<Booking> resource)
    {
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
                        columns.Add(
                            booking => booking.TotalAmount,
                            column =>
                                column
                                    .Sortable()
                                    .UseDisplay<NumberDisplay>(number => number.Format = "C0")
                        );
                    });
                    table.DefaultSortByDescending(booking => booking.TotalAmount);
                    table.EnablePaging(paging => paging.PageSize = 50);
                }
            );
            index.AddCards(
                "cards",
                cards =>
                    cards.Card(card =>
                    {
                        card.Title(booking => booking.Reference);
                        card.Badge(booking => booking.Status);
                        card.Stats(stats =>
                        {
                            stats.Add(booking => booking.BookedAt);
                            stats.Add(booking => booking.TotalAmount);
                        });
                    })
            );
        });
    }
}
