namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a radio group.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class RadioGroupEditorHandler(RadioGroupEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<RadioGroupEditor>(editor, services)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/RadioGroup";
}
