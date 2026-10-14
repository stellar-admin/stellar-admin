namespace StellarAdmin.Sidebar;

/// <summary>
///     A user menu item that links to a URL.
/// </summary>
public sealed class UrlUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     The absolute URL or app-relative path the item links to.
    /// </summary>
    public string Href { get; }

    /// <summary>
    ///     The icon shown before the text, or <c>null</c> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     The item text.
    /// </summary>
    public string Text { get; }

    internal UrlUserMenuItem(string text, string url)
    {
        Text = text;
        Href = url;
    }
}
