namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a select.
/// </summary>
public sealed class SelectEditorHandler(SelectEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<SelectEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Select";
}
