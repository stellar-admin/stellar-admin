using DashboardPlayground.Models;

namespace DashboardPlayground.Data;

internal static class ProductSeed
{
    public static Product[] Create() =>
        [
            new()
            {
                Id = 1,
                Name = "Notebook",
                Price = 8.50m,
                Details = new() { Sku = "NB-001" },
            },
            new()
            {
                Id = 2,
                Name = "Desk lamp",
                Price = 34.95m,
                Details = new() { Sku = "DL-002" },
            },
            new()
            {
                Id = 3,
                Name = "Travel mug",
                Price = 18.00m,
                Details = new() { Sku = "TM-003" },
            },
            .. Enumerable
                .Range(4, 34)
                .Select(id => new Product
                {
                    Id = id,
                    Name = $"Sample product {id:00}",
                    Price = id * 2.5m,
                    Details = new() { Sku = $"S-{id:000}" },
                }),
        ];
}
