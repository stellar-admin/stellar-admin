using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.IndexPageWidgets;

public sealed class IndexPageWidgetsBookingsScenario : Scenario
{
    public override string Description =>
        "The bookings grid with All and Pending scopes and a search, the totals of the bookings it lists above it, and Upcoming departures beside it.";

    public override string StartPath => "/stellaradmin/bookings?scope=pending";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(booking => booking.Reference);
                    columns.Add(booking => booking.Traveller);
                    columns.Add(booking => booking.Status);
                });
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All");
                    scopes.Add(
                        "pending",
                        "Pending",
                        booking => booking.Status == BookingStatus.Pending
                    );
                    scopes.DefaultScope = "all";
                });
                index.EnableSearch(term => booking => booking.Reference.Contains(term));
                // Ten rows, so the page stays short
                index.EnablePaging(paging => paging.PageSize = 10);
                index.Widgets(widgets =>
                {
                    widgets.Add<BookingTotalsHandler>(
                        "Totals",
                        "_BookingTotalsWidget",
                        widget =>
                            widget.Parameters(context => new BookingTotalsParameters(
                                context.Scope,
                                context.Search
                            ))
                    );
                    widgets.Add<UpcomingDeparturesHandler>(
                        "Upcoming departures",
                        "_UpcomingDeparturesWidget",
                        widget => widget.Placement = WidgetPlacement.Aside
                    );
                });
            })
        );
        Travellers.Add(admin);
    }

    public record BookingTotalsParameters(string? Scope, string? Search);

    public sealed class BookingTotalsHandler(ApplicationDbContext db)
        : IWidgetHandler<BookingTotalsParameters, BookingTotals>
    {
        public async Task<BookingTotals?> LoadAsync(
            BookingTotalsParameters parameters,
            WidgetLoadContext context,
            CancellationToken cancellationToken
        )
        {
            var bookings = db.Bookings.AsQueryable();
            if (parameters.Scope == "pending")
                bookings = bookings.Where(booking => booking.Status == BookingStatus.Pending);

            if (parameters.Search is { } search)
                bookings = bookings.Where(booking => booking.Reference.Contains(search));

            var amounts = await bookings
                .Select(booking => booking.TotalAmount)
                .ToListAsync(cancellationToken);

            return new BookingTotals(amounts.Count, amounts.Sum());
        }
    }

    public sealed class UpcomingDeparturesHandler(ApplicationDbContext db)
        : IWidgetHandler<IReadOnlyList<UpcomingDeparture>>
    {
        public Task<IReadOnlyList<UpcomingDeparture>?> LoadAsync(
            WidgetLoadContext context,
            CancellationToken cancellationToken
        ) => UpcomingDepartures.LoadAsync(db, null, cancellationToken)!;
    }
}
