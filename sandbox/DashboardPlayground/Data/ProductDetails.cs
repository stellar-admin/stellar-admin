using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Data;

public sealed class ProductDetails
{
    [StringLength(20)]
    public string Sku { get; set; } = "";
}
