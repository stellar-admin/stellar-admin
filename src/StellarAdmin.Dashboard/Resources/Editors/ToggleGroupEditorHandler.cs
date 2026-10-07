namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a toggle group.
/// </summary>
public sealed class ToggleGroupEditorHandler(ToggleGroupEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<ToggleGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/ToggleGroup";
}
