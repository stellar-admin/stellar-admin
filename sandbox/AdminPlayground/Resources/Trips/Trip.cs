using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.Trips;

public sealed class Trip
{
    [Display(Name = "Arrival airport")]
    public string? ArrivalAirport { get; set; }

    [Display(Name = "City")]
    [Required]
    public int? CityId { get; set; }

    [Display(Name = "Country")]
    [Required]
    public int? CountryId { get; set; }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Display(Name = "Neighbourhoods")]
    public List<int> NeighbourhoodIds { get; set; } = [];

    [Display(Name = "Pickup points")]
    public List<int> PickupPointIds { get; set; } = [];
}
