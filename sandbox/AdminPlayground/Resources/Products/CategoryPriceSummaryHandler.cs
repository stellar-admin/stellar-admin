using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Products;

public sealed record CategoryPriceSummary(
    string Category,
    int Products,
    decimal Lowest,
    decimal Average,
    decimal Highest
);

// Summarizes the prices of the products the scope and search select, per category. The query already has the scope and
// search applied, and reads each product's category
public sealed class CategoryPriceSummaryHandler
    : IEfCoreLayoutHandler<Product, CategoryPriceSummary>
{
    public async Task<ResourceListResult<CategoryPriceSummary>> ListAsync(
        IQueryable<Product> entities,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        // SQLite can't aggregate decimals, so the prices come back to be summarized here
        var prices = await entities
            .Select(product => new
            {
                Category = product.Category != null ? product.Category.Name : null,
                product.Price,
            })
            .ToListAsync(cancellationToken);
        var items = prices
            .GroupBy(product => product.Category ?? "No category")
            .Select(category => new CategoryPriceSummary(
                category.Key,
                category.Count(),
                category.Min(product => product.Price),
                category.Average(product => product.Price),
                category.Max(product => product.Price)
            ))
            .OrderBy(summary => summary.Category)
            .ToList();

        return new(items, items.Count);
    }
}
