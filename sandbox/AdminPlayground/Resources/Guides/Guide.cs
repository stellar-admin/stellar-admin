using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Resources.Guides;

public sealed class Guide
{
    // HTML from the rich text editor; [DataType(Html)] also makes it the field's default editor and display
    [DataType(DataType.Html)]
    public string? Bio { get; set; }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";
}
