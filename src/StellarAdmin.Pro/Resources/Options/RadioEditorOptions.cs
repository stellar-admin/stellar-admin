namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     Configures a radio group or radio choice-card editor.
/// </summary>
public class RadioEditorOptions : EditorOptions
{
    /// <summary>
    ///     Additional CSS classes for the radio editor and its choices.
    /// </summary>
    public override RadioEditorClassNames ClassNames { get; } = new();
}
