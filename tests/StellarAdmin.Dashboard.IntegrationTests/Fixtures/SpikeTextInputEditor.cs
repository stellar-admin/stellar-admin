using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

// Spike: proves a friendly editor can live in an EditorTemplates subfolder.
public sealed class SpikeTextInputEditorOptions
    : EditorOptions,
        IFieldEditorOptions<SpikeTextInputEditor>;

public sealed class SpikeTextInputEditor(SpikeTextInputEditorOptions options)
    : IFieldEditor<SpikeTextInputEditorOptions>
{
    public SpikeTextInputEditorOptions Options { get; } = options;

    public string TemplateName => "Editors/TextInput";

    public Task<object?> PrepareAsync(CancellationToken cancellationToken) =>
        Task.FromResult<object?>(null);
}
