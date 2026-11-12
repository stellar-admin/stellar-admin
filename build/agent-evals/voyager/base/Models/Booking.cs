using System.ComponentModel.DataAnnotations;

namespace Voyager.Models;

public class Booking
{
    public int Id { get; set; }

    [Required, StringLength(12)]
    public string Reference { get; set; } = "";

    [Required]
    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Required, StringLength(100)]
    public string Destination { get; set; } = "";

    [DataType(DataType.Date)]
    public DateOnly DepartureDate { get; set; }

    public BookingStatus Status { get; set; }

    public CabinClass Cabin { get; set; }

    [Range(1, 12)]
    public int Travellers { get; set; } = 1;

    public decimal Total { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
}

public enum CabinClass
{
    Economy,
    [Display(Name = "Premium economy")]
    PremiumEconomy,
    Business,
    First,
}
