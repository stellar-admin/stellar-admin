namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores additional CSS classes for the parts of an image editor.
/// </summary>
public sealed class ImageEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Gets or sets additional CSS classes for the image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    ///     Gets or sets additional CSS classes for the note under the editor.
    /// </summary>
    /// <remarks>
    ///     The note shows the accepted types, upload progress, errors and staged changes.
    /// </remarks>
    public string? Note { get; set; }
}
