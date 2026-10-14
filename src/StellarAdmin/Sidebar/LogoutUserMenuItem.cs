namespace StellarAdmin.Sidebar;

/// <summary>
///     A user menu item that logs the user out by posting a form.
/// </summary>
public sealed class LogoutUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     The icon shown before the text, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>log-out</c>.
    /// </remarks>
    public string? Icon { get; internal set; } = "log-out";

    /// <summary>
    ///     The app-relative path the form posts to, or <c>null</c> for the admin panel's logout endpoint.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    ///     The item text.
    /// </summary>
    public string Text => "Log out";

    internal LogoutUserMenuItem(string? path) => Path = path;
}
