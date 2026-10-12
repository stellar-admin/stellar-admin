using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductFactsModel
{
    public bool Active { get; set; }

    public bool? Discontinued { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public TimeOnly Opens { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public decimal Price { get; set; }

    [DisplayFormat(DataFormatString = "{0:0.000}")]
    public decimal Weight { get; set; }

    [Required]
    public string ProductName { get; set; } = "";

    public Guid Reference { get; set; }

    public DateOnly Released { get; set; }

    public ProductFactsStatus Status { get; set; }

    [UIHint("ProductCode")]
    public string? Code { get; set; }

    public DateTime Updated { get; set; }

    [Url]
    public string? Website { get; set; }
}

public enum ProductFactsStatus
{
    Draft,

    [Display(Name = "On sale")]
    OnSale,
}
