using StellarAdmin;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Trips;

// Dependent fields: the city's choices depend on the country, and the neighbourhoods and pickup points on the city.
// The arrival airport is looked up within the country. New trips are added in an inline row, which cascades too.
internal static class TripResourceRegistration
{
    internal static void AddTripResource(this StellarAdminBuilder admin)
    {
        admin.Services.AddSingleton<TripDataSource>();
        admin.Services.AddSingleton<TripCatalog>();
        admin.Services.AddSingleton<TripAirportSource>();
        admin.AddResource<Trip>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Icon = "plane";
                item.Order = 26;
            });
            resource.UseDataSource<TripDataSource>();
            resource.UseKey(trip => trip.Id);
            resource.AllowDelete();
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Name);
                    columns.Add(
                        trip => TripCatalog.CountryName(trip.CountryId),
                        column =>
                        {
                            column.Title = "Country";
                            column.EditsField(trip => trip.CountryId);
                        }
                    );
                    columns.Add(
                        trip => TripCatalog.CityName(trip.CityId),
                        column =>
                        {
                            column.Title = "City";
                            column.EditsField(trip => trip.CityId);
                        }
                    );
                    columns.Add(trip => trip.ArrivalAirport);
                })
            );
            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(trip => trip.Name);
                    AddPlaceFields(fields);
                });
                create.UseInlineRow();
            });
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(trip => trip.Name);
                    AddPlaceFields(fields);
                    fields
                        .Add(trip => trip.NeighbourhoodIds)
                        .DependsOn(
                            trip => trip.CityId,
                            city => city.EmptyText = "Choose a city to see its neighbourhoods"
                        )
                        .UseToggleGroupEditor(group =>
                            group.UseItems(
                                (trip, services, cancellationToken) =>
                                    services
                                        .GetRequiredService<TripCatalog>()
                                        .NeighbourhoodsAsync(trip.CityId!.Value, cancellationToken)
                            )
                        );
                    fields
                        .Add(trip => trip.PickupPointIds)
                        .DependsOn(trip => trip.CityId)
                        .UseCheckboxGroupEditor(group =>
                            group.UseItems(
                                (trip, services, cancellationToken) =>
                                    services
                                        .GetRequiredService<TripCatalog>()
                                        .PickupPointsAsync(trip.CityId!.Value, cancellationToken)
                            )
                        );
                })
            );
        });
    }

    private static void AddPlaceFields(ResourceFieldsBuilder<Trip> fields)
    {
        fields
            .Add(trip => trip.CountryId)
            .UseSelectEditor(select => select.UseItems(TripCatalog.CountryChoices()));
        fields
            .Add(trip => trip.CityId)
            .DependsOn(trip => trip.CountryId)
            .UseSelectEditor(select =>
                select.UseItems(
                    (trip, services, cancellationToken) =>
                        services
                            .GetRequiredService<TripCatalog>()
                            .CitiesAsync(trip.CountryId!.Value, cancellationToken)
                )
            );
        fields
            .Add(trip => trip.ArrivalAirport)
            .DependsOn(trip => trip.CountryId)
            .UseLookupSheetEditor(lookup =>
                lookup.UseSource<TripAirportSource>(trip => trip.CountryId!.Value)
            );
    }
}
