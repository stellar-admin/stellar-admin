using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.Destinations;

public sealed class Destination
{
    [Required]
    public string Country { get; set; } = "";

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    // A file name: a sample image in wwwroot/images, or an upload's name in the upload folder
    public string? Photo { get; set; }
}
