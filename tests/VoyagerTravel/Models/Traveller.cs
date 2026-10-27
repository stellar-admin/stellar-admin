using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Email), IsUnique = true)]
public class Traveller
{
    public int Id { get; set; }

    [MaxLength(60)]
    public required string FirstName { get; set; }

    [MaxLength(60)]
    public required string LastName { get; set; }

    [MaxLength(254)]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(30)]
    [Phone]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? NationalityId { get; set; }

    public Country? Nationality { get; set; }

    public required Address Address { get; set; }

    public LoyaltyTier LoyaltyTier { get; set; } = LoyaltyTier.Explorer;

    public bool MarketingOptIn { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Booking> Bookings { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
}
