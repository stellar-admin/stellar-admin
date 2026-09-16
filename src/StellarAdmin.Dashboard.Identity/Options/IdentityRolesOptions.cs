using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Identity.Options;

/// <summary>
///     The configured options for the role management screens.
/// </summary>
/// <typeparam name="TRole">The Identity role type.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
public class IdentityRolesOptions<TRole, TKey> : ResourceOptions<TRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public IdentityRolesOptions()
        : base(
            new IndexPageDefaults(
                Title: "Roles",
                CreateLabel: "New role",
                EmptyTitle: "No roles yet",
                EmptyDescription: "Roles will appear here once they have been created.",
                EmptyIcon: "shield",
                Columns: ["Name"],
                SortBy: "Name"
            ),
            new FormPageDefaults(
                Title: "Create role",
                SubmitLabel: "Create role",
                Fields: ["Name"]
            ),
            new FormPageDefaults(Title: "Edit role", SubmitLabel: "Save changes", Fields: ["Name"]),
            new DeleteDefaults<TRole>(
                Title: "Delete role",
                MessageFormat: "Are you sure you want to delete the role {0}?",
                ConfirmLabel: "Delete",
                CancelLabel: "Cancel",
                DisplayName: role => role.Name
            )
        ) { }
}
