namespace StellarAdmin.Sidebar;

internal sealed class LogoutOptions
{
    public List<string> AuthenticationSchemes { get; } = [];

    public string? Icon { get; set; } = "log-out";

    public string? Path { get; set; }

    public string? RedirectPath { get; set; }

    public bool Show { get; set; } = true;
}
