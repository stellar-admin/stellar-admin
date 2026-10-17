namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a Boolean field as a single checkbox.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class CheckboxEditorHandler(CheckboxEditor editor)
    : FieldEditorHandler<CheckboxEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Checkbox";
}
