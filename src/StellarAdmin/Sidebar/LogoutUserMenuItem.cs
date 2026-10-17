namespace StellarAdmin.Sidebar;

/// <summary>
///     The user menu's log out item, shown after the other items. It logs the user out by posting a form.
/// </summary>
public sealed class LogoutUserMenuItem
{
    /// <summary>
    ///     The icon shown before the text, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>log-out</c>.
    /// </remarks>
    public string? Icon { get; }

    /// <summary>
    ///     The app-relative path the form posts to, or <c>null</c> for the admin panel's logout endpoint.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    ///     The item text.
    /// </summary>
    public string Text => "Log out";

    internal LogoutUserMenuItem(string? path, string? icon)
    {
        Path = path;
        Icon = icon;
    }
}
