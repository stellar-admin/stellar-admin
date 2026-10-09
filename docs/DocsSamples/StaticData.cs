using System.ComponentModel.DataAnnotations;

namespace DocsSamples;

public class StaticData
{
    public static Booking[] Bookings =>
        [
            new Booking(
                Id: "TRP-4821",
                Status: BookingStatus.Confirmed,
                Destination: "London, UK",
                Amount: 1249.99M
            ),
            new Booking(
                Id: "TRP-4822",
                Status: BookingStatus.Pending,
                Destination: "Kyoto, Japan",
                Amount: 2450.00M
            ),
            new Booking(
                Id: "TRP-4823",
                Status: BookingStatus.Confirmed,
                Destination: "Cancun, Mexico",
                Amount: 850.50M
            ),
            new Booking(
                Id: "TRP-4824",
                Status: BookingStatus.Confirmed,
                Destination: "Cape Town, SA",
                Amount: 1500.00M
            ),
            new Booking(
                Id: "TRP-4825",
                Status: BookingStatus.Cancelled,
                Destination: "Rome, Italy",
                Amount: 185.00M
            ),
        ];

    /// <summary>The traveller details and itinerary behind each booking in <see cref="Bookings" />.</summary>
    public static BookingDetails DetailsFor(string bookingId) =>
        bookingId switch
        {
            "TRP-4821" => new(
                "Amelia Hart",
                "amelia.hart@example.com",
                "2 adults",
                "Theatre tickets requested for the second evening.",
                [
                    new("Flight", "12 Nov", "JFK → LHR", "$640.00"),
                    new("Hotel", "12 Nov", "The Langham, 4 nights", "$560.00"),
                    new("Transfer", "16 Nov", "Hotel → Heathrow", "$49.99"),
                ]
            ),
            "TRP-4822" => new(
                "Kenji Watanabe",
                "kenji.w@example.com",
                "1 adult",
                "Ryokan room with a garden view; vegetarian meals on both flights.",
                [
                    new("Flight", "4 Dec", "SFO → KIX", "$1,180.00"),
                    new("Hotel", "5 Dec", "Hoshinoya Kyoto, 3 nights", "$1,170.00"),
                    new("Activity", "6 Dec", "Private tea ceremony, Uji", "$100.00"),
                ]
            ),
            "TRP-4823" => new(
                "Lucía Fernández",
                "lucia.f@example.com",
                "2 adults, 1 child",
                "Cot needed in the room.",
                [
                    new("Flight", "20 Dec", "MAD → CUN", "$520.00"),
                    new("Hotel", "20 Dec", "Hotel Xcaret, 5 nights", "$330.50"),
                ]
            ),
            "TRP-4824" => new(
                "Aisha Rahman",
                "aisha.r@example.com",
                "2 adults",
                "Hold the Table Mountain tickets until the weather forecast is in.",
                [
                    new("Flight", "8 Jan", "LHR → CPT", "$980.00"),
                    new("Hotel", "8 Jan", "The Silo Hotel, 3 nights", "$420.00"),
                    new("Activity", "10 Jan", "Table Mountain cableway", "$100.00"),
                ]
            ),
            _ => new(
                "Mateo Rossi",
                "mateo.rossi@example.com",
                "1 adult",
                "Cancelled by the traveller and refunded in full.",
                [new("Hotel", "3 Feb", "Hotel Artemide, 2 nights", "$185.00")]
            ),
        };
}

public record BookingDetails(
    string Traveller,
    string Email,
    string Party,
    string Notes,
    IReadOnlyList<ItinerarySegment> Itinerary
);

public record ItinerarySegment(string Type, string Date, string Description, string Amount);

public record Booking(
    string Id,
    [property: UIHint("BookingStatus")] BookingStatus Status,
    string Destination,
    decimal Amount
);

public enum BookingStatus
{
    Confirmed,
    Pending,
    Cancelled,
}

public enum CabinClass
{
    [Display(Name = "Economy")]
    Economy,

    [Display(Name = "Premium Economy")]
    PremiumEconomy,

    [Display(Name = "Business Class")]
    Business,

    [Display(Name = "First Class")]
    First,
}
