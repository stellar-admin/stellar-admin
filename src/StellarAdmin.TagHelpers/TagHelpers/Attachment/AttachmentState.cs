namespace StellarAdmin.TagHelpers;

/// <summary>
///     The point an attachment has reached in its upload lifecycle.
/// </summary>
public enum AttachmentState
{
    /// <summary>Nothing has been attached yet, drawn with a dashed border as a drop target.</summary>
    Idle,

    /// <summary>The file is being uploaded, and its title shimmers.</summary>
    Uploading,

    /// <summary>The upload finished and the file is being processed, and its title shimmers.</summary>
    Processing,

    /// <summary>The upload or processing failed.</summary>
    Error,

    /// <summary>The file is available.</summary>
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
