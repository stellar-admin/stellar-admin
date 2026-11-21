using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.CardArrangement;

// card-arrangement-arrangements-vertical, -horizontal and -overlay: the page's first example in each arrangement
[ScenarioArguments(ResourceCardArrangement.Vertical)]
[ScenarioArguments(ResourceCardArrangement.Horizontal)]
[ScenarioArguments(ResourceCardArrangement.Overlay)]
public sealed class CardArrangementArrangementsScenario(ResourceCardArrangement arrangement)
    : Scenario
{
    public override string Description =>
        $"The trips index with {arrangement.ToString().ToLowerInvariant()} cards showing the cover image, title, summary and difficulty.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.EnablePaging();
                cards.Card(card =>
                {
                    card.Arrangement = arrangement;
                    card.Image(trip => trip.CoverImageUrl);
                    card.Title(trip => trip.Title);
                    card.Description(trip => trip.Summary);
                    card.Badge(trip => trip.Difficulty);
                });
            }
        );
}
