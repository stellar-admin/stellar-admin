using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using VoyagerTravel.Data;

namespace VoyagerTravel.Models;

[Index(nameof(Reference), IsUnique = true)]
public class Booking
{
    public int Id { get; set; }

    [MaxLength(10)]
    public required string Reference { get; set; }

    public int TravellerId { get; set; }

    public Traveller Traveller { get; set; } = null!;

    public int DepartureId { get; set; }

    public Departure Departure { get; set; } = null!;

    [Display(Name = "Booked at")]
    public DateTime BookedAt { get; set; }

    [Display(Name = "Updated at")]
    public DateTime? UpdatedAt { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Precision(10, 2)]
    [Display(Name = "Total amount")]
    public decimal TotalAmount { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Special requests")]
    public string? SpecialRequests { get; set; }

    [MaxLength(450)]
    [Display(Name = "Assigned agent")]
    public string? AssignedAgentId { get; set; }

    [Display(Name = "Assigned agent")]
    public ApplicationUser? AssignedAgent { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public List<BookingPassenger> Passengers { get; set; } = [];

    public List<Payment> Payments { get; set; } = [];

    [Display(Name = "Add-ons")]
    public List<BookingAddOn> AddOns { get; set; } = [];
}
