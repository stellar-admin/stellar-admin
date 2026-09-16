using System.ComponentModel.DataAnnotations;

namespace IdentitySimplePlayground.Data;

public class ProductMeta
{
    [Display(
        Name = "Allow backorders",
        Description = "Indicate whether orders can be accepted when stock runs out."
    )]
    public bool AllowBackorders { get; set; }

    [Display(Name = "Category", Description = "The category associated with this product.")]
    public Category? Category { get; set; }

    [Display(
        Name = "Category",
        Description = "Choose the category that best describes this product."
    )]
    public int? CategoryId { get; set; }

    [Range(typeof(decimal), "0", "999999999.99")]
    [Display(
        Name = "Compare-at price",
        Description = "An optional reference price to compare with the selling price."
    )]
    public decimal? CompareAtPrice { get; set; }

    [UIHint("EnumRadioChoiceCards")]
    [EnumDataType(typeof(ProductCondition))]
    [Display(Name = "Condition", Description = "Select the condition of the item being sold.")]
    public ProductCondition Condition { get; set; }

    [Range(typeof(decimal), "0", "999999999.99")]
    [Display(Name = "Cost price", Description = "The cost to purchase or produce one unit.")]
    public decimal CostPrice { get; set; }

    [Display(Name = "Created at", Description = "When this product record was created, in UTC.")]
    public DateTime CreatedAt { get; set; }

    [Range(typeof(decimal), "0", "999999")]
    [Display(
        Name = "Depth (cm)",
        Description = "The depth of one unit in centimeters; leave blank if unknown."
    )]
    public decimal? DepthCm { get; set; }

    [DataType(DataType.MultilineText)]
    [StringLength(4000)]
    [Display(Name = "Description", Description = "Describe the product and its main features.")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "999999")]
    [Display(
        Name = "Height (cm)",
        Description = "The height of one unit in centimeters; leave blank if unknown."
    )]
    public decimal? HeightCm { get; set; }

    [Display(
        Name = "Product ID",
        Description = "The identifier assigned when the product is created."
    )]
    public int Id { get; set; }

    [Display(
        Name = "Published",
        Description = "Indicate whether this product is marked as published."
    )]
    public bool IsPublished { get; set; }

    [Required]
    [StringLength(200)]
    [Display(
        Name = "Product name",
        Description = "Enter the name customers will use to identify this product."
    )]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999.99")]
    [Display(Name = "Selling price", Description = "The selling price for one unit.")]
    public decimal Price { get; set; }

    [Display(
        Name = "Published at",
        Description = "The publication date and time in UTC; leave blank if unpublished."
    )]
    public DateTime? PublishedAt { get; set; }

    [Display(
        Name = "Requires shipping",
        Description = "Indicate whether this product is a physical item that needs shipping."
    )]
    public bool RequiresShipping { get; set; } = true;

    [Required]
    [StringLength(64)]
    [Display(
        Name = "SKU",
        Description = "Enter the stock-keeping code used to identify this product."
    )]
    public string Sku { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock quantity", Description = "The number of units currently in stock.")]
    public int StockQuantity { get; set; }

    [Display(
        Name = "Track inventory",
        Description = "Indicate whether inventory should be tracked for this product."
    )]
    public bool TrackInventory { get; set; } = true;

    [Range(typeof(decimal), "0", "999999")]
    [Display(
        Name = "Weight (kg)",
        Description = "The weight of one unit in kilograms; leave blank if unknown."
    )]
    public decimal? WeightKg { get; set; }

    [Range(typeof(decimal), "0", "999999")]
    [Display(
        Name = "Width (cm)",
        Description = "The width of one unit in centimeters; leave blank if unknown."
    )]
    public decimal? WidthCm { get; set; }
}
