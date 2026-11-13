namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an attachment arranges its media and content.
/// </summary>
[GenerateDataAttributeText]
public enum AttachmentOrientation
{
    /// <summary>
    ///     The media sits beside the content, as a row.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The media sits above the content, as a card.
    /// </summary>
    Vertical,
}
