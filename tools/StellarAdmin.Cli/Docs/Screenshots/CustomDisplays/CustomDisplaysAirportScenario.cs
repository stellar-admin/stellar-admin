using Microsoft.Extensions.DependencyInjection;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.CustomDisplays;

// A guide's view page with the host's AirportDisplay, whose handler looks up the home airport's name
public sealed class CustomDisplaysAirportScenario : Scenario
{
    public override string Description =>
        "A guide's view page showing the home airport's name, loaded by the AirportDisplay handler, beside its code.";

    public override string StartPath => "/stellaradmin/guides/view/1";

    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSingleton<IAirportDirectory, AirportDirectory>();

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Guide>(resource =>
        {
            resource.UseTitle(guide => guide.FirstName + " " + guide.LastName);
            resource.Index(index => index.EnablePaging());
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(guide => guide.FirstName);
                    fields.Add(guide => guide.LastName);
                    fields.Add(guide => guide.Email);
                    fields.Add(guide => guide.HomeAirport, field => field.UseDisplay<AirportDisplay>());
                })
            );
        });
    }
}
