using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Users;

public sealed class EditUserHandler(ApplicationDbContext db, UserManager<ApplicationUser> users)
    : IResourceEditHandler<EditUserModel>
{
    public async Task<EditUserModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await users.FindByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        var roleIds = await db
            .UserRoles.Where(role => role.UserId == id)
            .Select(role => role.RoleId)
            .ToArrayAsync(cancellationToken);

        return new EditUserModel
        {
            DepartmentId = user.DepartmentId,
            Email = user.Email ?? "",
            EmailConfirmed = user.EmailConfirmed,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PreferredLanguage = user.PreferredLanguage,
            RoleIds = roleIds,
            TimeZoneId = user.TimeZoneId,
        };
    }

    public async Task<ResourceOperationResult> UpdateAsync(
        string id,
        EditUserModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await users.FindByIdAsync(id);
        if (user is null)
        {
            return ResourceOperationResult.NotFound();
        }

        var currentRoleIds = await db
            .UserRoles.Where(role => role.UserId == id)
            .Select(role => role.RoleId)
            .ToArrayAsync(cancellationToken);
        var selectedRoleIds = model.RoleIds.ToHashSet(StringComparer.Ordinal);
        var roleNames = await UserRoleAssignments.ResolveNamesAsync(
            db,
            currentRoleIds.Concat(selectedRoleIds),
            cancellationToken
        );
        if (roleNames is null)
        {
            return ResourceOperationResult.ValidationFailed(
                nameof(EditUserModel.RoleIds),
                "Select valid roles."
            );
        }

        user.DepartmentId = model.DepartmentId;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PreferredLanguage = model.PreferredLanguage;
        user.TimeZoneId = model.TimeZoneId;
        user.UserName = model.Email;
        user.Email = model.Email;
        user.EmailConfirmed = model.EmailConfirmed;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return IdentityOperationResults.ForUser(result, creating: false);
        }

        var removedRoleNames = currentRoleIds
            .Except(selectedRoleIds, StringComparer.Ordinal)
            .Select(roleId => roleNames[roleId])
            .ToArray();
        if (removedRoleNames.Length > 0)
        {
            result = await users.RemoveFromRolesAsync(user, removedRoleNames);
            if (!result.Succeeded)
            {
                return IdentityOperationResults.ForUser(result, creating: false);
            }
        }

        var addedRoleNames = selectedRoleIds
            .Except(currentRoleIds, StringComparer.Ordinal)
            .Select(roleId => roleNames[roleId])
            .ToArray();
        if (addedRoleNames.Length > 0)
        {
            result = await users.AddToRolesAsync(user, addedRoleNames);
            if (!result.Succeeded)
            {
                return IdentityOperationResults.ForUser(result, creating: false);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return ResourceOperationResult.Success();
    }
}
