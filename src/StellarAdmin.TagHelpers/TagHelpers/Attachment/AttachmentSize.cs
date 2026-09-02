namespace StellarAdmin.TagHelpers;

/// <summary>
///     The size of an attachment.
/// </summary>
public enum AttachmentSize
{
    /// <summary>The default attachment size.</summary>
    Default,

    /// <summary>A small attachment.</summary>
    Small,

    /// <summary>An extra-small attachment.</summary>
    ExtraSmall,
}

internal static class AttachmentSizeExtensions
{
    extension(AttachmentSize size)
    {
        public string GetDataAttributeText() =>
            size switch
            {
                AttachmentSize.Default => "default",
                AttachmentSize.Small => "sm",
                AttachmentSize.ExtraSmall => "xs",
                _ => string.Empty,
            };
    }
}
