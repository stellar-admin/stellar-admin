using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StellarAdmin.TagHelpers;

namespace ComponentPlayground.Pages.Demo;

/// <summary>A Voyager Travel booking shown in the row-details tables.</summary>
public record RowDetailBooking(
    int Id,
    string Reference,
    string Lead,
    string Destination,
    string Departs,
    string Status,
    string Total,
    string Email,
    string Party,
    string Notes
);

/// <summary>A segment of a booking's itinerary.</summary>
public record RowDetailSegment(string Type, string Date, string Description, string Amount);

/// <summary>The rows partial's model: the bookings and the one rendered expanded.</summary>
public record RowDetailRows(IReadOnlyList<RowDetailBooking> Bookings, int? ExpandedId);

public class TableRowDetails : PageModel
{
    private static readonly RowDetailBooking[] Bookings =
    [
        new(24817, "VG-24817", "Sofia Marín", "Kyoto, Japan", "4 Nov 2026", "Ticketed", "$6,482.00", "sofia.marin@example.com", "2 adults", "Anniversary trip; ryokan room with a garden view."),
        new(24822, "VG-24822", "Liam O’Connell", "Reykjavík, Iceland", "18 Nov 2026", "Confirmed", "$3,215.50", "liam.oc@example.com", "1 adult", "Northern lights tour may move for weather."),
        new(24830, "VG-24830", "Aisha Rahman", "Cape Town, South Africa", "2 Dec 2026", "Pending", "$8,940.00", "aisha.r@example.com", "2 adults, 2 children", "Family room with an interconnecting door."),
        new(24834, "VG-24834", "Mateo Rossi", "Lisbon, Portugal", "9 Dec 2026", "Cancelled", "$1,120.00", "mateo.rossi@example.com", "1 adult", "Refunded in full within the 30-day window."),
        new(24841, "VG-24841", "Hannah Becker", "Queenstown, New Zealand", "14 Jan 2027", "Confirmed", "$11,305.00", "h.becker@example.com", "2 adults", "Self-drive; confirm driving permits."),
    ];

    private static readonly RowDetailBooking[] MoreBookings =
    [
        new(24850, "VG-24850", "Kenji Watanabe", "Hanoi, Vietnam", "20 Jan 2027", "Confirmed", "$2,760.00", "kenji.w@example.com", "1 adult", "Street-food tour on the first evening."),
        new(24856, "VG-24856", "Priya Nair", "Marrakesh, Morocco", "3 Feb 2027", "Pending", "$1,980.00", "priya.nair@example.com", "2 adults", "Riad with a rooftop terrace requested."),
    ];

    // Settings for the first table come from the query string, so each state has a URL.
    [BindProperty(SupportsGet = true)]
    public TableRowDetailEmphasis Emphasis { get; set; } = TableRowDetailEmphasis.Band;

    [BindProperty(SupportsGet = true)]
    public TableRowDetailInset Inset { get; set; } = TableRowDetailInset.Aligned;

    [BindProperty(SupportsGet = true)]
    public TableRowDetailExpandMode Mode { get; set; } = TableRowDetailExpandMode.Multiple;

    [BindProperty(SupportsGet = true)]
    public bool Animate { get; set; } = true;

    [BindProperty(SupportsGet = true)]
    public bool RowClick { get; set; }

    public RowDetailRows Rows { get; } = new(Bookings, Bookings[0].Id);

    public IReadOnlyList<RowDetailBooking> AllBookings => Bookings;

    /// <summary>Itinerary segments shown in a grid nested in a booking's details.</summary>
    public static IReadOnlyList<RowDetailSegment> SegmentsFor(RowDetailBooking booking) =>
        [
            new("Flight", booking.Departs, $"Outbound to {booking.Destination}", "$1,240.00"),
            new("Hotel", booking.Departs, $"{booking.Destination} · 4 nights", "$2,880.00"),
            new("Transfer", booking.Departs, "Airport → hotel", "$60.00"),
        ];

    public async Task<IActionResult> OnGetDetailsAsync(int id)
    {
        // Simulated latency, so the loading placeholder is visible.
        await Task.Delay(400);

        var booking = Bookings.Concat(MoreBookings).First(booking => booking.Id == id);
        return Partial("_TableRowDetailsBooking", booking);
    }

    public PartialViewResult OnGetRows() => Partial("_TableRowDetailsRows", new RowDetailRows(Bookings, null));

    public PartialViewResult OnGetMore() => Partial("_TableRowDetailsRows", new RowDetailRows(MoreBookings, null));
}
