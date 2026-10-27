using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Owned]
public class Address
{
    [MaxLength(120)]
    public required string Line1 { get; set; }

    [MaxLength(120)]
    public string? Line2 { get; set; }

    [MaxLength(80)]
    public required string City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(2)]
    public required string CountryCode { get; set; }
}
