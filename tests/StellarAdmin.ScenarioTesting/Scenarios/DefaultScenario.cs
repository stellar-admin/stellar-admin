using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Builders;
using StellarAdmin.ScenarioTesting.Hosting;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Scenarios;

public sealed class DefaultScenario : Scenario
{
    public override string Description =>
        "Trips on the Voyager Travel data, with search, create, edit and delete.";

    public override string Name => "default";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Trip>(resource =>
        {
            resource.UseTitle(trip => trip.Title);
            resource.Index(index =>
            {
                index.EnableSearch(term => trip => trip.Title.Contains(term));
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Code);
                    columns.Add(trip => trip.Title);
                    columns.Add(trip => trip.BasePrice);
                    columns.Add(trip => trip.IsPublished);
                });
            });
            resource.AllowCreate(create => create.Fields(TripFields));
            resource.AllowEdit(edit => edit.Fields(TripFields));
            resource.AllowDelete();
        });
    }

    private static void TripFields(ResourceFieldsBuilder<Trip> fields)
    {
        fields.Add(trip => trip.Title);
        fields.Add(trip => trip.Code);
        fields.Add(trip => trip.DestinationId);
        fields.Add(trip => trip.BasePrice);
        fields.Add(trip => trip.IsPublished);
    }
}
