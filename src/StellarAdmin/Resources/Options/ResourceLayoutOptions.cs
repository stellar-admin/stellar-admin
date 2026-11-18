namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of one layout of a resource's index page.
/// </summary>
public abstract class ResourceLayoutOptions
{
    /// <summary>
    ///     Gets the name of the icon on the layout's switcher segment.
    /// </summary>
    /// <value>
    ///     The icon name, or <see langword="null" /> to show the title instead. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the layout ignores the scopes.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to hide the scopes and list every resource while the layout is
    ///     shown; otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool IgnoresScopes { get; internal set; }

    /// <summary>
    ///     Gets a value that indicates whether the layout ignores the search.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to hide the search box and leave the list unsearched while the
    ///     layout is shown; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    public bool IgnoresSearch { get; internal set; }

    /// <summary>
    ///     Gets the key that identifies the layout.
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Gets the layout's title, which names its switcher segment.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the key. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Title { get; internal set; }

    private protected ResourceLayoutOptions(string key) => Key = key;
}
