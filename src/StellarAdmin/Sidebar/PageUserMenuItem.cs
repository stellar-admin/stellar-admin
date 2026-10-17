namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a user menu item that links to a Razor Page.
/// </summary>
public sealed class PageUserMenuItem : UserMenuItem
{
    /// <summary>
    ///     Gets the name of the area, or <see langword="null" /> for a page outside any area.
    /// </summary>
    public string? Area { get; }

    /// <summary>
    ///     Gets the name of the icon shown before the text, or <see langword="null" /> for none.
    /// </summary>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets the name of the page.
    /// </summary>
    public string PageName { get; }

    /// <summary>
    ///     Gets the text of the item.
    /// </summary>
    public string Text { get; }

    internal PageUserMenuItem(string text, string page, string? area)
    {
        Text = text;
        PageName = page;
        Area = area;
    }
}
