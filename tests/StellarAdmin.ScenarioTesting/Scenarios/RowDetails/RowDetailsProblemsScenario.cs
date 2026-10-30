using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

// Each resource gets its row details wrong in its own way
public sealed class RowDetailsProblemsScenario : Scenario
{
    public override string Description =>
        "Row detail configuration problems: a panel handler for two models and an undefined tab style on bookings, a handler that isn't one on trips, and a handler on partners, which have no key.";

    public override string StartPath => "/stellaradmin/bookings";

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<BookingHistoryService>();

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.Columns(RowDetailsBookings.AddColumns);
                index.EnableRowDetails(
                    panels =>
                    {
                        panels.Add("Summary", "_BookingSummary");
                        panels.Add("Pricing", "_BookingPricing", RowDetailsBookings.Pricing);
                        panels.Add<BookingHistoryAndPricingHandler>("History", "_BookingHistory");
                    },
                    details =>
                        details.PanelLayout = ResourceRowDetailPanelLayout.Tabs(
                            (ResourceRowDetailTabStyle)7
                        )
                );
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns => columns.Add(trip => trip.Title));
                index.EnableRowDetails<BookingHistoryService>("_BookingHistory");
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
        admin.AddResource<Partner>(resource =>
        {
            resource.UseDataSource<PartnerDataSource>();
            resource.Index(index =>
            {
                index.Columns(columns => columns.Add(partner => partner.Name));
                index.EnableRowDetails<BookingHistoryHandler>("_BookingHistory");
            });
        });
    }

    // Loads two models, so the panel's partial view model isn't known
    public sealed class BookingHistoryAndPricingHandler
        : IResourceRowDetailHandler<BookingHistory>,
            IResourceRowDetailHandler<BookingPricing>
    {
        Task<BookingHistory?> IResourceRowDetailHandler<BookingHistory>.FindAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult<BookingHistory?>(null);

        Task<BookingPricing?> IResourceRowDetailHandler<BookingPricing>.FindAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult<BookingPricing?>(null);
    }

    // A travel partner from outside the database, with no key
    public sealed record Partner(string Name);

    public sealed class PartnerDataSource : IResourceDataSource<Partner>
    {
        public Task<ResourceListResult<Partner>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new ResourceListResult<Partner>(
                    [new Partner("Alpine Rail Tours"), new Partner("Coral Coast Cruises")],
                    2
                )
            );
    }
}
