namespace StellarAdmin.UI;

/// <summary>
///     Specifies how far an attachment has progressed through its upload.
/// </summary>
[GenerateDataAttributeText]
public enum AttachmentState
{
    /// <summary>
    ///     Nothing is attached yet, and the attachment shows a dashed border.
    /// </summary>
    Idle,

    /// <summary>
    ///     The file is uploading, and its title shimmers.
    /// </summary>
    Uploading,

    /// <summary>
    ///     The file is uploaded and being processed, and its title shimmers.
    /// </summary>
    Processing,

    /// <summary>
    ///     The upload or processing failed.
    /// </summary>
    Error,

    /// <summary>
    ///     The file is available.
    /// </summary>
    Done,
}
