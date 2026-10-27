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

    [Range(1, 5)]
    public int Difficulty { get; set; } = 2;

    [Precision(10, 2)]
    public decimal BasePrice { get; set; }

    [Range(1, 40)]
    public int MaxGroupSize { get; set; } = 16;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public List<TripTag> Tags { get; set; } = [];

    public List<Guide> Guides { get; set; } = [];

    public List<Departure> Departures { get; set; } = [];

    public List<ItineraryDay> ItineraryDays { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
}
