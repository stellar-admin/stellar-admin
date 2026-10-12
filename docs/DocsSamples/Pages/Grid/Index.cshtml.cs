using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Grid;

public class Index : PageModel
{
    public static TravellerBooking SampleBooking =>
        new()
        {
            Reference = "TRP-4822",
            Status = "Confirmed",
            FirstName = "Kenji",
            LastName = "Watanabe",
            Email = "kenji.watanabe@example.com",
            Phone = "+81 75 555 0142",
            Departure = new DateOnly(2026, 11, 14),
            Return = new DateOnly(2026, 11, 21),
            Cabin = "premium-economy",
            Notes = "Theatre tickets requested for the second evening.",
            SendConfirmation = true,
            CreatedAt = new DateTime(2026, 10, 2, 9, 14, 0),
            CreatedBy = "Amelia Hart",
            UpdatedAt = new DateTime(2026, 10, 11, 16, 40, 0),
            UpdatedBy = "System",
            Total = 2450.00M,
            Paid = 1000.00M,
        };

    public TravellerBooking Booking { get; set; } = SampleBooking;

    public void OnGet() { }
}

public class TravellerBooking
{
    [Display(Name = "Balance"), DisplayFormat(DataFormatString = "€{0:N2}")]
    public decimal Balance => Total - Paid;

    public string Cabin { get; set; } = "";

    [Display(Name = "Created"), DisplayFormat(DataFormatString = "{0:d MMM yyyy, HH:mm}")]
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = "";

    public DateOnly Departure { get; set; }

    [EmailAddress]
    public string Email { get; set; } = "";

    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Display(Name = "Agent notes"), DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [DisplayFormat(DataFormatString = "€{0:N2}")]
    public decimal Paid { get; set; }

    [Phone]
    public string Phone { get; set; } = "";

    [Display(Name = "Booking")]
    public string Reference { get; set; } = "";

    public DateOnly Return { get; set; }

    [Display(Name = "Send confirmation", Description = "Email the itinerary when the booking is saved.")]
    public bool SendConfirmation { get; set; }

    public string Status { get; set; } = "";

    [DisplayFormat(DataFormatString = "€{0:N2}")]
    public decimal Total { get; set; }

    [Display(Name = "Last updated"), DisplayFormat(DataFormatString = "{0:d MMM yyyy, HH:mm}")]
    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = "";
}
