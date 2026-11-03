using Microsoft.EntityFrameworkCore;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.WidgetData;

public sealed class WidgetDataLoadingScenario : Scenario
{
    public override string Description =>
        "The home page with the Bookings widget loaded and the Revenue widget's placeholder, whose handler never finishes.";

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
                widgets.Add<PendingRevenueHandler>(
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

    // Never finishes, so the placeholder stays for the screenshot
    public sealed class PendingRevenueHandler : IWidgetHandler<Revenue>
    {
        public async Task<Revenue?> LoadAsync(
            WidgetLoadContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return null;
        }
    }
}
