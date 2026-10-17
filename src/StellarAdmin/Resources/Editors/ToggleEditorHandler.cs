namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a Boolean field as an on and off switch.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class ToggleEditorHandler(ToggleEditor editor)
    : FieldEditorHandler<ToggleEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Toggle";
}
