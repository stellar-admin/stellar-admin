using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.BooleanAndEnumDisplays;

public sealed class BooleanAndEnumDisplaysGridScenario : Scenario
{
    public override string Description =>
        "The trips grid with whether each trip is published as Live or Draft, and its difficulty.";

    public override string StartPath => "/stellaradmin/trips?page=8";

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
                        trip => trip.IsPublished,
                        column =>
                            column.UseDisplay<BooleanDisplay>(flag =>
                            {
                                flag.Appearance = BooleanDisplayAppearance.Text;
                                flag.TrueText = "Live";
                                flag.FalseText = "Draft";
                            })
                    );
                    columns.Add(trip => trip.Difficulty);
                });
                // Six rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 6);
            })
        );
    }
}
