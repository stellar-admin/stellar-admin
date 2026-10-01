namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a radio group or radio choice-card editor.
/// </summary>
public class RadioGroupEditor : FieldEditor
{
    /// <summary>
    ///     Additional CSS classes for the radio editor and its choices.
    /// </summary>
    public override RadioGroupEditorClassNames ClassNames { get; } = new();
}
