using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Excursions;

// Lists excursions only: it saves nothing, so the create form the resource asks for has no handler
internal sealed class ExcursionDataSource : IResourceDataSource<Excursion>
{
    private static readonly Excursion[] Excursions =
    [
        new()
        {
            Id = 1,
            Name = "Sunrise hike",
            Destination = "Kyoto",
            Price = 45m,
        },
        new()
        {
            Id = 2,
            Name = "Harbour cruise",
            Destination = "Lisbon",
            Price = 60m,
        },
        new()
        {
            Id = 3,
            Name = "Spice market tour",
            Destination = "Marrakesh",
            Price = 35m,
        },
    ];

    public Task<ResourceListResult<Excursion>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResourceListResult<Excursion>(Excursions, Excursions.Length));
}
