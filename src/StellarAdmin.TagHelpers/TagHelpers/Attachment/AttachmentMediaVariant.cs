namespace StellarAdmin.TagHelpers;

/// <summary>
///     The kind of media held by an attachment's media region.
/// </summary>
public enum AttachmentMediaVariant
{
    /// <summary>An icon standing in for the file type.</summary>
    Icon,

    /// <summary>A thumbnail preview of the file itself.</summary>
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
