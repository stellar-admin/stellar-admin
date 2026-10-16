namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Additional CSS classes for an image editor, its image and its note.
/// </summary>
public sealed class ImageEditorClassNames : EditorClassNames
{
    /// <summary>
    ///     Additional CSS classes applied to the image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    ///     Additional CSS classes applied to the note under the editor, which shows the accepted
    ///     types, upload progress, errors and staged changes.
    /// </summary>
    public string? Note { get; set; }
}
