namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a segmented control.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class SegmentedControlEditorHandler(
    SegmentedControlEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<SegmentedControlEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/SegmentedControl";
}
