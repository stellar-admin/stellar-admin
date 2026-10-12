using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Read-only display fields among inputs and in details sections, with every standard display.
public sealed class DisplayFieldsLayoutGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<DisplayFieldsLayoutGallery>
{
    // Editable

    [Required]
    [Display(Name = "Traveller name")]
    public string? TravellerName { get; set; }

    [Required]
    public string? Destination { get; set; }

    public DateOnly Departure { get; set; }

    // Read-only

    [Display(Name = "Booking reference")]
    public string? BookingReference { get; set; }

    [Display(Name = "Cabin")]
    public GalleryCabin Cabin { get; set; }

    // Views/Shared/DisplayTemplates/BookingStatus.cshtml chooses the badge by value
    [Display(Name = "Status")]
    [UIHint("BookingStatus")]
    public GalleryBookingStatus Status { get; set; }

    [Display(Name = "Contact email")]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Display(Name = "Created by")]
    public string? CreatedBy { get; set; }

    [Display(Name = "Created")]
    [DisplayFormat(DataFormatString = "{0:d MMM yyyy, HH:mm}")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Paid")]
    public bool IsPaid { get; set; }

    [Display(Name = "Agent notes")]
    public string? AgentNotes { get; set; }

    [Display(Name = "Last updated")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "Total")]
    public decimal Total { get; set; }

    [Display(Name = "Hotel website")]
    [Url]
    public string? HotelWebsite { get; set; }

    [Display(Name = "Hotel phone")]
    [Phone]
    public string? HotelPhone { get; set; }

    [Display(Name = "Check-in")]
    public TimeOnly CheckIn { get; set; }

    [Display(Name = "Nights")]
    public int Nights { get; set; }

    [Display(Name = "Itinerary")]
    [DataType(DataType.MultilineText)]
    public string? Itinerary { get; set; }

    // A getter-only property: display fields are never bound, so they need no setter
    [Display(Name = "Summary")]
    public string? Summary =>
        TravellerName is null
            ? null
            : $"{TravellerName} to {Destination} on {Departure:d MMM yyyy}";

    public static DisplayFieldsLayoutGallery CreateSample() =>
        new()
        {
            TravellerName = "Amelia Hart",
            Destination = "Lisbon",
            Departure = new(2027, 4, 2),
            BookingReference = "VGR-24817",
            Cabin = GalleryCabin.PremiumEconomy,
            Status = GalleryBookingStatus.Confirmed,
            ContactEmail = "amelia.hart@example.com",
            CreatedBy = "Jonah Reyes",
            CreatedAt = new(2026, 9, 14, 10, 32, 0),
            IsPaid = true,
            Total = 1840.5m,
            HotelWebsite = "https://example.com/hotel-alfama",
            HotelPhone = "+351 21 555 0142",
            CheckIn = new(15, 0),
            Nights = 6,
            Itinerary =
                "Day 1: arrive in Lisbon and check in at Hotel Alfama.\nDay 2: walking tour of Alfama and Baixa, then a tram ride to Belém.\nDay 3: day trip to Sintra and Cascais.\nDays 4 to 6: free days with optional excursions.",
        };
}

public enum GalleryBookingStatus
{
    Pending,

    Confirmed,

    Cancelled,
}
