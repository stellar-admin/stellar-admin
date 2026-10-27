using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public class Destination
{
    public int Id { get; set; }

    [MaxLength(120)]
    public required string Name { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public int? ParentDestinationId { get; set; }

    public Destination? ParentDestination { get; set; }

    public List<Destination> ChildDestinations { get; set; } = [];

    [MaxLength(300)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? HeroImageUrl { get; set; }

    public Climate Climate { get; set; }

    public bool IsFeatured { get; set; }

    public List<Trip> Trips { get; set; } = [];
}
