using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.UI;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.FormLayout;

// One scenario per layout, e.g. form-layout-section-layout-card
[ScenarioArguments(FormSectionLayout.Stacked)]
[ScenarioArguments(FormSectionLayout.Split)]
[ScenarioArguments(FormSectionLayout.Card)]
public sealed class FormLayoutSectionLayoutScenario(FormSectionLayout layout) : Scenario
{
    public override string Description =>
        $"The trip edit page with its sections in the {layout} layout.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        FormLayoutTrips.AddRelatedResources(admin);
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.AllowEdit(edit =>
            {
                edit.SectionLayout = layout;
                edit.Fields(FormLayoutTrips.AddSections);
            });
        });
    }
}
