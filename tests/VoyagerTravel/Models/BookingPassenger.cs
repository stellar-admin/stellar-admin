using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class BookingPassenger
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    [MaxLength(60)]
    [Display(Name = "First name")]
    public required string FirstName { get; set; }

    [MaxLength(60)]
    [Display(Name = "Last name")]
    public required string LastName { get; set; }

    [Display(Name = "Date of birth")]
    public DateOnly DateOfBirth { get; set; }

    [MaxLength(20)]
    [Display(Name = "Passport number")]
    public string? PassportNumber { get; set; }

    [Display(Name = "Room type")]
    public RoomType RoomType { get; set; } = RoomType.Twin;

    [Precision(10, 2)]
    public decimal Price { get; set; }
}
