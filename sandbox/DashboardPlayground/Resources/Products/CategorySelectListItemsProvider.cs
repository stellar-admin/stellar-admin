using DashboardPlayground.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.Products;

public sealed class CategorySelectListItemsProvider(ApplicationDbContext db)
    : ISelectListItemsProvider
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
