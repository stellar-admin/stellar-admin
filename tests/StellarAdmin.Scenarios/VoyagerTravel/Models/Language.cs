using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VoyagerTravel.Models;

[Index(nameof(Code), IsUnique = true)]
public class Language
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(5)]
    public required string Code { get; set; }

    public List<Guide> Guides { get; set; } = [];
}
