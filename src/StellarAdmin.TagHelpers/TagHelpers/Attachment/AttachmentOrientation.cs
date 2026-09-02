namespace StellarAdmin.TagHelpers;

/// <summary>
///     How an attachment arranges its media and content.
/// </summary>
public enum AttachmentOrientation
{
    /// <summary>The media sits beside the content, as a row.</summary>
    Horizontal,

    /// <summary>The media sits above the content, as a card.</summary>
    Vertical,
}

internal static class AttachmentOrientationExtensions
{
    extension(AttachmentOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                AttachmentOrientation.Horizontal => "horizontal",
                AttachmentOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
    }
}
