using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public enum Season
{
    Spring,
    Summer,

    [Display(Name = "Autumn and winter")]
    Autumn,
}

public sealed class Tour
{
    public Dictionary<string, int> BaggageAllowance { get; set; } = [];

    public int[] DepartureDays { get; set; } = [];

    [Editable(false)]
    public List<string> Guides { get; set; } = [];

    public List<string> Highlights { get; set; } = [];

    public int Id { get; set; }

    public Dictionary<string, string> LocalNames { get; set; } = [];

    public string Name { get; set; } = "";

    public string? Notes { get; set; }

    public Dictionary<Season, decimal> SeasonalPrices { get; set; } = [];

    [MinLength(1)]
    public List<string> Themes { get; set; } = [];
}

public sealed class TourStore
{
    // The result of the next create or update, or null for success
    public ResourceOperationResult? SaveResult { get; set; }

    public Tour? Saved { get; set; }

    public List<Tour> Tours { get; } = [];
}

public sealed class TourDataSource(TourStore store) : IResourceCrudDataSource<Tour>
{
    public Task<ResourceOperationResult> CreateAsync(
        Tour resource,
        CancellationToken cancellationToken
    ) => Save(resource);

    public Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.NotFound());

    public Task<Tour?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(store.Tours.FirstOrDefault(tour => tour.Id.ToString() == id));

    public Task<ResourceListResult<Tour>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResourceListResult<Tour>(store.Tours.ToArray(), store.Tours.Count));

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Tour resource,
        CancellationToken cancellationToken
    ) => Save(resource);

    private Task<ResourceOperationResult> Save(Tour resource)
    {
        store.Saved = resource;

        return Task.FromResult(store.SaveResult ?? ResourceOperationResult.Success());
    }
}
