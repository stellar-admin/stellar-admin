namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a user menu item that links to a URL.
/// </summary>
public sealed class UrlUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     Gets the absolute URL or app-relative path that the item links to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     Gets the name of the icon shown before the text, or <see langword="null" /> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets the text of the item.
    /// </summary>
    public string Text { get; }

    internal UrlUserMenuItem(string text, string url)
    {
        Text = text;
        Href = url;
    }
}
