using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Host.Models;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.Widgets;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.FormWidgets;

public sealed class FormWidgetsTripScenario : Scenario
{
    public override string Description =>
        "A trip's edit page with the trip's upcoming departures in a column beside the form.";

    public override string StartPath => "/stellaradmin/trips/edit/1";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index => index.EnablePaging());
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.Code);
                    fields.Add(trip => trip.Summary);
                    fields.Add(trip => trip.DurationDays);
                    fields.Add(trip => trip.BasePrice);
                });
                edit.Widgets(widgets =>
                {
                    widgets.Add<UpcomingDeparturesHandler>(
                        "Upcoming departures",
                        "_UpcomingDeparturesWidget",
                        widget =>
                        {
                            widget.Placement = WidgetPlacement.Aside;
                            widget.Parameters(context => new UpcomingDeparturesParameters(
                                context.Record.Id
                            ));
                        }
                    );
                });
            });
        });
    }

    public record UpcomingDeparturesParameters(int TripId);

    public sealed class UpcomingDeparturesHandler(ApplicationDbContext db)
        : IWidgetHandler<UpcomingDeparturesParameters, IReadOnlyList<UpcomingDeparture>>
    {
        public Task<IReadOnlyList<UpcomingDeparture>?> LoadAsync(
            UpcomingDeparturesParameters parameters,
            WidgetLoadContext context,
            CancellationToken cancellationToken
        ) => UpcomingDepartures.LoadAsync(db, parameters.TripId, cancellationToken)!;
    }
}
