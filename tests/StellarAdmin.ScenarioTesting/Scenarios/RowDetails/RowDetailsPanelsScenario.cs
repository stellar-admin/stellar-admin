using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios.RowDetails;

// One scenario per panel layout, e.g. row-details-panels-tabs-line. The first panel loads when it's shown, so
// tabs load it when the row expands
[ScenarioArguments("accordion")]
[ScenarioArguments("accordion-single")]
[ScenarioArguments("tabs")]
[ScenarioArguments("tabs-line")]
public sealed class RowDetailsPanelsScenario(string layout) : Scenario
{
    public override string Description =>
        $"Row details in History, Summary and Pricing panels, laid out as {layout}; Pricing has an icon and its own name.";

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
                        panels.Add<BookingHistoryHandler>("History", "_BookingHistory");
                        panels.Add("Summary", "_BookingSummary");
                        panels.Add(
                            "Pricing",
                            "_BookingPricing",
                            RowDetailsBookings.Pricing,
                            panel =>
                            {
                                panel.Name = "prices";
                                panel.Icon = "receipt";
                            }
                        );
                    },
                    details =>
                        details.PanelLayout = layout switch
                        {
                            "accordion-single" => ResourceRowDetailPanelLayout.Accordion(
                                ResourceRowDetailAccordionExpandMode.Single
                            ),
                            "tabs" => ResourceRowDetailPanelLayout.Tabs(),
                            "tabs-line" => ResourceRowDetailPanelLayout.Tabs(
                                ResourceRowDetailTabStyle.Line
                            ),
                            _ => ResourceRowDetailPanelLayout.Accordion(),
                        }
                );
                index.EnablePaging(paging => paging.PageSize = 10);
            })
        );
    }
}
