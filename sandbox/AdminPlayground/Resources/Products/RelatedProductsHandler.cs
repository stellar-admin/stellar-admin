using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Products;

// Loads up to five other products in the product's category when the Related tab is first shown
public sealed class RelatedProductsHandler(ApplicationDbContext db)
    : IResourceRowDetailHandler<RelatedProducts>
{
    public async Task<RelatedProducts?> FindAsync(string id, CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var productId))
        {
            return null;
        }

        var product = await db
            .Products.Where(product => product.Id == productId)
            .Select(product => new
            {
                product.CategoryId,
                Category = product.Category != null ? product.Category.Name : null,
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            return null;
        }

        if (product.CategoryId is null)
        {
            return new RelatedProducts(null, []);
        }

        var related = await db
            .Products.Where(other =>
                other.CategoryId == product.CategoryId && other.Id != productId
            )
            .OrderBy(other => other.Name)
            .Take(5)
            .Select(other => new RelatedProduct(other.Name, other.Details.Sku, other.Price))
            .ToListAsync(cancellationToken);

        return new RelatedProducts(product.Category, related);
    }
}
