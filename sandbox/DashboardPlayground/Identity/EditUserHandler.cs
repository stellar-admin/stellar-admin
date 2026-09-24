using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class EditUserHandler(UserManager<IdentityUser> users)
    : IResourceEditHandler<EditUserModel>
{
    public async Task<EditUserModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await users.FindByIdAsync(id);

        return user is null
            ? null
            : new EditUserModel { Email = user.Email ?? "", EmailConfirmed = user.EmailConfirmed };
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

        user.UserName = model.Email;
        user.Email = model.Email;
        user.EmailConfirmed = model.EmailConfirmed;

        return IdentityOperationResults.ForUser(await users.UpdateAsync(user), creating: false);
    }
}
