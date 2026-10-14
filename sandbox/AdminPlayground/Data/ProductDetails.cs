using System.ComponentModel.DataAnnotations;

namespace AdminPlayground.Data;

public sealed class ProductDetails
{
    [StringLength(20)]
    public string Sku { get; set; } = "";
}
