using Microsoft.EntityFrameworkCore;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// Every category with no products counted: a handler that loads its own data, whatever the scope and search
public sealed class CategoryNamesHandler(CatalogDbContext db)
    : IResourceLayoutHandler<CategoryTotal>
{
    public async Task<ResourceListResult<CategoryTotal>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var items = await db.Set<Category>()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryTotal(category.Name, 0))
            .ToListAsync(cancellationToken);

        return new(items, items.Count);
    }
}
