using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Destinations;

// Kept in memory for the lifetime of the app
public sealed class DestinationDataSource : IResourceCrudDataSource<Destination>
{
    private readonly List<Destination> _destinations =
    [
        new()
        {
            Id = 1,
            Name = "Cape Town",
            Country = "South Africa",
            Photo = "cape-town.jpg",
        },
        new()
        {
            Id = 2,
            Name = "Chiang Mai",
            Country = "Thailand",
            Photo = "chiang-mai.jpg",
        },
        new()
        {
            Id = 3,
            Name = "Kyoto",
            Country = "Japan",
            Photo = "kyoto.jpg",
        },
        new()
        {
            Id = 4,
            Name = "Lisbon",
            Country = "Portugal",
            Photo = "lisbon.jpg",
        },
        new()
        {
            Id = 5,
            Name = "Reykjavík",
            Country = "Iceland",
        },
        new()
        {
            Id = 6,
            Name = "Valparaíso",
            Country = "Chile",
            Photo = "missing.jpg",
        },
    ];
    private readonly Lock _lock = new();
    private int _nextId = 7;

    public Task<ResourceOperationResult> CreateAsync(
        Destination model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            model.Id = _nextId++;
            _destinations.Add(Copy(model));

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
                int.TryParse(id, out var key)
                && _destinations.RemoveAll(destination => destination.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Destination?> FindAsync(string id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                    ? _destinations
                        .Where(destination => destination.Id == key)
                        .Select(Copy)
                        .FirstOrDefault()
                    : null
            );
        }
    }

    public Task<ResourceListResult<Destination>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            var items = _destinations
                .OrderBy(destination => destination.Name)
                .Select(Copy)
                .ToList();

            return Task.FromResult(new ResourceListResult<Destination>(items, items.Count));
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Destination model,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            if (
                !int.TryParse(id, out var key)
                || _destinations.FindIndex(destination => destination.Id == key) is var index
                    && index < 0
            )
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            model.Id = key;
            _destinations[index] = Copy(model);

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }

    private static Destination Copy(Destination destination) =>
        new()
        {
            Country = destination.Country,
            Id = destination.Id,
            Name = destination.Name,
            Photo = destination.Photo,
        };
}
