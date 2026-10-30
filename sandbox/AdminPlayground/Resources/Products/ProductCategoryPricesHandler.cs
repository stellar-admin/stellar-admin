using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Widgets;

namespace AdminPlayground.Resources.Products;

// The product the view or edit page shows
public sealed record ProductCategoryPricesParameters(int ProductId);

public sealed record ProductCategoryPrices(
    string? Category,
    decimal Price,
    int Products,
    decimal Lowest,
    decimal Average,
    decimal Highest
);

// Compares the product's price with the prices of the products in its category
public sealed class ProductCategoryPricesHandler(ApplicationDbContext db)
    : IWidgetHandler<ProductCategoryPricesParameters, ProductCategoryPrices>
{
    public async Task<ProductCategoryPrices?> LoadAsync(
        ProductCategoryPricesParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    )
    {
        var product = await db
            .Products.Where(product => product.Id == parameters.ProductId)
            .Select(product => new
            {
                product.Price,
                product.CategoryId,
                Category = product.Category != null ? product.Category.Name : null,
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (product?.CategoryId is null)
        {
            return null;
        }

        // SQLite can't aggregate decimals, so the prices come back to be summarized here
        var prices = await db
            .Products.Where(other => other.CategoryId == product.CategoryId)
            .Select(other => other.Price)
            .ToListAsync(cancellationToken);

        return new(
            product.Category,
            product.Price,
            prices.Count,
            prices.Min(),
            prices.Average(),
            prices.Max()
        );
    }
}
