namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an on and off switch editor for a Boolean property.
/// </summary>
public sealed class ToggleEditor : FieldEditor
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Toggle";
}
