using DashboardPlayground.Data;
using DashboardPlayground.Resources.Shared;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.Users;

public sealed class CreateUserHandler(ApplicationDbContext db, UserManager<ApplicationUser> users)
    : IResourceCreateHandler<CreateUserModel>
{
    public async Task<ResourceOperationResult> CreateAsync(
        CreateUserModel model,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleNames = await UserRoleAssignments.ResolveNamesAsync(
            db,
            model.RoleIds,
            cancellationToken
        );
        if (roleNames is null)
        {
            return ResourceOperationResult.ValidationFailed(
                nameof(CreateUserModel.RoleIds),
                "Select valid roles."
            );
        }

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
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return IdentityOperationResults.ForUser(result, creating: true);
        }

        if (roleNames.Count > 0)
        {
            result = await users.AddToRolesAsync(user, roleNames.Values);
            if (!result.Succeeded)
            {
                return IdentityOperationResults.ForUser(result, creating: true);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return ResourceOperationResult.Success();
    }
}
