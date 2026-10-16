using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

/// <summary>A Voyager Travel tour, edited in place in the grid.</summary>
public sealed class Tour
{
    public bool Active { get; set; }

    [Required(ErrorMessage = "Choose a country.")]
    public string? CountryCode { get; set; }

    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a name.")]
    public string Name { get; set; } = "";

    public string Notes { get; set; } = "";

    [Range(1, 100000, ErrorMessage = "Enter a price from $1 to $100,000.")]
    public decimal Price { get; set; }
}

/// <summary>What the grid partial renders: the whole grid for the page, or rows for a row request.</summary>
public sealed class TourGrid
{
    public IEnumerable<int> EditingKeys { get; init; } = [];

    public List<Tour> Items { get; init; } = [];

    public Tour? NewItem { get; init; }

    public bool RowsOnly { get; init; }

    /// <summary>
    ///     Whether the editing rows came with the page, which renders them without their details
    ///     rows, so their responses bring the details row too.
    /// </summary>
    public bool WithPage { get; init; }
}

public class DataGridEditing : PageModel
{
    public static readonly IReadOnlyDictionary<string, string> Countries = new Dictionary<
        string,
        string
    >
    {
        ["IS"] = "Iceland",
        ["JP"] = "Japan",
        ["NZ"] = "New Zealand",
        ["PE"] = "Peru",
        ["PT"] = "Portugal",
    };

    private static readonly Lock StoreLock = new();
    private static readonly List<Tour> Store =
    [
        new() { Id = 1, Name = "Reykjavík Northern Lights", CountryCode = "IS", Price = 1890m, Active = true, Notes = "Departures October to March." },
        new() { Id = 2, Name = "Kyoto Temples", CountryCode = "JP", Price = 2450m, Active = true, Notes = "Includes a tea ceremony." },
        new() { Id = 3, Name = "Milford Sound Cruise", CountryCode = "NZ", Price = 980m, Active = false, Notes = "Weather dependent." },
        new() { Id = 4, Name = "Machu Picchu Trek", CountryCode = "PE", Price = 3100m, Active = true, Notes = "Permits sell out months ahead." },
    ];
    private static int _nextId = 5;

    public TourGrid Grid { get; private set; } = new();

    // ?edit=1&edit=3 renders rows 1 and 3 in edit mode with the page.
    public void OnGet(int[] edit) => Grid = new() { Items = List(), EditingKeys = edit };

    // Only for a row the page rendered in edit mode: a row opened with Edit is put back in the browser.
    public IActionResult OnGetCancel(int id) => Rows(new() { Items = [Find(id)] });

    public IActionResult OnGetEdit(int id) => Rows(new() { Items = [Find(id)], EditingKeys = [id] });

    public IActionResult OnGetNew() => Rows(new() { NewItem = new() });

    public async Task<IActionResult> OnPostSaveAsync(int? id, bool withPage)
    {
        // An editing row's fields are named rows[key].Field, with the key "new" for a new row.
        var tour = new Tour();
        var key = id?.ToString(CultureInfo.InvariantCulture) ?? "new";
        if (!await TryUpdateModelAsync(tour, $"rows[{key}]"))
        {
            // The editing row again, with the posted values and the errors.
            if (id is null)
            {
                return Rows(new() { NewItem = tour, WithPage = withPage });
            }

            tour.Id = id.Value;

            return Rows(new() { Items = [tour], EditingKeys = [id.Value], WithPage = withPage });
        }

        lock (StoreLock)
        {
            if (id is null)
            {
                tour.Id = _nextId++;
                Store.Add(tour);
            }
            else
            {
                var stored = Store.Find(item => item.Id == id)!;
                stored.Name = tour.Name;
                stored.CountryCode = tour.CountryCode;
                stored.Price = tour.Price;
                stored.Active = tour.Active;
                tour = stored;
            }
        }

        return Rows(new() { Items = [tour] });
    }

    private static Tour Find(int id)
    {
        lock (StoreLock)
        {
            return Store.Find(item => item.Id == id)!;
        }
    }

    private static List<Tour> List()
    {
        lock (StoreLock)
        {
            return [.. Store];
        }
    }

    // A row request renders the same grid partial with rows-only, so its rows match the page's.
    private PartialViewResult Rows(TourGrid grid) =>
        Partial(
            "_DataGridEditingGrid",
            new TourGrid
            {
                Items = grid.Items,
                EditingKeys = grid.EditingKeys,
                NewItem = grid.NewItem,
                RowsOnly = true,
                WithPage = grid.WithPage,
            }
        );
}
