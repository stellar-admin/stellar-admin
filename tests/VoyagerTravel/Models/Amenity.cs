using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Name), IsUnique = true)]
public class Amenity
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(40)]
    public required string Icon { get; set; }

    public List<Accommodation> Accommodations { get; set; } = [];
}
