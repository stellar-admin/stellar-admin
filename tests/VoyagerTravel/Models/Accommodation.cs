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
    public int StarRating { get; set; }

    public TimeOnly CheckInTime { get; set; } = new(15, 0);

    public TimeOnly CheckOutTime { get; set; } = new(11, 0);

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public List<Amenity> Amenities { get; set; } = [];
}
