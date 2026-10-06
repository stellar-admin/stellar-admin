using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.FieldEditors;

// The items of the lookup gallery
public sealed record GalleryAirport(string Code, string City, string Country);

// Searches a fixed list in memory. A real source would query a database or an API.
public sealed class GalleryAirportLookupSource : ILookupSource<GalleryAirport, string>
{
    private static readonly GalleryAirport[] Airports =
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

    public Task<GalleryAirport?> FindAsync(string value, CancellationToken cancellationToken) =>
        Task.FromResult(Airports.FirstOrDefault(airport => airport.Code == value));

    public Task<LookupPage<GalleryAirport>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = Airports
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
