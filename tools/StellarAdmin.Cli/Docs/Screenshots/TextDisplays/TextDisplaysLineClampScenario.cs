using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.TextDisplays;

public sealed class TextDisplaysLineClampScenario : Scenario
{
    public override string Description =>
        "The trips grid with at most 2 lines of each trip's summary.";

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
                        trip => trip.Summary,
                        column => column.UseDisplay<TextDisplay>(text => text.LineClamp = 2)
                    );
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
    }
}
