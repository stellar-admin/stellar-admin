using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.Amenities;

public sealed class Amenity
{
    // An icon's name, such as "wifi"
    public string? Icon { get; set; }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
}
