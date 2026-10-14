namespace StellarAdmin.Sidebar;

/// <summary>
///     The signed-in user and their menu items, the model of <c>_SidebarUserMenu.cshtml</c>.
/// </summary>
/// <param name="Name">The user's name.</param>
/// <param name="Description">The line under the name, or <c>null</c> for none.</param>
/// <param name="AvatarUrl">The URL of the user's picture, or <c>null</c> to show initials.</param>
/// <param name="Items">The menu items, in the order they are shown.</param>
public sealed record SidebarUserMenuModel(
    string Name,
    string? Description,
    string? AvatarUrl,
    IReadOnlyList<UserMenuItem> Items
);
