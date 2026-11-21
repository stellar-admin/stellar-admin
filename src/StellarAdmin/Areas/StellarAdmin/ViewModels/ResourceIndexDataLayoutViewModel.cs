namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents a segment of an index page's data layout switcher.
/// </summary>
public sealed class ResourceIndexDataLayoutViewModel
{
    /// <summary>
    ///     Gets the name of the segment's icon, or <see langword="null" /> to show the title instead.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    ///     Gets a value that indicates whether the data layout is shown.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the data layout is shown; otherwise, <see langword="false"
    ///     />.
    /// </value>
    public bool IsActive { get; }

    /// <summary>
    ///     Gets the key that identifies the data layout in the query string.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the segment's title, which is its tooltip and accessible name.
    /// </summary>
    public string Title { get; }

    internal ResourceIndexDataLayoutViewModel(string key, string title, string? icon, bool isActive)
    {
        Key = key;
        Title = title;
        Icon = icon;
        IsActive = isActive;
    }
}
