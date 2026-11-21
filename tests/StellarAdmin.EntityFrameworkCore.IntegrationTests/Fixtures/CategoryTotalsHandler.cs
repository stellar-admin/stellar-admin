using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// The number of products per category, from the entities the scope and search select
public sealed class CategoryTotalsHandler : IEfCoreDataLayoutHandler<Product, CategoryTotal>
{
    public async Task<ResourceListResult<CategoryTotal>> ListAsync(
        IQueryable<Product> entities,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var items = await entities
            .GroupBy(product => product.Category!.Name)
            .OrderBy(category => category.Key)
            .Select(category => new CategoryTotal(category.Key, category.Count()))
            .ToListAsync(cancellationToken);

        return new(items, items.Count);
    }
}
