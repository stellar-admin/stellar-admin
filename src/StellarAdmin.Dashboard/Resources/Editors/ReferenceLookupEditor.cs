using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a reference field using values from a lookup provider.
/// </summary>
public sealed class ReferenceLookupEditor(
    ReferenceLookupEditorOptions options,
    IReferenceLookupProviderResolver lookupProviders
) : IFieldEditor<ReferenceLookupEditorOptions>
{
    /// <inheritdoc />
    public string TemplateName => nameof(ReferenceLookupEditor);

    /// <inheritdoc />
    public async Task<object?> PrepareAsync(CancellationToken cancellationToken)
    {
        if (options.LookupProviderType is not { } providerType)
        {
            throw new InvalidOperationException("A reference lookup provider is required.");
        }

        var provider = lookupProviders.Resolve(providerType);

        return await provider.GetLookupsAsync(cancellationToken);
    }
}
