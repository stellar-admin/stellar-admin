using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Destinations;

public sealed record DestinationCountry(string Country, IReadOnlyList<string> Destinations);

// The destinations grouped by country. The handler loads its own data, so it would apply a scope or search itself
public sealed class DestinationCountriesHandler(DestinationDataSource destinations)
    : IResourceLayoutHandler<DestinationCountry>
{
    public async Task<ResourceListResult<DestinationCountry>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await destinations.ListAsync(new(), cancellationToken);
        var items = result
            .Items.GroupBy(destination => destination.Country)
            .OrderBy(country => country.Key)
            .Select(country => new DestinationCountry(
                country.Key,
                country.Select(destination => destination.Name).ToList()
            ))
            .ToList();

        return new(items, items.Count);
    }
}
