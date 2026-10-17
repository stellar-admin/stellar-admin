namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a numeric field as a slider.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class SliderEditorHandler(SliderEditor editor)
    : FieldEditorHandler<SliderEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Slider";
}
