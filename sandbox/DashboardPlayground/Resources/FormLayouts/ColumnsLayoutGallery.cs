using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Each section is one column count scenario; its title names the configuration.
public sealed class ColumnsLayoutGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<ColumnsLayoutGallery>
{
    // Columns(3)

    [Display(Name = "Trip name")]
    public string? TripName { get; set; }

    public string? Destination { get; set; }

    public string? Departure { get; set; }

    [Display(Name = "Return")]
    public string? ReturnDate { get; set; }

    public string? Travellers { get; set; }

    public string? Budget { get; set; }

    // Columns per breakpoint

    [Display(Name = "First name")]
    public string? FirstName { get; set; }

    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    [Display(Name = "Passport number")]
    public string? PassportNumber { get; set; }

    public string? Nationality { get; set; }

    [Display(Name = "Loyalty number")]
    public string? LoyaltyNumber { get; set; }

    [Display(Name = "Seat preference")]
    public string? SeatPreference { get; set; }

    // Default(2)

    public string? Street { get; set; }

    public string? City { get; set; }

    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    // No Columns

    [Display(Name = "Emergency contact")]
    public string? EmergencyContact { get; set; }

    [Display(Name = "Emergency phone")]
    public string? EmergencyPhone { get; set; }

    public static ColumnsLayoutGallery CreateSample() =>
        new()
        {
            TripName = "Lisbon spring break",
            Destination = "Lisbon",
            Departure = "2027-04-02",
            ReturnDate = "2027-04-09",
            Travellers = "2",
            Budget = "2,400",
            FirstName = "Amelia",
            LastName = "Hart",
            Email = "amelia@voyager.travel",
            Phone = "+44 20 7946 0958",
            PassportNumber = "533380006",
            Nationality = "British",
            LoyaltyNumber = "VT-204918",
            SeatPreference = "Window",
            Street = "14 Harbour Lane",
            City = "Brighton",
            PostalCode = "BN1 1AA",
            Country = "United Kingdom",
            EmergencyContact = "Oliver Hart",
            EmergencyPhone = "+44 20 7946 0123",
        };
}
