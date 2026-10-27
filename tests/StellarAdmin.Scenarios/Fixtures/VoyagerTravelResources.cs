using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Builders;
using VoyagerTravel.Models;

namespace StellarAdmin.Scenarios.Fixtures;

public static class VoyagerTravelResources
{
    // Trips, destinations and countries on the Voyager Travel database
    public static StellarAdminBuilder AddVoyagerTravelResources(this StellarAdminBuilder admin)
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
        admin.AddEfCoreResource<Destination>(resource =>
        {
            resource.UseTitle(destination => destination.Name);
            resource.Index(index =>
                index.Columns(columns => columns.Add(destination => destination.Name))
            );
        });
        admin.AddEfCoreResource<Country>(resource =>
        {
            resource.UseTitle(country => country.Name);
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(country => country.Name);
                    columns.Add(country => country.IsoCode);
                })
            );
        });

        return admin;
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
