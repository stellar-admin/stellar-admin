namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a single-line text input.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class TextInputEditorHandler(TextInputEditor editor)
    : FieldEditorHandler<TextInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/TextInput";
}
