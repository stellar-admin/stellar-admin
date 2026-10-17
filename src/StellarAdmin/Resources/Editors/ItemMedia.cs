namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents the media displayed beside an item's text.
/// </summary>
/// <remarks>
///     The media is an <see cref="Icon" />, an <see cref="Avatar" />, an <see cref="Image" /> or a
///     <see cref="Code" />.
/// </remarks>
public abstract record ItemMedia
{
    private ItemMedia() { }

    /// <summary>
    ///     Represents a registered icon, displayed in the text color.
    /// </summary>
    /// <param name="Name">The icon's name.</param>
    public sealed record Icon(string Name) : ItemMedia;

    /// <summary>
    ///     Represents a round avatar image, or the initials of the item's text when there is no
    ///     image.
    /// </summary>
    /// <param name="Url">The image URL, or <see langword="null" /> to display initials.</param>
    public sealed record Avatar(string? Url) : ItemMedia;

    /// <summary>
    ///     Represents a square image with rounded corners, cropped to fill its size.
    /// </summary>
    /// <param name="Url">The image URL.</param>
    public sealed record Image(string Url) : ItemMedia;

    /// <summary>
    ///     Represents a short code, such as an airport or currency code, displayed as a monospace
    ///     badge.
    /// </summary>
    /// <param name="Value">The code.</param>
    public sealed record Code(string Value) : ItemMedia;
}
