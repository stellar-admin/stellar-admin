using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class CreateUserHandler(UserManager<IdentityUser> users)
    : IResourceCreateHandler<CreateUserModel>
{
    public async Task<ResourceOperationResult> CreateAsync(
        CreateUserModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed,
        };
        var result = await users.CreateAsync(user, model.Password);

        return IdentityOperationResults.ForUser(result, creating: true);
    }
}
