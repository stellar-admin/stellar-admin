using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.Inputs;

public sealed class InputsBooleanSwitchScenario : Scenario
{
    public override string Description => "A trip's Published field as a switch.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit => edit.Fields(fields => fields.Add(trip => trip.IsPublished).UseToggleEditor()));
        });
    }
}
