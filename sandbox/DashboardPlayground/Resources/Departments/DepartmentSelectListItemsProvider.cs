using DashboardPlayground.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.Departments;

public sealed class DepartmentSelectListItemsProvider(ApplicationDbContext db)
    : ISelectListItemsProvider
{
    public async Task<IReadOnlyList<SelectListItem>> GetItemsAsync(
        CancellationToken cancellationToken
    )
    {
        var departments = await db
            .Departments.AsNoTracking()
            .OrderBy(department => department.Name)
            .ToListAsync(cancellationToken);

        return
        [
            new("Not set", ""),
            .. departments.Select(department => new SelectListItem(
                department.Name,
                department.Id.ToString()
            )),
        ];
    }
}
