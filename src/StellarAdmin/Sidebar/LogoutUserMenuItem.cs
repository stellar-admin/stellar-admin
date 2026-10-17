namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents the user menu's log out item.
/// </summary>
/// <remarks>
///     The item is shown after the other items. It logs the user out by posting a form.
/// </remarks>
public sealed class LogoutUserMenuItem
{
    /// <summary>
    ///     Gets the name of the icon shown before the text, or <see langword="null" /> for none.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    ///     Gets the app-relative path that the form posts to, or <see langword="null" /> for the
    ///     admin panel's logout endpoint.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    ///     Gets the text of the item.
    /// </summary>
    public string Text => "Log out";

    internal LogoutUserMenuItem(string? path, string? icon)
    {
        Path = path;
        Icon = icon;
    }
}
