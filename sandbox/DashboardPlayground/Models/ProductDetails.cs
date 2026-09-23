using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Models;

public sealed class ProductDetails
{
    [StringLength(20)]
    public string Sku { get; set; } = "";
}
