using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class EditProductModel(decimal price)
{
    public int Id { get; set; }

    public decimal Price { get; set; } = price;

    [Required]
    [Display(Name = "Product name")]
    public string ProductName { get; set; } = "";
}
