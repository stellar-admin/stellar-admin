namespace StellarAdmin.Sidebar;

/// <summary>
///     The signed-in user and their menu items, the model of <c>_SidebarUserMenu.cshtml</c>.
/// </summary>
public sealed class SidebarUserMenuModel
{
    /// <summary>
    ///     The URL of the user's picture, or <c>null</c> to show initials.
    /// </summary>
    public string? AvatarUrl { get; }

    /// <summary>
    ///     The line under the name, or <c>null</c> for none.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    ///     The menu items, in the order they are shown.
    /// </summary>
    public IReadOnlyList<UserMenuItem> Items { get; }

    /// <summary>
    ///     The log out item, shown after the other items, or <c>null</c> when log out is hidden.
    /// </summary>
    public LogoutUserMenuItem? Logout { get; }

    /// <summary>
    ///     The user's name.
    /// </summary>
    public string Name { get; }

    internal SidebarUserMenuModel(
        string name,
        string? description,
        string? avatarUrl,
        IReadOnlyList<UserMenuItem> items,
        LogoutUserMenuItem? logout
    )
    {
        Name = name;
        Description = description;
        AvatarUrl = avatarUrl;
        Items = items;
        Logout = logout;
    }
}
