using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FieldEditors;

// The items of the lookup gallery
public sealed record GalleryAirport(string Code, string City, string Country);

// The airports of the lookup gallery, kept in memory for the lifetime of the app so the gallery can create them
public sealed class GalleryAirportStore
{
    private readonly List<GalleryAirport> _airports =
    [
        new("AKL", "Auckland", "New Zealand"),
        new("AMS", "Amsterdam", "Netherlands"),
        new("ATH", "Athens", "Greece"),
        new("BCN", "Barcelona", "Spain"),
        new("BKK", "Bangkok", "Thailand"),
        new("BOG", "Bogotá", "Colombia"),
        new("CAI", "Cairo", "Egypt"),
        new("CDG", "Paris", "France"),
        new("CPT", "Cape Town", "South Africa"),
        new("DEL", "Delhi", "India"),
        new("DXB", "Dubai", "United Arab Emirates"),
        new("EZE", "Buenos Aires", "Argentina"),
        new("FCO", "Rome", "Italy"),
        new("GRU", "São Paulo", "Brazil"),
        new("HKG", "Hong Kong", "China"),
        new("HND", "Tokyo Haneda", "Japan"),
        new("IST", "Istanbul", "Türkiye"),
        new("JFK", "New York", "United States"),
        new("JNB", "Johannesburg", "South Africa"),
        new("KEF", "Reykjavík", "Iceland"),
        new("LAX", "Los Angeles", "United States"),
        new("LHR", "London", "United Kingdom"),
        new("LIS", "Lisbon", "Portugal"),
        new("MAD", "Madrid", "Spain"),
        new("MEX", "Mexico City", "Mexico"),
        new("NBO", "Nairobi", "Kenya"),
        new("NRT", "Tokyo Narita", "Japan"),
        new("ORD", "Chicago", "United States"),
        new("SIN", "Singapore", "Singapore"),
        new("SYD", "Sydney", "Australia"),
        new("YVR", "Vancouver", "Canada"),
        new("ZRH", "Zurich", "Switzerland"),
    ];
    private readonly Lock _lock = new();

    public ResourceOperationResult Add(GalleryAirport airport)
    {
        lock (_lock)
        {
            if (_airports.Any(existing => existing.Code == airport.Code))
            {
                return ResourceOperationResult.ValidationFailed(
                    nameof(GalleryAirport.Code),
                    "An airport with this code already exists."
                );
            }

            _airports.Add(airport);

            return ResourceOperationResult.Success(airport.Code);
        }
    }

    public GalleryAirport[] ToArray()
    {
        lock (_lock)
        {
            return _airports.ToArray();
        }
    }
}

// Searches the airports in memory. A real source would query a database or an API.
public sealed class GalleryAirportLookupSource(GalleryAirportStore store)
    : ILookupSource<GalleryAirport, string>
{
    public Task<GalleryAirport?> FindAsync(string value, CancellationToken cancellationToken) =>
        Task.FromResult(store.ToArray().FirstOrDefault(airport => airport.Code == value));

    public Task<LookupPage<GalleryAirport>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = store
            .ToArray()
            .Where(airport =>
                string.IsNullOrEmpty(query.Term)
                || airport.City.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
                || airport.Code.Contains(query.Term, StringComparison.OrdinalIgnoreCase)
                || airport.Country.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
            )
            .OrderBy(airport => airport.City, StringComparer.CurrentCulture)
            .ToArray();

        return Task.FromResult(
            new LookupPage<GalleryAirport>(
                matches.Skip(query.Skip).Take(query.Take).ToArray(),
                matches.Length > query.Skip + query.Take
            )
        );
    }
}

// Lists the airports for the airport resource, which the gallery's EnableCreate field creates airports with
public sealed class GalleryAirportDataSource(GalleryAirportStore store)
    : IResourceDataSource<GalleryAirport>
{
    public Task<ResourceListResult<GalleryAirport>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var airports = store.ToArray().OrderBy(airport => airport.City).ToArray();

        return Task.FromResult(new ResourceListResult<GalleryAirport>(airports, airports.Length));
    }
}

public sealed class CreateGalleryAirportModel
{
    [Required]
    public string City { get; set; } = "";

    [Required]
    [RegularExpression("[A-Z]{3}", ErrorMessage = "Use three capital letters.")]
    public string Code { get; set; } = "";

    [Required]
    public string Country { get; set; } = "";
}

// The create model isn't the resource, so the handler returns the new airport's key
public sealed class CreateGalleryAirportHandler(GalleryAirportStore store)
    : IResourceCreateHandler<CreateGalleryAirportModel>
{
    public Task<ResourceOperationResult> CreateAsync(
        CreateGalleryAirportModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(store.Add(new(model.Code, model.City, model.Country)));
}
