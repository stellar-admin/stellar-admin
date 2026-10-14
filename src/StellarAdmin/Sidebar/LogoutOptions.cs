namespace StellarAdmin.Sidebar;

internal sealed class LogoutOptions
{
    public List<string> AuthenticationSchemes { get; } = [];

    public string? RedirectPath { get; set; }
}
