using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field using application-supplied select choices.
/// </summary>
public sealed class SelectListEditor(SelectListEditorOptions options, IServiceProvider services)
    : ChoiceItemsEditor<SelectListEditorOptions>(options, services)
{
    /// <inheritdoc />
    public override string TemplateName => nameof(SelectListEditor);
}
