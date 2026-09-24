using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class UserDataSource(UserManager<IdentityUser> users)
    : IResourceDataSource<IdentityUser>,
        IResourceDeleteHandler<IdentityUser>
{
    public async Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return ResourceOperationResult.NotFound();
        }

        return IdentityOperationResults.ForUser(await users.DeleteAsync(user), creating: false);
    }

    public async Task<ResourceListResult<IdentityUser>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = users.Users.AsNoTracking();
        if (request.Search is { } term)
        {
            query = query.Where(user => user.Email != null && user.Email.Contains(term));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        IOrderedQueryable<IdentityUser> ordered = request.Sort switch
        {
            { Field: nameof(IdentityUser.Email), Direction: ResourceSortDirection.Ascending } =>
                query.OrderBy(user => user.Email),
            { Field: nameof(IdentityUser.Email), Direction: ResourceSortDirection.Descending } =>
                query.OrderByDescending(user => user.Email),
            {
                Field: nameof(IdentityUser.EmailConfirmed),
                Direction: ResourceSortDirection.Ascending
            } => query.OrderBy(user => user.EmailConfirmed),
            {
                Field: nameof(IdentityUser.EmailConfirmed),
                Direction: ResourceSortDirection.Descending
            } => query.OrderByDescending(user => user.EmailConfirmed),
            _ => query.OrderBy(user => user.Email),
        };
        query = ordered.ThenBy(user => user.Id);

        if (request.Paging is { } paging)
        {
            query = query.Skip(checked((paging.Page - 1) * paging.PageSize)).Take(paging.PageSize);
        }

        return new(await query.ToListAsync(cancellationToken), totalCount);
    }
}
