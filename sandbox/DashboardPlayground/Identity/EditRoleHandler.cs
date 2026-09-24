using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

public sealed class EditRoleHandler(RoleManager<IdentityRole> roles)
    : IResourceEditHandler<RoleFormModel>
{
    public async Task<RoleFormModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roles.FindByIdAsync(id);

        return role is null ? null : new RoleFormModel { Name = role.Name ?? "" };
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

        return IdentityOperationResults.ForRole(await roles.UpdateAsync(role));
    }
}
