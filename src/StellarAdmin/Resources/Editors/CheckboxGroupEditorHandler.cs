namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a collection field as a checkbox group.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class CheckboxGroupEditorHandler(
    CheckboxGroupEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<CheckboxGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/CheckboxGroup";
}
