namespace StellarAdmin.UI;

/// <summary>
///     Specifies how far an attachment has progressed through its upload.
/// </summary>
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

internal static class AttachmentStateExtensions
{
    extension(AttachmentState state)
    {
        public string GetDataAttributeText() =>
            state switch
            {
                AttachmentState.Idle => "idle",
                AttachmentState.Uploading => "uploading",
                AttachmentState.Processing => "processing",
                AttachmentState.Error => "error",
                AttachmentState.Done => "done",
                _ => string.Empty,
            };
    }
}
