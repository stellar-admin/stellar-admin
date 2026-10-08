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

    /// <summary>
    ///     Where chips show their check mark while on. Defaults to <see cref="ToggleGroupCheckPlacement.ReplaceMedia" />.
    ///     Only chips show a check mark.
    /// </summary>
    public ToggleGroupCheckPlacement CheckPlacement { get; set; }
}
