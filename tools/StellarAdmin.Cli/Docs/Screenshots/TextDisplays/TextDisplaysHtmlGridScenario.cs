using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.TextDisplays;

public sealed class TextDisplaysHtmlGridScenario : Scenario
{
    public override string Description =>
        "The trips grid with each trip's HTML description as plain text, cut off after 4 lines.";

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
                        trip => trip.Description,
                        column => column.UseDisplay<HtmlDisplay>(html => html.LineClamp = 4)
                    );
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
