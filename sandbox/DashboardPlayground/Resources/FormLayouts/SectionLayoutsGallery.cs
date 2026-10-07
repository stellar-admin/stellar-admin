using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Sections that keep or override the form's Split layout, and a lookup whose create form opens in the sheet.
public sealed class SectionLayoutsGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SectionLayoutsGallery>
{
    // Form layout

    [Display(Name = "Trip name")]
    public string? TripName { get; set; }

    public string? Budget { get; set; }

    // Stacked

    public string? Airline { get; set; }

    [Display(Name = "Flight number")]
    public string? FlightNumber { get; set; }

    // Card

    public string? Hotel { get; set; }

    [Display(Name = "Room type")]
    public string? RoomType { get; set; }

    // Create sheet

    public string? Destination { get; set; }

    public static SectionLayoutsGallery CreateSample() =>
        new()
        {
            TripName = "Lisbon spring break",
            Budget = "2,400",
            Airline = "Voyager Air",
            FlightNumber = "VA 214",
            Hotel = "Casa do Rio Boutique Hotel",
            RoomType = "Deluxe double",
            Destination = "LIS",
        };
}
