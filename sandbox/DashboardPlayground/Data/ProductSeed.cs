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
            },
            new()
            {
                Id = 2,
                Name = "Desk lamp",
                Price = 34.95m,
            },
            new()
            {
                Id = 3,
                Name = "Travel mug",
                Price = 18.00m,
            },
            .. Enumerable
                .Range(4, 34)
                .Select(id => new Product
                {
                    Id = id,
                    Name = $"Sample product {id:00}",
                    Price = id * 2.5m,
                }),
        ];
}
