using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public class Destination
{
    public int Id { get; set; }

    [MaxLength(120)]
    public required string Name { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    [Display(Name = "Parent destination")]
    public int? ParentDestinationId { get; set; }

    [Display(Name = "Parent destination")]
    public Destination? ParentDestination { get; set; }

    [Display(Name = "Child destinations")]
    public List<Destination> ChildDestinations { get; set; } = [];

    public int? CityId { get; set; }

    public City? City { get; set; }

    [Display(Name = "Requires airport")]
    public bool RequiresAirport { get; set; }

    [MaxLength(300)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    [Display(Name = "Hero image URL")]
    public string? HeroImageUrl { get; set; }

    public Climate Climate { get; set; }

    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; }

    public List<Trip> Trips { get; set; } = [];
}
