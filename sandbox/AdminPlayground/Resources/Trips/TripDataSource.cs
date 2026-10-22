using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Trips;

// Kept in memory for the lifetime of the app
public sealed class TripDataSource : IResourceCrudDataSource<Trip>
{
    private readonly List<Trip> _trips =
    [
        new()
        {
            Id = 1,
            Name = "Temples and tea in Kyoto",
            CountryId = 2,
            CityId = 21,
            ArrivalAirport = "KIX",
            NeighbourhoodIds = [211, 213],
            PickupPointIds = [2101],
        },
        new()
        {
            Id = 2,
            Name = "Hillside murals of Valparaíso",
            CountryId = 1,
            CityId = 12,
            ArrivalAirport = "SCL",
            NeighbourhoodIds = [121, 122],
        },
        new()
        {
            Id = 3,
            Name = "Lisbon by tram",
            CountryId = 3,
            CityId = 31,
            NeighbourhoodIds = [311],
        },
    ];
    private readonly Lock _lock = new();
    private int _nextId = 4;

    public Task<ResourceOperationResult> CreateAsync(
        Trip model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            model.Id = _nextId++;
            _trips.Add(Copy(model));

            return Task.FromResult(
                ResourceOperationResult.Success(
                    model.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
                )
            );
        }
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key) && _trips.RemoveAll(trip => trip.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Trip?> FindAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                    ? _trips.Where(trip => trip.Id == key).Select(Copy).FirstOrDefault()
                    : null
            );
        }
    }

    public Task<ResourceListResult<Trip>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            var items = _trips.OrderBy(trip => trip.Name).Select(Copy).ToList();

            return Task.FromResult(new ResourceListResult<Trip>(items, items.Count));
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Trip model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            if (
                !int.TryParse(id, out var key)
                || _trips.FindIndex(trip => trip.Id == key) is var index && index < 0
            )
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            model.Id = key;
            _trips[index] = Copy(model);

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }

    private static Trip Copy(Trip trip) =>
        new()
        {
            ArrivalAirport = trip.ArrivalAirport,
            CityId = trip.CityId,
            CountryId = trip.CountryId,
            Id = trip.Id,
            Name = trip.Name,
            NeighbourhoodIds = [.. trip.NeighbourhoodIds],
            PickupPointIds = [.. trip.PickupPointIds],
        };
}
