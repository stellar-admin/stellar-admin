using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Card sections in a multi-column form, with groups inside them.
public sealed class SectionsLayoutGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SectionsLayoutGallery>
{
    // Traveller

    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    // Loyalty

    [Display(Name = "Programme")]
    public string? LoyaltyProgramme { get; set; }

    [Display(Name = "Member number")]
    public string? LoyaltyNumber { get; set; }

    // Itinerary

    [Display(Name = "Origin · stacked group")]
    public string? Origin { get; set; }

    [Display(Name = "Destination · stacked group")]
    public string? Destination { get; set; }

    [Display(Name = "Departure · two-column group")]
    public string? Departure { get; set; }

    [Display(Name = "Return · two-column group")]
    public string? ReturnDate { get; set; }

    [Display(Name = "Travellers")]
    public string? Travellers { get; set; }

    // Preferences

    [Display(Name = "Seat · inner stacked group")]
    public string? Seat { get; set; }

    [Display(Name = "Meal · inner stacked group")]
    public string? Meal { get; set; }

    [Display(Name = "Notes · beside the inner group")]
    public string? Notes { get; set; }

    // Emergency contact

    [Display(Name = "Name")]
    public string? EmergencyContact { get; set; }

    [Display(Name = "Phone")]
    public string? EmergencyPhone { get; set; }

    public static SectionsLayoutGallery CreateSample() =>
        new()
        {
            FirstName = "Amelia",
            LastName = "Hart",
            Email = "amelia@voyager.travel",
            Phone = "+44 20 7946 0958",
            LoyaltyProgramme = "Voyager Miles",
            LoyaltyNumber = "VT-204918",
            Origin = "London",
            Destination = "Lisbon",
            Departure = "2027-04-02",
            ReturnDate = "2027-04-09",
            Travellers = "2",
            Seat = "Window",
            Meal = "Vegetarian",
            Notes = "Travelling with a folding bicycle.",
            EmergencyContact = "Oliver Hart",
            EmergencyPhone = "+44 20 7946 0123",
        };
}
