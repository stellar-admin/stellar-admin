using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Identity.Options;

/// <summary>
///     The configured options for the user management screens.
/// </summary>
/// <typeparam name="TUser">The Identity user type.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
public class IdentityUsersOptions<TUser, TKey> : ResourceOptions<TUser>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    public IdentityUsersOptions()
        : base(
            new IndexPageDefaults(
                Title: "Users",
                CreateLabel: "New user",
                EmptyTitle: "No users yet",
                EmptyDescription: "User accounts will appear here once they have been created.",
                EmptyIcon: "users",
                Columns: ["UserName", "Email", "EmailConfirmed"],
                SortBy: "UserName"
            ),
            new FormPageDefaults(
                Title: "Create user",
                SubmitLabel: "Create user",
                Fields: ["UserName", "Email"]
            ),
            new FormPageDefaults(
                Title: "Edit user",
                SubmitLabel: "Save changes",
                Fields: ["UserName", "Email", "PhoneNumber"]
            ),
            new DeleteDefaults<TUser>(
                Title: "Delete user",
                MessageFormat: "Are you sure you want to delete the user {0}?",
                ConfirmLabel: "Delete",
                CancelLabel: "Cancel",
                DisplayName: user => user.UserName
            )
        ) { }
}
