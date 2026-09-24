using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Identity;

internal static class IdentityOperationResults
{
    public static ResourceOperationResult ForRole(IdentityResult result) =>
        FromIdentity(
            result,
            error =>
                error.Code
                    is nameof(IdentityErrorDescriber.DuplicateRoleName)
                        or nameof(IdentityErrorDescriber.InvalidRoleName)
                    ? nameof(RoleFormModel.Name)
                    : null
        );

    public static ResourceOperationResult ForUser(IdentityResult result, bool creating) =>
        FromIdentity(
            result,
            error =>
                error.Code switch
                {
                    nameof(IdentityErrorDescriber.DuplicateUserName)
                    or nameof(IdentityErrorDescriber.InvalidUserName) => nameof(
                        EditUserModel.Email
                    ),
                    nameof(IdentityErrorDescriber.DuplicateEmail)
                    or nameof(IdentityErrorDescriber.InvalidEmail) => nameof(EditUserModel.Email),
                    _ when creating
                            && error.Code.StartsWith("Password", StringComparison.Ordinal) =>
                        nameof(CreateUserModel.Password),
                    _ => null,
                }
        );

    private static ResourceOperationResult FromIdentity(
        IdentityResult result,
        Func<IdentityError, string?> fieldName
    ) =>
        result.Succeeded
            ? ResourceOperationResult.Success()
            : ResourceOperationResult.ValidationFailed(
                result.Errors.Select(error => new ResourceValidationError(
                    fieldName(error),
                    error.Description
                ))
            );
}
