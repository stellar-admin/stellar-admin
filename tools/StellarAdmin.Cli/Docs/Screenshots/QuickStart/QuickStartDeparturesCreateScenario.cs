using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.Cli.Docs.Screenshots.QuickStart;

public sealed class QuickStartDeparturesCreateScenario : Scenario
{
    public override string Description =>
        "The departure create form with the trip lookup, from the quick start.";

    public override string StartPath => "/stellaradmin/departures/create";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.ConfigureBranding(branding => branding.Title = "Voyager Travel");
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Code);
                    columns.Add(trip => trip.Title);
                })
            );
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(trip => trip.Code);
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DurationDays);
                })
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Code);
                    fields.Add(trip => trip.Title);
                    fields.Add(trip => trip.DurationDays);
                })
            );
        });
        admin.AddEfCoreResource<Departure>(resource =>
        {
            resource.Index(index =>
            {
                index.Columns(columns =>
                {
                    columns.Add(departure => departure.Trip);
                    columns.Add(departure => departure.StartDate, column => column.Sortable());
                    columns.Add(departure => departure.Capacity);
                    columns.Add(departure => departure.Status);
                });
                index.EnableSearch(term => departure => departure.Trip.Title.Contains(term));
                index.DefaultSortBy(departure => departure.StartDate);
                index.EnablePaging();
            });

            resource.AllowCreate(create =>
            {
                create.UseFactory(() =>
                    new Departure { StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)) }
                );
                create.Fields(fields =>
                {
                    fields.Add(departure => departure.TripId);
                    fields.Add(departure => departure.StartDate);
                    fields.Add(departure => departure.Capacity);
                });
            });

            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.AddDisplayField(departure => departure.Trip);
                    fields.Add(departure => departure.StartDate);
                    fields.Add(departure => departure.Capacity);
                    fields.Add(departure => departure.Status);
                })
            );

            resource.AllowDelete();
        });
    }
}
