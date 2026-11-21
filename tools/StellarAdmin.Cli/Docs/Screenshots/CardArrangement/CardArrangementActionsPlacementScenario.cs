using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.CardArrangement;

// card-arrangement-actions-placement-menu and -footer: the page's last example with the actions in each placement
[ScenarioArguments(ResourceCardActionsPlacement.Menu)]
[ScenarioArguments(ResourceCardActionsPlacement.Footer)]
public sealed class CardArrangementActionsPlacementScenario(ResourceCardActionsPlacement placement)
    : Scenario
{
    public override string Description =>
        $"The trips index with horizontal cards, square images and the actions {(placement == ResourceCardActionsPlacement.Menu ? "in a menu" : "in the footer")}.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.EnablePaging();
                cards.Card(card =>
                {
                    card.Arrangement = ResourceCardArrangement.Horizontal;
                    card.ActionsPlacement = placement;
                    card.Image(
                        trip => trip.CoverImageUrl,
                        image => image.Shape = ResourceCardImageShape.Square
                    );
                    card.Title(trip => trip.Title);
                    card.Badge(trip => trip.Difficulty);
                    card.Stats(stats =>
                    {
                        stats.Add(trip => trip.DurationDays);
                        stats.Add(trip => trip.MaxGroupSize);
                    });
                });
            },
            actions: true
        );
}
