using DashboardPlayground.Data;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class CreateUserHandler(UserManager<ApplicationUser> users)
    : IResourceCreateHandler<CreateUserModel>
{
    public async Task<ResourceOperationResult> CreateAsync(
        CreateUserModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            DepartmentId = model.DepartmentId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PreferredLanguage = model.PreferredLanguage,
            TimeZoneId = model.TimeZoneId,
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed,
        };
        var result = await users.CreateAsync(user, model.Password);

        return IdentityOperationResults.ForUser(result, creating: true);
    }
}
