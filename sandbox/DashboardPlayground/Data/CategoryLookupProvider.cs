using DashboardPlayground.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Data;

public sealed class CategoryLookupProvider(ProductDbContext db) : IReferenceLookupProvider
{
    public async Task<IReadOnlyList<SelectListItem>> GetLookupsAsync(
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
