using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class InventoryItem(string sku)
{
    [Required]
    public string Name { get; set; } = "New item";

    public string Sku { get; set; } = sku;
}
