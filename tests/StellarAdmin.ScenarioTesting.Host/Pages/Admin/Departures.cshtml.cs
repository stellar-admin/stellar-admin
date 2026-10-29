using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VoyagerTravel.Data;
using VoyagerTravel.Models;

namespace StellarAdmin.ScenarioTesting.Host.Pages.Admin;

// The custom pages docs example, without its [Authorize], so every scenario's user can open it
public sealed class DeparturesModel(ApplicationDbContext db) : PageModel
{
    public List<Booking> Departures { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        Departures = await db
            .Bookings.Include(booking => booking.Departure)
            .Where(booking =>
                booking.Departure.StartDate >= today && booking.Departure.StartDate < today.AddDays(7)
            )
            .OrderBy(booking => booking.Departure.StartDate)
            .ToListAsync();
    }
}
