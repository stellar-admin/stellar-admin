using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.LayoutData;

public sealed class LayoutDataTableScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table and a Revenue per destination table loaded by a handler.";

    public override string StartPath => "/stellaradmin/bookings?layout=revenue";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(
                index => index.EnableSearch(term => booking => booking.Reference.Contains(term)),
                layouts =>
                {
                    layouts.AddTable(
                        "bookings",
                        table =>
                        {
                            table.Columns(columns => columns.Add(booking => booking.Reference));
                            table.EnablePaging();
                        }
                    );
                    layouts.AddTable<DestinationRevenue, DestinationRevenueHandler>(
                        "revenue",
                        table =>
                        {
                            table.Title = "Revenue per destination";
                            table.Icon = "chart-column";
                            table.Columns(columns =>
                            {
                                columns.Add(item => item.Destination);
                                columns.Add(item => item.Bookings);
                                columns.Add(item => item.Revenue);
                            });
                        }
                    );
                }
            )
        );
    }

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
}
