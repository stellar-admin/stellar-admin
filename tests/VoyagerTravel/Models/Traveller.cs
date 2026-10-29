using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Email), IsUnique = true)]
public class Traveller
{
    public int Id { get; set; }

    [MaxLength(60)]
    [Display(Name = "First name")]
    public required string FirstName { get; set; }

    [MaxLength(60)]
    [Display(Name = "Last name")]
    public required string LastName { get; set; }

    [MaxLength(254)]
    [EmailAddress]
    public required string Email { get; set; }

    [MaxLength(30)]
    [Phone]
    public string? Phone { get; set; }

    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Nationality")]
    public int? NationalityId { get; set; }

    public Country? Nationality { get; set; }

    public required Address Address { get; set; }

    [Display(Name = "Loyalty tier")]
    public LoyaltyTier LoyaltyTier { get; set; } = LoyaltyTier.Explorer;

    [Display(Name = "Marketing opt-in")]
    public bool MarketingOptIn { get; set; }

    [MaxLength(500)]
    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Display(Name = "Created at")]
    public DateTime CreatedAt { get; set; }

    public List<Booking> Bookings { get; set; } = [];

    public List<Review> Reviews { get; set; } = [];
}
