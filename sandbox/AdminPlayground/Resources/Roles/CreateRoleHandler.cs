using AdminPlayground.Data;
using AdminPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Roles;

public sealed class CreateRoleHandler(RoleManager<ApplicationRole> roles)
    : IResourceCreateHandler<RoleFormModel>
{
    public async Task<ResourceOperationResult> CreateAsync(
        RoleFormModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        return IdentityOperationResults.ForRole(
            await roles.CreateAsync(
                new ApplicationRole { Name = model.Name, Description = model.Description }
            )
        );
    }
}
