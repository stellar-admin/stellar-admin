namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents the signed-in user and their menu items, for the <c>_SidebarUserMenu.cshtml</c>
///     view.
/// </summary>
public sealed class SidebarUserMenuModel
{
    /// <summary>
    ///     Gets the URL of the user's picture, or <see langword="null" /> to show the user's
    ///     initials.
    /// </summary>
    public string? AvatarUrl { get; }

    /// <summary>
    ///     Gets the line under the user's name, or <see langword="null" /> for none.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    ///     Gets the menu items, in the order they are shown.
    /// </summary>
    public IReadOnlyList<UserMenuItem> Items { get; }

    /// <summary>
    ///     Gets the log out item, or <see langword="null" /> when log out is hidden.
    /// </summary>
    /// <remarks>
    ///     The log out item is shown after the other items.
    /// </remarks>
    public LogoutUserMenuItem? Logout { get; }

    /// <summary>
    ///     Gets the user's name.
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
