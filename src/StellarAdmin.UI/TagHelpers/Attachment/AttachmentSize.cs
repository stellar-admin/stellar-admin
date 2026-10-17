namespace StellarAdmin.UI;

/// <summary>
///     The size of an attachment.
/// </summary>
public enum AttachmentSize
{
    /// <summary>The default attachment size.</summary>
    Default,

    /// <summary>An extra-small attachment.</summary>
    ExtraSmall,

    /// <summary>A small attachment.</summary>
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
