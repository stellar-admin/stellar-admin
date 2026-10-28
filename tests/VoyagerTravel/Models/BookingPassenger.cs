using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

public class BookingPassenger
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    [MaxLength(60)]
    public required string FirstName { get; set; }

    [MaxLength(60)]
    public required string LastName { get; set; }

    public DateOnly DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? PassportNumber { get; set; }

    public RoomType RoomType { get; set; } = RoomType.Twin;

    [Precision(10, 2)]
    public decimal Price { get; set; }
}
