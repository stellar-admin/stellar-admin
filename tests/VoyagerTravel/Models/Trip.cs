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

    public TripType TripType { get; set; }

    [Range(1, 60)]
    public int DurationDays { get; set; }

    public TripDifficulty Difficulty { get; set; } = TripDifficulty.Moderate;

    [Precision(10, 2)]
    public decimal BasePrice { get; set; }

    [Range(1, 40)]
    public int MaxGroupSize { get; set; } = 16;

    [MaxLength(300)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public int? LeadGuideId { get; set; }

    public Guide? LeadGuide { get; set; }

    [MaxLength(20)]
    public required string Transport { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(7)]
    public string? BrandColor { get; set; }

    public List<string> Highlights { get; set; } = [];

    public List<string> DepartureAirports { get; set; } = [];

    public Dictionary<string, int> BaggageAllowance { get; set; } = [];

    public Dictionary<string, int> GuidesByLanguage { get; set; } = [];

    public List<TripTag> Tags { get; set; } = [];

    public List<Guide> Guides { get; set; } = [];

    public List<Departure> Departures { get; set; } = [];

    public List<ItineraryDay> ItineraryDays { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
}
