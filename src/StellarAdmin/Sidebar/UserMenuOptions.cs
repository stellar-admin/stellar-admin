namespace StellarAdmin.Sidebar;

internal sealed class UserMenuOptions
{
    public string? AvatarClaimType { get; set; }

    public string? DescriptionClaimType { get; set; }

    public List<UserMenuItem> Items { get; } = [UserMenuItem.Logout()];

    public LogoutOptions Logout { get; } = new();

    public string? NameClaimType { get; set; }
}
