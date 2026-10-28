using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Sidebar;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.EnvironmentIndicator;

// One scenario per kind of indicator, e.g. environment-indicator-banner: the docs' live banner and staging
// stripe, and a staging frame
[ScenarioArguments("stripe")]
[ScenarioArguments("frame")]
[ScenarioArguments("banner")]
public sealed class EnvironmentIndicatorScenario(string indicator) : Scenario
{
    public override string Description =>
        $"The trips index with a {indicator} environment indicator.";

    public override string StartPath => "/stellaradmin/trips";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding =>
        {
            branding.Title = "Voyager Travel";
            branding.Subtitle = "Admin";
            branding.Logo = BrandingLogo.Icon("compass");
        });
        admin.EnableEnvironmentIndicator(
            indicator switch
            {
                "banner" => StellarAdmin.EnvironmentIndicator.Banner(
                    "Live",
                    banner =>
                    {
                        banner.Color = EnvironmentColor.Danger;
                        banner.Description = "Changes affect real customers";
                    }
                ),
                "frame" => StellarAdmin.EnvironmentIndicator.Frame(
                    "Staging",
                    frame => frame.Color = EnvironmentColor.Warning
                ),
                _ => StellarAdmin.EnvironmentIndicator.Stripe(
                    "Staging",
                    stripe => stripe.Color = EnvironmentColor.Warning
                ),
            }
        );
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.UseTitle(trip => trip.Title);
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.DurationDays);
                    columns.Add(trip => trip.IsPublished);
                });
            });
        });
        admin.AddEfCoreResource<Destination>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(destination => destination.Name));
            })
        );
        admin.AddEfCoreResource<Booking>(resource =>
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(booking => booking.Reference));
            })
        );
    }
}
