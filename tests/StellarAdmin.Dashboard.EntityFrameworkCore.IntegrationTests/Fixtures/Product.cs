using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class Product
{
    public ProductDetails Details { get; set; } = new();

    public int Number { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Range(1, 1000)]
    public int Price { get; set; }
    public bool Hidden { get; set; }
    public int Version { get; set; }
    public int GeneratedCode { get; set; }
}
