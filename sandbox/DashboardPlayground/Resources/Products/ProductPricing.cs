namespace DashboardPlayground.Resources.Products;

// The Products index row details, projected from the product when its row expands
public sealed record ProductPricing(string Name, string Sku, string? Category, decimal Price);
