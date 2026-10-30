using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Widgets;

namespace AdminPlayground.Dashboard;

public sealed record CatalogStats(int Products, int Categories, decimal AveragePrice);

// Counts the catalog for the home page's stats
public sealed class CatalogStatsHandler(ApplicationDbContext db) : IWidgetHandler<CatalogStats>
{
    public async Task<CatalogStats?> LoadAsync(
        WidgetLoadContext context,
        CancellationToken cancellationToken
    )
    {
        var products = await db.Products.CountAsync(cancellationToken);
        var categories = await db.Categories.CountAsync(cancellationToken);

        // SQLite can't average decimals, so the prices come back to be averaged here
        var prices = await db
            .Products.Select(product => product.Price)
            .ToListAsync(cancellationToken);

        return new(products, categories, prices.Count == 0 ? 0 : prices.Average());
    }
}
