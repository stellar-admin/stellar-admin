using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.UI;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.BadgeDisplay;

public sealed class BadgeDisplayLimitScenario : Scenario
{
    public override string Description =>
        "The trips grid with at most 3 of each trip's highlights as outline badges.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(
                        trip => trip.Highlights,
                        column =>
                            column.UseDisplay<Resources.Displays.BadgeDisplay>(badge =>
                            {
                                badge.Variant = BadgeVariant.Outline;
                                badge.Limit = 3;
                            })
                    );
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
    }
}
