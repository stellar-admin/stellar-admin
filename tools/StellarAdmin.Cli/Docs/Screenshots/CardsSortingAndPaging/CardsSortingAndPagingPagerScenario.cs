using StellarAdmin;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.Cli.Docs.Screenshots.CardsSortingAndPaging;

public sealed class CardsSortingAndPagingPagerScenario : Scenario
{
    public override string Description =>
        "The trips index with title and summary cards sorted by title, 12 to a page.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin) =>
        CardTrips.Add(
            admin,
            cards =>
            {
                cards.DefaultSortBy(trip => trip.Title);
                cards.EnablePaging(paging => paging.PageSize = 12);
                cards.Card(card =>
                {
                    card.Title(trip => trip.Title);
                    card.Description(trip => trip.Summary);
                });
            }
        );
}
