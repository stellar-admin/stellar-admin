using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

// Spike: proves a friendly editor can live in an EditorTemplates subfolder.
public sealed class SpikeTextInputEditor : FieldEditor, IFieldEditor<SpikeTextInputEditorHandler>;

public sealed class SpikeTextInputEditorHandler(SpikeTextInputEditor editor)
    : FieldEditorHandler<SpikeTextInputEditor>(editor)
{
    public override string TemplateName => "Editors/TextInput";
}
