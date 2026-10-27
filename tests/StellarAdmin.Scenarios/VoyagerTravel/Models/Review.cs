using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public class Review
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public Trip Trip { get; set; } = null!;

    public int TravellerId { get; set; }

    public Traveller Traveller { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(120)]
    public required string Title { get; set; }

    [MaxLength(4000)]
    public string? Body { get; set; }

    public DateTime SubmittedAt { get; set; }

    public bool IsApproved { get; set; }
}
