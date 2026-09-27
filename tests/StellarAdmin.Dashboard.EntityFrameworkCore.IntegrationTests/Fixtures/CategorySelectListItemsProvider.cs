using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class CategorySelectListItemsProvider(CatalogDbContext db) : ISelectListItemsProvider
{
    public async Task<IReadOnlyList<SelectListItem>> GetItemsAsync(
        CancellationToken cancellationToken
    )
    {
        var categories = await db.Set<Category>()
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return
        [
            new("Not set", ""),
            .. categories.Select(category => new SelectListItem(
                category.Name,
                category.Id.ToString()
            )),
        ];
    }
}
