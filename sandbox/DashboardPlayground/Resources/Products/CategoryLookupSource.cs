using DashboardPlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Editors;

namespace DashboardPlayground.Resources.Products;

// Temporary hand-written source until the EF Core lookup items arrive
internal sealed class CategoryLookupSource(ApplicationDbContext db) : ILookupSource<Category, int>
{
    public Task<Category?> FindAsync(int value, CancellationToken cancellationToken) =>
        db.Set<Category>().FirstOrDefaultAsync(category => category.Id == value, cancellationToken);

    public async Task<LookupPage<Category>> SearchAsync(
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var categories = db.Set<Category>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Term))
        {
            categories = categories.Where(category => category.Name.Contains(query.Term));
        }

        var items = await categories
            .OrderBy(category => category.Name)
            .Skip(query.Skip)
            .Take(query.Take + 1)
            .ToListAsync(cancellationToken);

        return new LookupPage<Category>(items.Take(query.Take).ToList(), items.Count > query.Take);
    }
}
