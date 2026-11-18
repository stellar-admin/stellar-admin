namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents a segment of an index page's layout switcher.
/// </summary>
public sealed class ResourceIndexLayoutViewModel
{
    /// <summary>
    ///     Gets the name of the segment's icon, or <see langword="null" /> to show the title instead.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    ///     Gets a value that indicates whether the layout is shown.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the layout is shown; otherwise, <see langword="false" />.
    /// </value>
    public bool IsActive { get; }

    /// <summary>
    ///     Gets the key that identifies the layout in the query string.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the segment's title, which is its tooltip and accessible name.
    /// </summary>
    public string Title { get; }

    internal ResourceIndexLayoutViewModel(string key, string title, string? icon, bool isActive)
    {
        Key = key;
        Title = title;
        Icon = icon;
        IsActive = isActive;
    }
}
