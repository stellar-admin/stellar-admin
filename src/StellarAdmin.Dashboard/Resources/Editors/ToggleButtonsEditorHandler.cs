namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a row of toggle buttons.
/// </summary>
public sealed class ToggleButtonsEditorHandler(
    ToggleButtonsEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<ToggleButtonsEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/ToggleButtons";
}
