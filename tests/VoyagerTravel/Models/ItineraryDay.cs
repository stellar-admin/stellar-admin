using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(TripId), nameof(DayNumber), IsUnique = true)]
public class ItineraryDay
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public Trip Trip { get; set; } = null!;

    [Range(1, 60)]
    public int DayNumber { get; set; }

    [MaxLength(120)]
    public required string Title { get; set; }

    public string? Description { get; set; }

    public int? AccommodationId { get; set; }

    public Accommodation? Accommodation { get; set; }

    public Meals Meals { get; set; } = Meals.None;
}
