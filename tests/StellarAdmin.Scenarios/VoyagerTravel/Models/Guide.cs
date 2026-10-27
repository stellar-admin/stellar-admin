using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Email), IsUnique = true)]
public class Guide
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

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public string? Bio { get; set; }

    public DateOnly HiredOn { get; set; }

    public bool IsActive { get; set; } = true;

    public List<Language> Languages { get; set; } = [];

    public List<Trip> Trips { get; set; } = [];

    public List<Departure> LedDepartures { get; set; } = [];
}
