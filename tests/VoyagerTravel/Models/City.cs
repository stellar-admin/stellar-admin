using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public class City
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public bool HasAirport { get; set; }
}
