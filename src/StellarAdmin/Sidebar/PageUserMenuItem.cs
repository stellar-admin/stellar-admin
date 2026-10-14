namespace StellarAdmin.Sidebar;

/// <summary>
///     A user menu item that links to a Razor Page.
/// </summary>
public sealed class PageUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     The area name, or <c>null</c> for a page outside any area.
    /// </summary>
    public string? Area { get; }

    /// <summary>
    ///     The icon shown before the text, or <c>null</c> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     The page name.
    /// </summary>
    public string PageName { get; }

    /// <summary>
    ///     The item text.
    /// </summary>
    public string Text { get; }

    internal PageUserMenuItem(string text, string page, string? area)
    {
        Text = text;
        PageName = page;
        Area = area;
    }
}
