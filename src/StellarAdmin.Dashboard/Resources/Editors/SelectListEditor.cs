using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field using application-supplied select choices.
/// </summary>
public sealed class SelectListEditor(SelectListEditorOptions options, IServiceProvider services)
    : IFieldEditor<SelectListEditorOptions>
{
    /// <inheritdoc />
    public string TemplateName => nameof(SelectListEditor);

    /// <inheritdoc />
    public async Task<object?> PrepareAsync(CancellationToken cancellationToken)
    {
        var itemsLoader =
            options.ItemsLoader
            ?? throw new InvalidOperationException("Select list editor choices are required.");

        return await itemsLoader(services, cancellationToken);
    }
}
