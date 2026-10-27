using StellarAdmin.Resources;

namespace StellarAdmin.Scenarios.Fixtures;

// Fixed data, so every run renders the same rows
public sealed class TripDataSource : IResourceDataSource<Trip>
{
    private static readonly Trip[] Trips =
    [
        new()
        {
            Id = 1,
            Name = "Fjords by rail",
            Destination = "Norway",
            DepartsOn = new(2027, 5, 14),
            Price = 2890m,
        },
        new()
        {
            Id = 2,
            Name = "Kyoto in autumn",
            Destination = "Japan",
            DepartsOn = new(2027, 11, 3),
            Price = 3450m,
        },
        new()
        {
            Id = 3,
            Name = "Patagonia trek",
            Destination = "Chile",
            DepartsOn = new(2027, 2, 8),
            Price = 4120m,
        },
        new()
        {
            Id = 4,
            Name = "Amalfi coast drive",
            Destination = "Italy",
            DepartsOn = new(2027, 6, 21),
            Price = 2240m,
        },
        new()
        {
            Id = 5,
            Name = "Serengeti safari",
            Destination = "Tanzania",
            DepartsOn = new(2027, 8, 2),
            Price = 5680m,
        },
    ];

    public Task<ResourceListResult<Trip>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<Trip> items = Trips;
        if (request.Search is { } search)
        {
            items = items.Where(trip =>
                trip.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }

        var totalCount = items.Count();
        if (request.Paging is { } paging)
        {
            items = items.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);
        }

        return Task.FromResult(new ResourceListResult<Trip>(items.ToArray(), totalCount));
    }
}
