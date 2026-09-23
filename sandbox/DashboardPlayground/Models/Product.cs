using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Models;

public sealed class Product
{
    public Category? Category { get; set; }

    public int? CategoryId { get; set; }

    public ProductDetails Details { get; set; } = new();

    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }
}
