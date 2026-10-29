using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Index;

public sealed class IndexOverviewScenario : Scenario
{
    public override string Description =>
        "The trips grid with Title and Base price columns, from the introduction's example.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.BasePrice);
                });
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.BasePrice);
                })
            );
        });
    }
}
