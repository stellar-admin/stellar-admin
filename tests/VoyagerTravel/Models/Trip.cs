using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Code), IsUnique = true)]
public class Trip
{
    public int Id { get; set; }

    [MaxLength(12)]
    public required string Code { get; set; }

    [MaxLength(150)]
    public required string Title { get; set; }

    public int DestinationId { get; set; }

    public Destination Destination { get; set; } = null!;

    [Display(Name = "Trip type")]
    public TripType TripType { get; set; }

    [Range(1, 60)]
    [Display(Name = "Duration (days)")]
    public int DurationDays { get; set; }

    public TripDifficulty Difficulty { get; set; } = TripDifficulty.Moderate;

    [Precision(10, 2)]
    [Display(Name = "Base price")]
    public decimal BasePrice { get; set; }

    [Range(1, 40)]
    [Display(Name = "Max group size")]
    public int MaxGroupSize { get; set; } = 16;

    [MaxLength(300)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    [Display(Name = "Cover image URL")]
    public string? CoverImageUrl { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Lead guide")]
    public int? LeadGuideId { get; set; }

    [Display(Name = "Lead guide")]
    public Guide? LeadGuide { get; set; }

    [MaxLength(20)]
    public required string Transport { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(7)]
    [Display(Name = "Brand color")]
    public string? BrandColor { get; set; }

    public List<string> Highlights { get; set; } = [];

    [Display(Name = "Departure airports")]
    public List<string> DepartureAirports { get; set; } = [];

    [Display(Name = "Baggage allowance")]
    public Dictionary<string, int> BaggageAllowance { get; set; } = [];

    [Display(Name = "Guides by language")]
    public Dictionary<string, int> GuidesByLanguage { get; set; } = [];

    public List<TripTag> Tags { get; set; } = [];

    public List<Guide> Guides { get; set; } = [];

    public List<Departure> Departures { get; set; } = [];

    [Display(Name = "Itinerary days")]
    public List<ItineraryDay> ItineraryDays { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
}
