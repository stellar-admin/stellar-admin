using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.WidgetData;

public sealed class WidgetDataErrorScenario : Scenario
{
    public override string Description =>
        "The home page with the Bookings widget loaded and the Revenue widget's error, whose handler throws.";

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
                widgets.Add<FailingRevenueHandler>(
                    "Revenue",
                    "_RevenueWidget",
                    widget => widget.Span = 6
                );
            });
        });
        admin.AddEfCoreResource<Booking>(resource => resource.Index(index => index.EnablePaging()));
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

    // Throws, so the widget displays its error
    public sealed class FailingRevenueHandler : IWidgetHandler<Revenue>
    {
        public Task<Revenue?> LoadAsync(
            WidgetLoadContext context,
            CancellationToken cancellationToken
        ) => throw new InvalidOperationException("The revenue service is unavailable.");
    }
}
