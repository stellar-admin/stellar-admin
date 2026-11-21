using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.DataFromAHandler;

public sealed class DataFromAHandlerCardsScenario : Scenario
{
    public override string Description =>
        "The bookings index with a Bookings table and destinations cards loaded by a handler, the cards selected.";

    public override string StartPath => "/stellaradmin/bookings?layout=destinations";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.EnableSearch(term => booking => booking.Reference.Contains(term));
                index.AddTable(
                    "bookings",
                    table =>
                    {
                        table.Columns(columns => columns.Add(booking => booking.Reference));
                        table.EnablePaging();
                    }
                );
                index.AddCards<DestinationRevenue, DestinationRevenueHandler>(
                    "destinations",
                    cards =>
                    {
                        cards.Card(card =>
                        {
                            card.Title(item => item.Destination);
                            card.Stats(stats =>
                            {
                                stats.Add(item => item.Bookings);
                                stats.Add(
                                    item => item.Revenue,
                                    stat =>
                                        stat.UseDisplay<NumberDisplay>(number =>
                                            number.Format = "C0"
                                        )
                                );
                            });
                        });
                    }
                );
            })
        );
    }
}
