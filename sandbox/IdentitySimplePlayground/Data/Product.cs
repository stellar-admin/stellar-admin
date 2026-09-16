using Microsoft.AspNetCore.Mvc;

namespace IdentitySimplePlayground.Data;

[ModelMetadataType<ProductMeta>]
public class Product
{
    public bool AllowBackorders { get; set; }

    public Category? Category { get; set; }

    public int? CategoryId { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public ProductCondition Condition { get; set; } = ProductCondition.New;

    public decimal CostPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal? DepthCm { get; set; }

    public string? Description { get; set; }

    public decimal? HeightCm { get; set; }

    public int Id { get; set; }

    public bool IsPublished { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool RequiresShipping { get; set; } = true;

    public string Sku { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public bool TrackInventory { get; set; } = true;

    public decimal? WeightKg { get; set; }

    public decimal? WidthCm { get; set; }
}
