using DashboardPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Categories;

// Loads a category's products, another resource, when its row first expands
public sealed class CategoryProductsHandler(ApplicationDbContext db)
    : IResourceRowDetailHandler<CategoryProducts>
{
    public async Task<CategoryProducts?> FindAsync(string id, CancellationToken cancellationToken)
    {
        if (
            !int.TryParse(id, out var categoryId)
            || !await db.Categories.AnyAsync(
                category => category.Id == categoryId,
                cancellationToken
            )
        )
        {
            return null;
        }

        var products = db.Products.Where(product => product.CategoryId == categoryId);
        var count = await products.CountAsync(cancellationToken);
        var top = await products
            .OrderBy(product => product.Name)
            .Take(5)
            .Select(product => new CategoryProduct(
                product.Name,
                product.Details.Sku,
                product.Price
            ))
            .ToListAsync(cancellationToken);

        return new CategoryProducts(count, top);
    }
}
