using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Users;

public sealed class UserDataSource(UserManager<ApplicationUser> users)
    : IResourceDataSource<ApplicationUser>,
        IResourceDeleteHandler<ApplicationUser>
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

    public async Task<ResourceListResult<ApplicationUser>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        IQueryable<ApplicationUser> query = users
            .Users.AsNoTracking()
            .Include(user => user.Department);
        if (request.Search is { } term)
        {
            query = query.Where(user =>
                (user.Email != null && user.Email.Contains(term))
                || user.FirstName.Contains(term)
                || user.LastName.Contains(term)
            );
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        IOrderedQueryable<ApplicationUser> ordered = request.Sort switch
        {
            { Field: nameof(ApplicationUser.Email), Direction: ResourceSortDirection.Ascending } =>
                query.OrderBy(user => user.Email),
            { Field: nameof(ApplicationUser.Email), Direction: ResourceSortDirection.Descending } =>
                query.OrderByDescending(user => user.Email),
            {
                Field: nameof(ApplicationUser.EmailConfirmed),
                Direction: ResourceSortDirection.Ascending
            } => query.OrderBy(user => user.EmailConfirmed),
            {
                Field: nameof(ApplicationUser.EmailConfirmed),
                Direction: ResourceSortDirection.Descending
            } => query.OrderByDescending(user => user.EmailConfirmed),
            {
                Field: nameof(ApplicationUser.FirstName),
                Direction: ResourceSortDirection.Ascending
            } => query.OrderBy(user => user.FirstName),
            {
                Field: nameof(ApplicationUser.FirstName),
                Direction: ResourceSortDirection.Descending
            } => query.OrderByDescending(user => user.FirstName),
            {
                Field: nameof(ApplicationUser.LastName),
                Direction: ResourceSortDirection.Ascending
            } => query.OrderBy(user => user.LastName),
            {
                Field: nameof(ApplicationUser.LastName),
                Direction: ResourceSortDirection.Descending
            } => query.OrderByDescending(user => user.LastName),
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
