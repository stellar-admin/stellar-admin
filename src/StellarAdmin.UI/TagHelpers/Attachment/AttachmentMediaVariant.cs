namespace StellarAdmin.UI;

/// <summary>
///     Specifies the kind of media an attachment shows.
/// </summary>
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

internal static class AttachmentMediaVariantExtensions
{
    extension(AttachmentMediaVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                AttachmentMediaVariant.Icon => "icon",
                AttachmentMediaVariant.Image => "image",
                _ => string.Empty,
            };
    }
}
