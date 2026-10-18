using System.Globalization;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Amenities;

// Kept in memory for the lifetime of the app. The spa's icon isn't registered, and laundry has none.
public sealed class AmenityDataSource : IResourceCrudDataSource<Amenity>
{
    private readonly List<Amenity> _amenities =
    [
        new()
        {
            Id = 1,
            Name = "Wi-Fi",
            Icon = "wifi",
        },
        new()
        {
            Id = 2,
            Name = "Pool",
            Icon = "waves-ladder",
        },
        new()
        {
            Id = 3,
            Name = "Breakfast",
            Icon = "coffee",
        },
        new()
        {
            Id = 4,
            Name = "Parking",
            Icon = "square-parking",
        },
        new()
        {
            Id = 5,
            Name = "Airport shuttle",
            Icon = "bus",
        },
        new()
        {
            Id = 6,
            Name = "Spa",
            Icon = "spa-flower",
        },
        new() { Id = 7, Name = "Laundry" },
    ];
    private readonly Lock _lock = new();
    private int _nextId = 8;

    public Task<ResourceOperationResult> CreateAsync(
        Amenity model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            model.Id = _nextId++;
            _amenities.Add(Copy(model));

            return Task.FromResult(
                ResourceOperationResult.Success(model.Id.ToString(CultureInfo.InvariantCulture))
            );
        }
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                && _amenities.RemoveAll(amenity => amenity.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Amenity?> FindAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                    ? _amenities.Where(amenity => amenity.Id == key).Select(Copy).FirstOrDefault()
                    : null
            );
        }
    }

    public Task<ResourceListResult<Amenity>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            var items = _amenities.OrderBy(amenity => amenity.Name).Select(Copy).ToList();

            return Task.FromResult(new ResourceListResult<Amenity>(items, items.Count));
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Amenity model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            if (
                !int.TryParse(id, out var key)
                || _amenities.FindIndex(amenity => amenity.Id == key) is var index && index < 0
            )
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            model.Id = key;
            _amenities[index] = Copy(model);

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }

    private static Amenity Copy(Amenity amenity) =>
        new()
        {
            Icon = amenity.Icon,
            Id = amenity.Id,
            Name = amenity.Name,
        };
}
