namespace StellarAdmin.Scenarios.Fixtures;

public static class TripResource
{
    public static StellarAdminBuilder AddTripResource(this StellarAdminBuilder admin) =>
        admin.AddResource<Trip>(resource =>
        {
            resource.UseDataSource<TripDataSource>();
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(trip => trip.Name);
                    columns.Add(trip => trip.Destination);
                    columns.Add(trip => trip.DepartsOn);
                    columns.Add(trip => trip.Price);
                })
            );
        });
}
