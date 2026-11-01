using AdminPlayground.Data;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Users;

// Creates the user with its password and roles through UserManager, inside the form's transaction, so a role that
// can't be added also removes the user
public sealed class CreateUserEvents(ApplicationDbContext db, UserManager<ApplicationUser> users)
    : EfCoreCreateEvents<ApplicationUser, CreateUserModel>
{
    public override async Task CreatingAsync(
        EfCoreSaveContext<ApplicationUser, CreateUserModel> context,
        CancellationToken cancellationToken
    )
    {
        var roleNames = await UserRoleAssignments.ResolveNamesAsync(
            db,
            context.Model.RoleIds,
            cancellationToken
        );
        if (roleNames is null)
        {
            context.Fail(model => model.RoleIds, "Select valid roles.");

            return;
        }

        var user = context.Entity;
        user.UserName = user.Email;
        var result = await users.CreateAsync(user, context.Model.Password);
        if (result.Succeeded && roleNames.Count > 0)
        {
            result = await users.AddToRolesAsync(user, roleNames.Values);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                if (UserIdentityErrors.IsEmailError(error))
                {
                    context.Fail(model => model.User.Email, error.Description);
                }
                else if (error.Code.StartsWith("Password", StringComparison.Ordinal))
                {
                    context.Fail(model => model.Password, error.Description);
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
