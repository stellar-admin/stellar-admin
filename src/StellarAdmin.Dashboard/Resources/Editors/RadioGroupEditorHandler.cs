namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a radio group.
/// </summary>
public sealed class RadioGroupEditorHandler(RadioGroupEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<RadioGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/RadioGroup";
}
