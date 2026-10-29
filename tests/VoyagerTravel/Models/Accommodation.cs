using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public class Accommodation
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public int DestinationId { get; set; }

    public Destination Destination { get; set; } = null!;

    [MaxLength(150)]
    public required string Name { get; set; }

    [Range(1, 5)]
    [Display(Name = "Star rating")]
    public int StarRating { get; set; }

    [Display(Name = "Check-in time")]
    public TimeOnly CheckInTime { get; set; } = new(15, 0);

    [Display(Name = "Check-out time")]
    public TimeOnly CheckOutTime { get; set; } = new(11, 0);

    [MaxLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    public List<Amenity> Amenities { get; set; } = [];
}
