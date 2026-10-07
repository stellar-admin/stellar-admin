namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a toggle group editor. A collection property selects multiple values; any other property selects one.
/// </summary>
public sealed class ToggleGroupEditor : ChoiceEditor, IFieldEditor<ToggleGroupEditorHandler>
{
    /// <summary>
    ///     How the choices are displayed. Defaults to <see cref="ToggleGroupAppearance.Chips" />.
    /// </summary>
    public ToggleGroupAppearance Appearance { get; set; }
}
