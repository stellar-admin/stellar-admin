using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.TitlesAndReferences;

public sealed class TitlesAndReferencesResourceReferenceSearchScenario : Scenario
{
    public override string Description =>
        "A trip's edit page with a lead guide lookup that searches guides on their name and email, orders them by last name and offers only active guides.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Reference(reference =>
                reference
                    .UseDescription(guide => guide.Email)
                    .UseAvatar(guide => guide.PhotoUrl)
                    .SearchOn(
                        guide => guide.FirstName,
                        guide => guide.LastName,
                        guide => guide.Email
                    )
                    .OrderBy(guide => guide.LastName)
                    .Where(guide => guide.IsActive)
            );
            resource.Index(index => index.EnablePaging());
        });
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.LeadGuideId);
                    fields.Add(trip => trip.Summary);
                })
            );
        });
    }
}
