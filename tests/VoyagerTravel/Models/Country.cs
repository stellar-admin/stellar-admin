using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(IsoCode), IsUnique = true)]
public class Country
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(2)]
    public required string IsoCode { get; set; }

    public Continent Continent { get; set; }

    public List<Destination> Destinations { get; set; } = [];
}
