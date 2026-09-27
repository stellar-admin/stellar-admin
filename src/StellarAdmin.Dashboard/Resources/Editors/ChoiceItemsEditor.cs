using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Loads choices for a field editor.
/// </summary>
public abstract class ChoiceItemsEditor<TOptions>(TOptions options, IServiceProvider services)
    : IFieldEditor<TOptions>
    where TOptions : ChoiceItemsEditorOptions
{
    /// <inheritdoc />
    public abstract string TemplateName { get; }

    /// <inheritdoc />
    public async Task<object?> PrepareAsync(CancellationToken cancellationToken)
    {
        var itemsLoader =
            options.ItemsLoader
            ?? throw new InvalidOperationException("Choice editor items are required.");

        return await itemsLoader(services, cancellationToken);
    }
}
