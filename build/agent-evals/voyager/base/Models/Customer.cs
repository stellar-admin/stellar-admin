using System.ComponentModel.DataAnnotations;

namespace Voyager.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [Phone]
    public string? Phone { get; set; }

    public string? Country { get; set; }

    public List<Booking> Bookings { get; set; } = [];
}
