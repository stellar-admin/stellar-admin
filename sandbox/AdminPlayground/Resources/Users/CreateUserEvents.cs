using AdminPlayground.Data;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.EntityFrameworkCore;

namespace AdminPlayground.Resources.Users;

// Creates the user with its password through UserManager, which saves the selected roles with it
public sealed class CreateUserEvents(UserManager<ApplicationUser> users)
    : EfCoreCreateEvents<ApplicationUser, CreateUserModel>
{
    public override async Task CreatingAsync(
        EfCoreSaveContext<ApplicationUser, CreateUserModel> context,
        CancellationToken cancellationToken
    )
    {
        var user = context.Entity;
        user.UserName = user.Email;
        var result = await users.CreateAsync(user, context.Model.Password);
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
