namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a multi-line text area.
/// </summary>
public sealed class TextareaEditorHandler(TextareaEditor editor)
    : FieldEditorHandler<TextareaEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Textarea";
}
