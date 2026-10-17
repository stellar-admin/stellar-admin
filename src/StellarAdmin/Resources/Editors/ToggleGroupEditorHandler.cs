namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a toggle group.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class ToggleGroupEditorHandler(ToggleGroupEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<ToggleGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/ToggleGroup";
}
