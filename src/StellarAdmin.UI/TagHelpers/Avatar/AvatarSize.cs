namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an avatar.
/// </summary>
public enum AvatarSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A small size.
    /// </summary>
    Small,

    /// <summary>
    ///     A large size.
    /// </summary>
    Large,
}

internal static class GetAvatarSizeAttributeText
{
    extension(AvatarSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                AvatarSize.Default => "default",
                AvatarSize.Small => "sm",
                AvatarSize.Large => "lg",
                _ => string.Empty,
            };
        }
    }
}
