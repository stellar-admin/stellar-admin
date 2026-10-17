namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a select.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class SelectEditorHandler(SelectEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<SelectEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Select";
}
