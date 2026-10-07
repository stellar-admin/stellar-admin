using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FormLayouts;

// The items of the form layout lookups, whose create form shows a sectioned grid in the sheet
public sealed record LayoutDestination(string Code, string Name, string Country);

// Kept in memory for the lifetime of the app so the lookups can create destinations
public sealed class LayoutDestinationStore
{
    private readonly List<LayoutDestination> _destinations =
    [
        new("AMS", "Amsterdam", "Netherlands"),
        new("BCN", "Barcelona", "Spain"),
        new("CPT", "Cape Town", "South Africa"),
        new("KEF", "Reykjavík", "Iceland"),
        new("LIS", "Lisbon", "Portugal"),
        new("SIN", "Singapore", "Singapore"),
        new("SYD", "Sydney", "Australia"),
    ];
    private readonly Lock _lock = new();

    public ResourceOperationResult Add(LayoutDestination destination)
    {
        lock (_lock)
        {
            if (_destinations.Any(existing => existing.Code == destination.Code))
            {
                return ResourceOperationResult.ValidationFailed(
                    nameof(LayoutDestination.Code),
                    "A destination with this code already exists."
                );
            }

            _destinations.Add(destination);

            return ResourceOperationResult.Success(destination.Code);
        }
    }

    public LayoutDestination[] ToArray()
    {
        lock (_lock)
        {
            return _destinations.ToArray();
        }
    }
}

public sealed class LayoutDestinationLookupSource(LayoutDestinationStore store)
    : ILookupSource<LayoutDestination, string>
{
    public Task<LayoutDestination?> FindAsync(string value, CancellationToken cancellationToken) =>
        Task.FromResult(store.ToArray().FirstOrDefault(destination => destination.Code == value));

    public Task<LookupPage<LayoutDestination>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = store
            .ToArray()
            .Where(destination =>
                string.IsNullOrEmpty(query.Term)
                || destination.Name.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
                || destination.Code.Contains(query.Term, StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(destination => destination.Name, StringComparer.CurrentCulture)
            .ToArray();

        return Task.FromResult(
            new LookupPage<LayoutDestination>(
                matches.Skip(query.Skip).Take(query.Take).ToArray(),
                matches.Length > query.Skip + query.Take
            )
        );
    }
}

public sealed class LayoutDestinationDataSource(LayoutDestinationStore store)
    : IResourceDataSource<LayoutDestination>
{
    public Task<ResourceListResult<LayoutDestination>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var destinations = store.ToArray().OrderBy(destination => destination.Name).ToArray();

        return Task.FromResult(
            new ResourceListResult<LayoutDestination>(destinations, destinations.Length)
        );
    }
}

public sealed class CreateLayoutDestinationModel
{
    [Required]
    [RegularExpression("[A-Z]{3}", ErrorMessage = "Use three capital letters.")]
    public string Code { get; set; } = "";

    [Required]
    public string Country { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    public string? Notes { get; set; }

    public string? Region { get; set; }
}

// The create model isn't the resource, so the handler returns the new destination's key
public sealed class CreateLayoutDestinationHandler(LayoutDestinationStore store)
    : IResourceCreateHandler<CreateLayoutDestinationModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        CreateLayoutDestinationModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(store.Add(new(model.Code, model.Name, model.Country)));
}
