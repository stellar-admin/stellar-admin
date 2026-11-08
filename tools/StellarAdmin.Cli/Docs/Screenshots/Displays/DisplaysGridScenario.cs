using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Displays;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Displays;

public sealed class DisplaysGridScenario : Scenario
{
    public override string Description =>
        "The travellers grid with whether each traveller has opted in to marketing as Subscribed or Not subscribed.";

    public override string StartPath => "/stellaradmin/travellers";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Traveller>(resource =>
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(traveller => traveller.FirstName);
                    columns.Add(traveller => traveller.LastName);
                    columns.Add(traveller => traveller.Email);
                    columns.Add(
                        traveller => traveller.MarketingOptIn,
                        column =>
                            column.UseDisplay<BooleanDisplay>(flag =>
                            {
                                flag.Appearance = BooleanDisplayAppearance.Text;
                                flag.TrueText = "Subscribed";
                                flag.FalseText = "Not subscribed";
                            })
                    );
                });
                // Five rows, so the grid crop stays short
                index.EnablePaging(paging => paging.PageSize = 5);
            })
        );
    }
}
