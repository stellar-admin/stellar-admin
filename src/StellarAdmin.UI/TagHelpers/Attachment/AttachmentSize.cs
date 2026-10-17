namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an attachment.
/// </summary>
public enum AttachmentSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     An extra-small size.
    /// </summary>
    ExtraSmall,

    /// <summary>
    ///     A small size.
    /// </summary>
    Small,
}

internal static class AttachmentSizeExtensions
{
    extension(AttachmentSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                AttachmentSize.Default => "default",
                AttachmentSize.ExtraSmall => "xs",
                AttachmentSize.Small => "sm",
                _ => string.Empty,
            };
    }
}
