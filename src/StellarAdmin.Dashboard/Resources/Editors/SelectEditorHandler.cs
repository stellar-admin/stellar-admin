namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field using application-supplied select choices.
/// </summary>
public sealed class SelectEditorHandler(SelectEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<SelectEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "SelectListEditor";
}
