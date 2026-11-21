using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.Cards;

// cards-field-layout-rows and cards-field-layout-grid: the first example's card with the field layout example's four fields
[ScenarioArguments(ResourceCardFieldLayout.Rows)]
[ScenarioArguments(ResourceCardFieldLayout.Grid)]
public sealed class CardsFieldLayoutScenario(ResourceCardFieldLayout layout) : Scenario
{
    public override string Description =>
        $"The trips index with cards whose destination, lead guide, trip type and transport fields use the {layout.ToString().ToLowerInvariant()} layout.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.EnablePaging();
                cards.Card(card =>
                {
                    card.Image(trip => trip.CoverImageUrl);
                    card.Title(trip => trip.Title);
                    card.Description(trip => trip.Summary);
                    card.Badge(trip => trip.Difficulty);
                    card.Stats(stats =>
                    {
                        stats.Add(trip => trip.DurationDays);
                        stats.Add(
                            trip => trip.BasePrice,
                            stat => stat.UseDisplay<NumberDisplay>(number => number.Format = "C0")
                        );
                        stats.Add(trip => trip.MaxGroupSize);
                    });
                    card.Fields(fields =>
                    {
                        fields.Layout = layout;
                        fields.Add(trip => trip.Destination);
                        fields.Add(trip => trip.LeadGuide);
                        fields.Add(trip => trip.TripType);
                        fields.Add(trip => trip.Transport);
                    });
                });
            }
        );
}
