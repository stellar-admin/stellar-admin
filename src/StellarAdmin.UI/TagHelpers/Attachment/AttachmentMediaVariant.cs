namespace StellarAdmin.UI;

/// <summary>
///     Specifies the kind of media an attachment shows.
/// </summary>
[GenerateDataAttributeText]
public enum AttachmentMediaVariant
{
    /// <summary>
    ///     An icon that represents the file type.
    /// </summary>
    Icon,

    /// <summary>
    ///     A thumbnail of the file.
    /// </summary>
    Image,
}
