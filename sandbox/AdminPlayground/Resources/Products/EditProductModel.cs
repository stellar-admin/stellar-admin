using System.ComponentModel.DataAnnotations;
using AdminPlayground.Data;

namespace AdminPlayground.Resources.Products;

// The product edit form's model: the product, and a reason for the change that isn't stored on it
public sealed class EditProductModel
{
    public Product Product { get; set; } = null!;

    [Required, StringLength(500)]
    [Display(Name = "Reason for the change")]
    public string Reason { get; set; } = "";
}
