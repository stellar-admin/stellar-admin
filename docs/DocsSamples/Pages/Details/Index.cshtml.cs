using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages.Details;

public class Index : PageModel
{
    public static TripBooking SampleBooking =>
        new()
        {
            Reference = "TRP-4822",
            Status = BookingStatus.Confirmed,
            LeadTraveller = "Kenji Watanabe",
            Email = "kenji.watanabe@example.com",
            DestinationId = 12,
            Destination = "Kyoto, Japan",
            Departure = new DateOnly(2026, 11, 14),
            Return = new DateOnly(2026, 11, 21),
            Party = "2 adults",
            Cabin = CabinClass.PremiumEconomy,
            Total = 2450.00M,
            Infants = 0,
            TravelInsurance = true,
            VisaRequired = false,
            ItineraryPage = "https://voyager.example/trips/kyoto",
            Interests = ["Culture", "Food", "Rail pass"],
            Notes =
                "Theatre tickets requested for the second evening.\nVegetarian meals for both travellers.",
            CreatedAt = new DateTime(2026, 10, 2, 9, 14, 0),
            CreatedBy = "Amelia Hart",
            UpdatedAt = new DateTime(2026, 10, 11, 16, 40, 0),
            UpdatedBy = "System",
            Version = 7,
        };

    public TripBooking Booking { get; set; } = SampleBooking;

    public void OnGet() { }
}

public class TripBooking
{
    [Display(Name = "Archived"), DisplayFormat(NullDisplayText = "Never")]
    public DateTime? ArchivedAt { get; set; }

    [Display(Name = "Cabin")]
    public CabinClass Cabin { get; set; }

    [Display(Name = "Created"), DisplayFormat(DataFormatString = "{0:d MMM yyyy, HH:mm}")]
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = "";

    [DisplayFormat(DataFormatString = "{0:d MMM yyyy}")]
    public DateOnly Departure { get; set; }

    public string Destination { get; set; } = "";

    public int DestinationId { get; set; }

    [Display(Name = "Dietary needs"), DisplayFormat(NullDisplayText = "None")]
    public string? DietaryNeeds { get; set; }

    [EmailAddress]
    public string Email { get; set; } = "";

    public int Infants { get; set; }

    public IReadOnlyList<string> Interests { get; set; } = [];

    [Display(Name = "Itinerary page"), Url]
    public string ItineraryPage { get; set; } = "";

    [Display(Name = "Lead traveller")]
    public string LeadTraveller { get; set; } = "";

    [Display(Name = "Agent notes"), DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public string Party { get; set; } = "";

    [Display(Name = "Promo code")]
    public string? PromoCode { get; set; }

    [Display(Name = "Booking")]
    public string Reference { get; set; } = "";

    [DisplayFormat(DataFormatString = "{0:d MMM yyyy}")]
    public DateOnly Return { get; set; }

    [UIHint("BookingStatus")]
    public BookingStatus Status { get; set; }

    [DisplayFormat(DataFormatString = "€{0:N2}")]
    public decimal Total { get; set; }

    [Display(Name = "Travel insurance")]
    public bool TravelInsurance { get; set; }

    [Display(Name = "Last updated"), DisplayFormat(DataFormatString = "{0:d MMM yyyy, HH:mm}")]
    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = "";

    public int Version { get; set; }

    [Display(Name = "Visa required")]
    public bool VisaRequired { get; set; }
}
