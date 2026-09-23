using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Editors;

internal sealed class ReferenceLookupProviderResolver(IServiceProvider services)
    : IReferenceLookupProviderResolver
{
    public IReferenceLookupProvider Resolve(Type providerType) =>
        (IReferenceLookupProvider)services.GetRequiredService(providerType);
}
