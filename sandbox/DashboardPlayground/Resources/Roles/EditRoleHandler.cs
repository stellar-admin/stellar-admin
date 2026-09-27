using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Roles;

public sealed class EditRoleHandler(RoleManager<ApplicationRole> roles)
    : IResourceEditHandler<RoleFormModel>
{
    public async Task<RoleFormModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roles.FindByIdAsync(id);

        return role is null
            ? null
            : new RoleFormModel { Name = role.Name ?? "", Description = role.Description };
    }

    public async Task<ResourceOperationResult> UpdateAsync(
        string id,
        RoleFormModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roles.FindByIdAsync(id);
        if (role is null)
        {
            return ResourceOperationResult.NotFound();
        }

        role.Name = model.Name;
        role.Description = model.Description;

        return IdentityOperationResults.ForRole(await roles.UpdateAsync(role));
    }
}
