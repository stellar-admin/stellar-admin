using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

/// <summary>A destination the trip search can return.</summary>
public record CommandDestination(string Slug, string City, string Country);

/// <summary>A booking the trip search can return.</summary>
public record CommandBooking(string Reference, string Traveller, string Trip);

/// <summary>The server-filtered results rendered into the command list.</summary>
public record CommandResults(
    string Query,
    IReadOnlyList<CommandDestination> Destinations,
    IReadOnlyList<CommandBooking> Bookings
);

public class Command : PageModel
{
    private static readonly CommandDestination[] Destinations =
    [
        new("lisbon", "Lisbon", "Portugal"),
        new("kyoto", "Kyoto", "Japan"),
        new("cape-town", "Cape Town", "South Africa"),
        new("reykjavik", "Reykjavík", "Iceland"),
        new("buenos-aires", "Buenos Aires", "Argentina"),
        new("marrakesh", "Marrakesh", "Morocco"),
        new("vancouver", "Vancouver", "Canada"),
        new("hanoi", "Hanoi", "Vietnam"),
        new("queenstown", "Queenstown", "New Zealand"),
        new("dubrovnik", "Dubrovnik", "Croatia"),
    ];

    private static readonly CommandBooking[] Bookings =
    [
        new("VY-2041", "Amara Okafor", "Lisbon city break"),
        new("VY-2057", "Tomás Reyes", "Kyoto in autumn"),
        new("VY-2063", "Priya Nair", "Cape Town and the Winelands"),
        new("VY-2078", "Lena Fischer", "Northern lights in Reykjavík"),
        new("VY-2090", "Kenji Watanabe", "Queenstown adventure week"),
    ];

    public CommandResults Results { get; private set; } = null!;

    public void OnGet()
    {
        Results = Search(string.Empty);
    }

    public async Task<IActionResult> OnGetSearchAsync(string? q)
    {
        // Simulated latency, so overlapping requests and slow responses can be exercised.
        await Task.Delay(250);

        return Partial("_CommandResults", Search(q?.Trim() ?? string.Empty));
    }

    private static CommandResults Search(string query)
    {
        if (query.Length == 0)
        {
            return new CommandResults(query, Destinations[..4], []);
        }

        bool Matches(params string[] fields) =>
            fields.Any(field => field.Contains(query, StringComparison.CurrentCultureIgnoreCase));

        return new CommandResults(
            query,
            Destinations.Where(d => Matches(d.City, d.Country)).ToList(),
            Bookings.Where(b => Matches(b.Reference, b.Traveller, b.Trip)).ToList()
        );
    }
}
