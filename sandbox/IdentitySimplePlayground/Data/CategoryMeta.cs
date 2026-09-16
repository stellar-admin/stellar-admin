using System.ComponentModel.DataAnnotations;

namespace IdentitySimplePlayground.Data;

public class CategoryMeta
{
    [DataType(DataType.MultilineText)]
    [StringLength(500)]
    [Display(
        Name = "Description",
        Description = "Explain what kinds of products belong in this category."
    )]
    public string? Description { get; set; }

    [Display(
        Name = "Category ID",
        Description = "The identifier assigned when the category is created."
    )]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(
        Name = "Category name",
        Description = "Enter a short, recognizable name for this category."
    )]
    public string Name { get; set; } = string.Empty;
}
