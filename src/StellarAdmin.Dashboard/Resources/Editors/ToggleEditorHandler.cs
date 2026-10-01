namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a Boolean field as an on and off switch.
/// </summary>
public sealed class ToggleEditorHandler(ToggleEditor editor)
    : FieldEditorHandler<ToggleEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Toggle";
}
