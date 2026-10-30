using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Widgets;

namespace AdminPlayground.Resources.Products;

// The index's scope and search, so the summary covers the products the grid lists
public sealed record ProductPriceSummaryParameters(string? Scope, string? Search);

public sealed record ProductPriceSummary(
    int Products,
    decimal Lowest,
    decimal Average,
    decimal Highest
);

// Summarizes the prices of the products the index lists
public sealed class ProductPriceSummaryHandler(ApplicationDbContext db)
    : IWidgetHandler<ProductPriceSummaryParameters, ProductPriceSummary>
{
    public async Task<ProductPriceSummary?> LoadAsync(
        ProductPriceSummaryParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    )
    {
        var products = parameters.Scope switch
        {
            "under-50" => db.Products.Where(product => product.Price < 50),
            "50-and-over" => db.Products.Where(product => product.Price >= 50),
            _ => db.Products,
        };
        if (parameters.Search is { } search)
        {
            products = products.Where(product => product.Name.Contains(search));
        }

        // SQLite can't aggregate decimals, so the prices come back to be summarized here
        var prices = await products.Select(product => product.Price).ToListAsync(cancellationToken);

        return prices.Count == 0
            ? new(0, 0, 0, 0)
            : new(prices.Count, prices.Min(), prices.Average(), prices.Max());
    }
}
