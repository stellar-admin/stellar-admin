namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a Boolean field as a single checkbox.
/// </summary>
public sealed class CheckboxEditorHandler(CheckboxEditor editor)
    : FieldEditorHandler<CheckboxEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Checkbox";
}
