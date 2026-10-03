namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The kind of media displayed beside a lookup item's title.
/// </summary>
public enum LookupMediaType
{
    /// <summary>
    ///     A short code, such as an airport or currency code.
    /// </summary>
    Code,

    /// <summary>
    ///     An avatar image, or the title's initials when there is no image.
    /// </summary>
    Avatar,
}
