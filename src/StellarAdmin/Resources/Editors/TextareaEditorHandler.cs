namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a multi-line text area.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class TextareaEditorHandler(TextareaEditor editor)
    : FieldEditorHandler<TextareaEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Textarea";
}
