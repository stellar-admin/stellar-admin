using AdminPlayground.Data;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Users;

// Saves the user through UserManager, which validates and normalizes its user name and email, with the selected roles.
// UserManager's store uses the same context, so the user it updates is the tracked entity
public sealed class EditUserEvents(UserManager<ApplicationUser> users)
    : EfCoreEditEvents<ApplicationUser, EditUserModel>
{
    public override async Task UpdatingAsync(
        EfCoreSaveContext<ApplicationUser, EditUserModel> context,
        CancellationToken cancellationToken
    )
    {
        var user = context.Entity;
        user.UserName = user.Email;
        var result = await users.UpdateAsync(user);
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
