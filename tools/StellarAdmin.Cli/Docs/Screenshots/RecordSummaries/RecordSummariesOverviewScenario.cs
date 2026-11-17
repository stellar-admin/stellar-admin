using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.RecordSummaries;

public sealed class RecordSummariesOverviewScenario : Scenario
{
    public override string Description =>
        "A trip's view page with its code, destination and difficulty badge under the title.";

    public override string StartPath => "/stellaradmin/trips/view/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Destination>(resource =>
            resource.Index(index => index.EnablePaging())
        );
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
            {
                index.EnablePaging();
                index.Columns(columns => columns.Add(trip => trip.Title));
            });
            resource.Summary(summary =>
            {
                summary.SeparateItems = true;
                summary.Add(trip => trip.Code);
                summary.Add(trip => trip.Destination);
                summary.Add(
                    trip => trip.Difficulty,
                    item => item.UseDisplay<Resources.Displays.BadgeDisplay>()
                );
            });
            resource.AllowView(view =>
                view.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DurationDays);
                })
            );
            resource.AllowEdit(edit => edit.Fields(fields => fields.Add(trip => trip.Title)));
        });
    }
}
