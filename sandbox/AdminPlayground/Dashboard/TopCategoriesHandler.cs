using AdminPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Widgets;

namespace AdminPlayground.Dashboard;

// The widget's picker changes Count; the page sets the first value
public sealed record TopCategoriesParameters([property: WidgetInput] int Count);

public sealed record TopCategories(int Count, IReadOnlyList<CategoryProductCount> Categories);

public sealed record CategoryProductCount(string Name, int Products);

// The categories with the most products
public sealed class TopCategoriesHandler(ApplicationDbContext db)
    : IWidgetHandler<TopCategoriesParameters, TopCategories>
{
    public async Task<TopCategories?> LoadAsync(
        TopCategoriesParameters parameters,
        WidgetLoadContext context,
        CancellationToken cancellationToken
    )
    {
        var count = Math.Clamp(parameters.Count, 1, 10);
        var categories = await db
            .Products.Where(product => product.Category != null)
            .GroupBy(product => product.Category!.Name)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(count)
            .Select(group => new CategoryProductCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken);

        return new(count, categories);
    }
}
