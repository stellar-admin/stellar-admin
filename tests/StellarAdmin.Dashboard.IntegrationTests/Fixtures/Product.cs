using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class Product
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Product name")]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }

    public Product() { }

    public Product(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}
