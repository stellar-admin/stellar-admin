namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a collection field as a checkbox group.
/// </summary>
public sealed class CheckboxGroupEditorHandler(
    CheckboxGroupEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<CheckboxGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/CheckboxGroup";
}
