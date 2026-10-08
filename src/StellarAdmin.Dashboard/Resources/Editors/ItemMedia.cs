namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The media displayed beside an item's text: an icon, an avatar, an image or a short code.
/// </summary>
public abstract record ItemMedia
{
    private ItemMedia() { }

    /// <summary>
    ///     A registered icon, displayed in the text color.
    /// </summary>
    /// <param name="Name">The icon's name.</param>
    public sealed record Icon(string Name) : ItemMedia;

    /// <summary>
    ///     A round avatar image, or the item text's initials when there is no image.
    /// </summary>
    /// <param name="Url">The image URL, or null to display initials.</param>
    public sealed record Avatar(string? Url) : ItemMedia;

    /// <summary>
    ///     A square image with rounded corners, cropped to fill its size.
    /// </summary>
    /// <param name="Url">The image URL.</param>
    public sealed record Image(string Url) : ItemMedia;

    /// <summary>
    ///     A short code, such as an airport or currency code, displayed as a monospace badge.
    /// </summary>
    /// <param name="Value">The code.</param>
    public sealed record Code(string Value) : ItemMedia;
}
