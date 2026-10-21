using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class Product
{
    public Category? Category { get; set; }

    public int? CategoryId { get; set; }

    public ProductDetails Details { get; set; } = new();

    public int Number { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [MaxLength(2)]
    [NotReservedTags]
    public List<Tag> Tags { get; } = [];

    [Range(1, 1000)]
    public int Price { get; set; }
    public bool Hidden { get; set; }
    public int Version { get; set; }
    public int GeneratedCode { get; set; }
}
