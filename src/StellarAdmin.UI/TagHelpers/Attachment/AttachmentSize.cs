namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of an attachment.
/// </summary>
[GenerateDataAttributeText]
public enum AttachmentSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     An extra-small size.
    /// </summary>
    [DataAttributeText("xs")]
    ExtraSmall,

    /// <summary>
    ///     A small size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
