using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Products;

public sealed record CategorySummary(
    int? CategoryId,
    string Category,
    int Products,
    decimal Average
);

// The products the scope and search select, counted per category, for a table whose rows link to the category. SQLite
// can't average decimals, so the prices come back to be summarized here
public sealed class CategorySummaryHandler : IEfCoreDataLayoutHandler<Product, CategorySummary>
{
    public async Task<ResourceListResult<CategorySummary>> ListAsync(
        IQueryable<Product> entities,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var prices = await entities
            .Select(product => new
            {
                product.CategoryId,
                Category = product.Category != null ? product.Category.Name : null,
                product.Price,
            })
            .ToListAsync(cancellationToken);
        var rows = prices
            .GroupBy(product => (product.CategoryId, product.Category))
            .Select(category => new CategorySummary(
                category.Key.CategoryId,
                category.Key.Category ?? "No category",
                category.Count(),
                Math.Round(category.Average(product => product.Price), 2)
            ));

        var descending = request.Sort?.Direction == ResourceSortDirection.Descending;
        rows = request.Sort?.Field switch
        {
            nameof(CategorySummary.Products) => descending
                ? rows.OrderByDescending(row => row.Products)
                : rows.OrderBy(row => row.Products),
            nameof(CategorySummary.Average) => descending
                ? rows.OrderByDescending(row => row.Average)
                : rows.OrderBy(row => row.Average),
            _ => descending
                ? rows.OrderByDescending(row => row.Category)
                : rows.OrderBy(row => row.Category),
        };
        var items = rows.ToList();

        return new(items, items.Count);
    }
}
