namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a single-line text input.
/// </summary>
public sealed class TextInputEditorHandler(TextInputEditor editor)
    : FieldEditorHandler<TextInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/TextInput";
}
