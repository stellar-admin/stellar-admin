namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an avatar.
/// </summary>
[GenerateDataAttributeText]
public enum AvatarSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A small size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A large size.
    /// </summary>
    [DataAttributeText("lg")]
    Large,
}
