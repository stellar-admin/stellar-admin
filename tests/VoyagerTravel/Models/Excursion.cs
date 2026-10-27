using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class Excursion
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public int DestinationId { get; set; }

    public Destination Destination { get; set; } = null!;

    [MaxLength(120)]
    public required string Name { get; set; }

    [Precision(4, 1)]
    public decimal DurationHours { get; set; }

    [Precision(10, 2)]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
