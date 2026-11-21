using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.CardArrangement;

public sealed class CardArrangementOverviewScenario : Scenario
{
    public override string Description =>
        "The trips index with horizontal cards, square images and the actions in the footer.";

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
                    card.ActionsPlacement = ResourceCardActionsPlacement.Footer;
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
