namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a radio group editor.
/// </summary>
public sealed class RadioGroupEditor : ChoiceEditor, IFieldEditor<RadioGroupEditorHandler>
{
    /// <summary>
    ///     How the choices are displayed. Defaults to <see cref="RadioGroupAppearance.Default" />.
    /// </summary>
    public RadioGroupAppearance Appearance { get; set; }

    /// <summary>
    ///     Additional CSS classes for the radio editor and its choices.
    /// </summary>
    public override RadioGroupEditorClassNames ClassNames { get; } = new();
}
