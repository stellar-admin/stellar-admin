using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.Cards;

public sealed class CardsOverviewScenario : Scenario
{
    public override string Description =>
        "The trips index with the cards of the cards page's first example.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.EnablePaging();
                cards.Card(CardTrips.FirstExample);
            }
        );
}
