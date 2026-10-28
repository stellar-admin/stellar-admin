using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Name), IsUnique = true)]
public class TripTag
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    public List<Trip> Trips { get; set; } = [];
}
