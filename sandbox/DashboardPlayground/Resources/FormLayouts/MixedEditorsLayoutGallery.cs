using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Different editors side by side, with descriptions and validation, so uneven heights and error states show in columns.
public sealed class MixedEditorsLayoutGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<MixedEditorsLayoutGallery>
{
    // Editors in three columns

    [Required]
    [Display(Name = "Traveller name")]
    public string? TravellerName { get; set; }

    [Display(Description = "Select uses the enum's values.")]
    public GalleryCabin Cabin { get; set; }

    [Required]
    public string? Destination { get; set; }

    public DateOnly Departure { get; set; }

    [Range(0, 4)]
    [Display(Name = "Checked bags", Description = "Up to four.")]
    public int CheckedBags { get; set; }

    [Display(Name = "Seat cabin")]
    public GalleryCabin SeatCabin { get; set; }

    [Display(Name = "Notes · span 2")]
    public string? Notes { get; set; }

    [Display(Name = "Travel insurance")]
    public bool Insurance { get; set; }

    // Horizontal fields in two columns

    [Display(Name = "Email updates", Description = "Flight changes and gate announcements.")]
    public bool EmailUpdates { get; set; }

    [Display(Name = "Text messages")]
    public bool TextMessages { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Accept the terms to continue.")]
    [Display(Name = "Accept the terms", Description = "Required to book.")]
    public bool AcceptTerms { get; set; }

    [Display(Name = "Share with partners")]
    public bool SharePartners { get; set; }

    public static MixedEditorsLayoutGallery CreateSample() =>
        new()
        {
            TravellerName = "Amelia Hart",
            Cabin = GalleryCabin.Business,
            Destination = "LIS",
            Departure = new(2027, 4, 2),
            CheckedBags = 1,
            SeatCabin = GalleryCabin.Economy,
            Notes = "Travelling with a folding bicycle; it is boxed and under 23 kg.",
            Insurance = true,
            EmailUpdates = true,
            TextMessages = false,
            AcceptTerms = true,
            SharePartners = false,
        };
}
