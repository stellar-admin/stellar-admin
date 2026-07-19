namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The size of a <c>&lt;sa-avatar&gt;</c>.
/// </summary>
public enum AvatarSize
{
    /// <summary>The default avatar size.</summary>
    Default,

    /// <summary>A small avatar.</summary>
    Small,

    /// <summary>A large avatar.</summary>
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
