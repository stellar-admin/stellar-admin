using StellarAdmin;
using StellarAdmin.Resources;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.CardArrangement;

// card-arrangement-image-shapes-widescreen, -banner, -landscape and -square: vertical cards with each image shape
[ScenarioArguments(ResourceCardImageShape.Widescreen)]
[ScenarioArguments(ResourceCardImageShape.Banner)]
[ScenarioArguments(ResourceCardImageShape.Landscape)]
[ScenarioArguments(ResourceCardImageShape.Square)]
public sealed class CardArrangementImageShapesScenario(ResourceCardImageShape shape) : Scenario
{
    public override string Description =>
        $"The trips index with vertical cards whose cover image has the {shape.ToString().ToLowerInvariant()} shape.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.EnablePaging();
                cards.Card(card =>
                {
                    card.Image(trip => trip.CoverImageUrl, image => image.Shape = shape);
                    card.Title(trip => trip.Title);
                    card.Description(trip => trip.Summary);
                    card.Badge(trip => trip.Difficulty);
                });
            }
        );
}
