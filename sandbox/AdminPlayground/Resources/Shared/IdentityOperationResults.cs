using AdminPlayground.Resources.Roles;
using Microsoft.AspNetCore.Identity;
using StellarAdmin.Resources;

namespace AdminPlayground.Resources.Shared;

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
