namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a whole-number field as a slider.
/// </summary>
public sealed class SliderEditorHandler(SliderEditor editor)
    : FieldEditorHandler<SliderEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Slider";
}
