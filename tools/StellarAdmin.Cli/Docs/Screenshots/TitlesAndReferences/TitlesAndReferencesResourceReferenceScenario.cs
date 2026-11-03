using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.TitlesAndReferences;

public sealed class TitlesAndReferencesResourceReferenceScenario : Scenario
{
    public override string Description =>
        "The trips index with title, destination and lead guide columns, where each guide is shown by its first and last name.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource => resource.Index(index => index.EnablePaging()));
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Index(index => index.EnablePaging());
        });
        admin.AddEfCoreResource<Trip>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.Destination);
                    columns.Add(trip => trip.LeadGuide);
                });
            })
        );
    }
}
