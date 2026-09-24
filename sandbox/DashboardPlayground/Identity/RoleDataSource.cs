using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class RoleDataSource(RoleManager<IdentityRole> roles)
    : IResourceDataSource<IdentityRole>,
        IResourceDeleteHandler<IdentityRole>
{
    public async Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roles.FindByIdAsync(id);
        if (role is null)
        {
            return ResourceOperationResult.NotFound();
        }

        return IdentityOperationResults.ForRole(await roles.DeleteAsync(role));
    }

    public async Task<ResourceListResult<IdentityRole>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = roles.Roles.AsNoTracking();
        if (request.Search is { } term)
        {
            query = query.Where(role => role.Name != null && role.Name.Contains(term));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        IOrderedQueryable<IdentityRole> ordered = request.Sort switch
        {
            { Field: nameof(IdentityRole.Name), Direction: ResourceSortDirection.Descending } =>
                query.OrderByDescending(role => role.Name),
            _ => query.OrderBy(role => role.Name),
        };
        query = ordered.ThenBy(role => role.Id);

        if (request.Paging is { } paging)
        {
            query = query.Skip(checked((paging.Page - 1) * paging.PageSize)).Take(paging.PageSize);
        }

        return new(await query.ToListAsync(cancellationToken), totalCount);
    }
}
