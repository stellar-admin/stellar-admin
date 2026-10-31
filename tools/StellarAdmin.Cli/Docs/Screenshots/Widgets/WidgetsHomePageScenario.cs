using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Widgets;

public sealed class WidgetsHomePageScenario : Scenario
{
    public override string Description =>
        "The home page without the setup checklist, with Bookings and Revenue in the first row, and Upcoming departures and Quick links in the second.";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.ConfigureHomePage(home =>
        {
            // The home page outside Development
            home.ShowSetupChecklist = false;
            home.Widgets(widgets =>
            {
                widgets.Add<BookingStatsHandler>(
                    "Bookings",
                    "_BookingStatsWidget",
                    widget => widget.Span = 6
                );
                widgets.Add<RevenueHandler>(
                    "Revenue",
                    "_RevenueWidget",
                    widget =>
                    {
                        widget.Span = 6;
                        widget.Parameters(context => new RevenueParameters(30));
                    }
                );
                widgets.Add<UpcomingDeparturesHandler>(
                    "Upcoming departures",
                    "_UpcomingDeparturesWidget",
                    widget => widget.Span = 6
                );
                widgets.Add("Quick links", "_QuickLinksWidget", widget => widget.Span = 6);
            });
        });
        admin.AddEfCoreResource<Booking>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowCreate();
        });
        admin.AddEfCoreResource<Trip>(resource => resource.Index(index => index.EnablePaging()));
        Travellers.Add(admin);
    }

    public sealed class BookingStatsHandler(ApplicationDbContext db) : IWidgetHandler<BookingStats>
    {
        public async Task<BookingStats?> LoadAsync(
            WidgetLoadContext context,
            CancellationToken cancellationToken
        )
        {
            var start = new DateTime(SeedDate.Today.Year, SeedDate.Today.Month, 1);
            var count = await db.Bookings.CountAsync(
                booking => booking.BookedAt >= start,
                cancellationToken
            );

            return new BookingStats(count);
        }
    }

    public record RevenueParameters([property: WidgetInput] int Days);

    public sealed class RevenueHandler(ApplicationDbContext db)
        : IWidgetHandler<RevenueParameters, Revenue>
    {
        public async Task<Revenue?> LoadAsync(
            RevenueParameters parameters,
            WidgetLoadContext context,
            CancellationToken cancellationToken
        )
        {
            var since = SeedDate.Today.ToDateTime(TimeOnly.MinValue).AddDays(-parameters.Days);
            var amounts = await db
                .Bookings.Where(booking =>
                    booking.BookedAt >= since && booking.Status != BookingStatus.Cancelled
                )
                .Select(booking => booking.TotalAmount)
                .ToListAsync(cancellationToken);

            return new Revenue(parameters.Days, amounts.Sum());
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
