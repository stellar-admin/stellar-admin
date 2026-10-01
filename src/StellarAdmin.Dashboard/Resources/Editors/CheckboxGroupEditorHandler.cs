namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a collection field using application-supplied checkbox choices.
/// </summary>
public sealed class CheckboxGroupEditorHandler(
    CheckboxGroupEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<CheckboxGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "CheckboxGroupEditor";
}
