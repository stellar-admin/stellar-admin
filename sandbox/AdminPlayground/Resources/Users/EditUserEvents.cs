using AdminPlayground.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Users;

// Loads the user's roles into the form, and saves the user and its roles through UserManager, which normalizes the user
// name and email. UserManager's store uses the same context, so the user it updates is the tracked entity
public sealed class EditUserEvents(ApplicationDbContext db, UserManager<ApplicationUser> users)
    : EfCoreEditEvents<ApplicationUser, EditUserModel>
{
    public override async Task LoadedAsync(
        EfCoreLoadedContext<ApplicationUser, EditUserModel> context,
        CancellationToken cancellationToken
    ) =>
        context.Model.RoleIds = await db
            .UserRoles.Where(role => role.UserId == context.Entity.Id)
            .Select(role => role.RoleId)
            .ToArrayAsync(cancellationToken);

    public override async Task UpdatingAsync(
        EfCoreSaveContext<ApplicationUser, EditUserModel> context,
        CancellationToken cancellationToken
    )
    {
        var user = context.Entity;
        var currentRoleIds = await db
            .UserRoles.Where(role => role.UserId == user.Id)
            .Select(role => role.RoleId)
            .ToArrayAsync(cancellationToken);
        var selectedRoleIds = context.Model.RoleIds.ToHashSet(StringComparer.Ordinal);
        var roleNames = await UserRoleAssignments.ResolveNamesAsync(
            db,
            currentRoleIds.Concat(selectedRoleIds),
            cancellationToken
        );
        if (roleNames is null)
        {
            context.Fail(model => model.RoleIds, "Select valid roles.");

            return;
        }

        user.UserName = user.Email;
        var result = await users.UpdateAsync(user);

        var removedRoleNames = currentRoleIds
            .Except(selectedRoleIds, StringComparer.Ordinal)
            .Select(roleId => roleNames[roleId])
            .ToArray();
        if (result.Succeeded && removedRoleNames.Length > 0)
        {
            result = await users.RemoveFromRolesAsync(user, removedRoleNames);
        }

        var addedRoleNames = selectedRoleIds
            .Except(currentRoleIds, StringComparer.Ordinal)
            .Select(roleId => roleNames[roleId])
            .ToArray();
        if (result.Succeeded && addedRoleNames.Length > 0)
        {
            result = await users.AddToRolesAsync(user, addedRoleNames);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                if (UserIdentityErrors.IsEmailError(error))
                {
                    context.Fail(model => model.User.Email, error.Description);
                }
                else
                {
                    context.Fail(error.Description);
                }
            }

            return;
        }

        context.Handled();
    }
}
