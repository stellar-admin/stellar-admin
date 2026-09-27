using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Roles;

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
