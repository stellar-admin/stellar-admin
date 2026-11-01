using Microsoft.AspNetCore.Identity;

namespace AdminPlayground.Resources.Users;

internal static class UserIdentityErrors
{
    // The user name is the email, so errors on either belong to the email field
    internal static bool IsEmailError(IdentityError error) =>
        error.Code
            is nameof(IdentityErrorDescriber.DuplicateUserName)
                or nameof(IdentityErrorDescriber.InvalidUserName)
                or nameof(IdentityErrorDescriber.DuplicateEmail)
                or nameof(IdentityErrorDescriber.InvalidEmail);
}
