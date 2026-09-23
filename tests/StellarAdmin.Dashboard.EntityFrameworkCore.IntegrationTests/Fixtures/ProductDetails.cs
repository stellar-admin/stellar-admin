using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class ProductDetails
{
    public string InternalNote { get; set; } = "";

    [StringLength(8)]
    public string Sku { get; set; } = "";
}
