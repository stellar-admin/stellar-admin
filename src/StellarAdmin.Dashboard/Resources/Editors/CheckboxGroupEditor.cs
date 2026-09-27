using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a collection field using application-supplied checkbox choices.
/// </summary>
public sealed class CheckboxGroupEditor(
    CheckboxGroupEditorOptions options,
    IServiceProvider services
) : ChoiceItemsEditor<CheckboxGroupEditorOptions>(options, services)
{
    /// <inheritdoc />
    public override string TemplateName => nameof(CheckboxGroupEditor);
}
