using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Email), IsUnique = true)]
public class Guide
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

    [MaxLength(500)]
    [Display(Name = "Photo URL")]
    public string? PhotoUrl { get; set; }

    public string? Bio { get; set; }

    [Display(Name = "Hired on")]
    public DateOnly HiredOn { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(3)]
    [Display(Name = "Home airport")]
    public string? HomeAirport { get; set; }

    public int? CountryId { get; set; }

    public Country? Country { get; set; }

    [MaxLength(4)]
    [Display(Name = "Badge code")]
    public string? BadgeCode { get; set; }

    public List<Language> Languages { get; set; } = [];

    public List<Trip> Trips { get; set; } = [];

    [Display(Name = "Led departures")]
    public List<Departure> LedDepartures { get; set; } = [];

    [Display(Name = "Led trips")]
    public List<Trip> LedTrips { get; set; } = [];
}
