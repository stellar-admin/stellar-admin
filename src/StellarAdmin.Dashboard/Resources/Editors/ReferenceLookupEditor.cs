using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a reference field using values from a lookup provider.
/// </summary>
public sealed class ReferenceLookupEditor : ResourceEditor
{
    private Type? _providerType;

    /// <summary>
    ///     Selects the provider that supplies the lookup values.
    /// </summary>
    public ReferenceLookupEditor UseLookup<TProvider>()
        where TProvider : class, IReferenceLookupProvider
    {
        _providerType = typeof(TProvider);
        return this;
    }

    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        if (_providerType is null)
        {
            throw new InvalidOperationException("A reference lookup provider is required.");
        }

        return await (
            (IReferenceLookupProvider)services.GetRequiredService(_providerType)
        ).GetLookupsAsync(cancellationToken);
    }
}
