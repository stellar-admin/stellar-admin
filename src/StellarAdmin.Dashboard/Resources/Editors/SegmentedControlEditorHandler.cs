namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a segmented control.
/// </summary>
public sealed class SegmentedControlEditorHandler(
    SegmentedControlEditor editor,
    IServiceProvider services
) : ChoiceEditorHandler<SegmentedControlEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/SegmentedControl";
}
