using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Users;

public sealed class EditUserHandler(UserManager<ApplicationUser> users)
    : IResourceEditHandler<EditUserModel>
{
    public async Task<EditUserModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await users.FindByIdAsync(id);

        return user is null
            ? null
            : new EditUserModel
            {
                DepartmentId = user.DepartmentId,
                Email = user.Email ?? "",
                EmailConfirmed = user.EmailConfirmed,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredLanguage = user.PreferredLanguage,
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

        user.DepartmentId = model.DepartmentId;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PreferredLanguage = model.PreferredLanguage;
        user.TimeZoneId = model.TimeZoneId;
        user.UserName = model.Email;
        user.Email = model.Email;
        user.EmailConfirmed = model.EmailConfirmed;

        return IdentityOperationResults.ForUser(await users.UpdateAsync(user), creating: false);
    }
}
