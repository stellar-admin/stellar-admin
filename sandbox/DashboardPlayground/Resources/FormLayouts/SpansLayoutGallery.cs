using System.ComponentModel.DataAnnotations;
using DashboardPlayground.Resources.FieldEditors;

namespace DashboardPlayground.Resources.FormLayouts;

// Each section is one column span scenario; each field's title names its span.
public sealed class SpansLayoutGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<SpansLayoutGallery>
{
    // Spans in a four-column grid

    [Display(Name = "Airline · span 1")]
    public string? Airline { get; set; }

    [Display(Name = "Flight · span 1")]
    public string? FlightNumber { get; set; }

    [Display(Name = "Route · span 2")]
    public string? Route { get; set; }

    [Display(Name = "Terminal · span 3")]
    public string? Terminal { get; set; }

    [Display(Name = "Gate · span 1")]
    public string? Gate { get; set; }

    [Display(Name = "Remarks · full")]
    public string? Remarks { get; set; }

    [Display(Name = "Departs · span 2")]
    public string? Departs { get; set; }

    [Display(Name = "Arrives · span 2")]
    public string? Arrives { get; set; }

    // Spans per breakpoint

    [Display(Name = "Hotel · full from Small, 2 from Medium")]
    public string? Hotel { get; set; }

    [Display(Name = "Check-in · span 1")]
    public string? CheckIn { get; set; }

    [Display(Name = "Check-out · span 1")]
    public string? CheckOut { get; set; }

    [Display(Name = "Room type · 2 from Small, 3 from Medium")]
    public string? RoomType { get; set; }

    [Display(Name = "Guests · span 1")]
    public string? Guests { get; set; }

    [Display(Name = "Requests · full from Small, 2 from Medium, full from Large")]
    public string? SpecialRequests { get; set; }

    // Spans larger than the grid

    [Display(Name = "Car rental · span 4 in 2 columns")]
    public string? CarRental { get; set; }

    [Display(Name = "Pick-up · span 1")]
    public string? PickUp { get; set; }

    [Display(Name = "Drop-off · span 1")]
    public string? DropOff { get; set; }

    [Display(Name = "Insurance · span 12 in 2 columns")]
    public string? Insurance { get; set; }

    // Fewer items than columns

    [Display(Name = "Visa · span 1")]
    public string? Visa { get; set; }

    [Display(Name = "Visa expiry · span 1")]
    public string? VisaExpiry { get; set; }

    public static SpansLayoutGallery CreateSample() =>
        new()
        {
            Airline = "Voyager Air",
            FlightNumber = "VA 214",
            Route = "London Heathrow to Lisbon",
            Terminal = "Terminal 2, Queen's Terminal",
            Gate = "B32",
            Remarks = "Priority boarding for families travelling with young children.",
            Departs = "2027-04-02 08:15",
            Arrives = "2027-04-02 11:05",
            Hotel = "Casa do Rio Boutique Hotel",
            CheckIn = "2027-04-02",
            CheckOut = "2027-04-09",
            RoomType = "Deluxe double with river view",
            Guests = "2",
            SpecialRequests = "Late check-in, around 22:00.",
            CarRental = "Compact, automatic",
            PickUp = "Lisbon airport",
            DropOff = "Lisbon airport",
            Insurance = "Full cover with zero excess",
            Visa = "Not required",
            VisaExpiry = "",
        };
}
