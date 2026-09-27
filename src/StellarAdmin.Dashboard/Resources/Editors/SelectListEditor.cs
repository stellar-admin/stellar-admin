using Microsoft.Extensions.DependencyInjection;
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
        if (options.ItemsFactory is { } itemsFactory)
        {
            return itemsFactory();
        }

        if (options.ItemsProviderType is { } providerType)
        {
            var provider = (ISelectListItemsProvider)services.GetRequiredService(providerType);
            return await provider.GetItemsAsync(cancellationToken);
        }

        throw new InvalidOperationException("Select list editor choices are required.");
    }
}
