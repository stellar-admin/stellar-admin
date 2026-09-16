using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace DataGridSpike.Pages;

public class UrlsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public GridState Bookings { get; set; } = new GridState();

    [BindProperty(SupportsGet = true)]
    public GridState Users { get; set; } = new GridState();

    public void OnGet() { }

    /// <summary>What the grid would do: current query merged into route values, own params overridden.</summary>
    public string GridUrl(string gridName, int pageNo, string? sortBy = null)
    {
        var values = new RouteValueDictionary();
        foreach (var parameter in Request.Query)
        {
            values[parameter.Key] =
                parameter.Value.Count == 1 ? parameter.Value[0] : parameter.Value.ToArray();
        }

        values[$"{gridName}.PageNo"] = pageNo;
        if (sortBy is not null)
        {
            values[$"{gridName}.SortBy"] = sortBy;
        }

        return Url.RouteUrl(values) ?? "(null)";
    }

    public class GridState
    {
        public int PageNo { get; set; } = 1;

        public string? SortBy { get; set; }

        public string? SortDir { get; set; }
    }
}
