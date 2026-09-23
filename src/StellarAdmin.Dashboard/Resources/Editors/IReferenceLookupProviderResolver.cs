namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Resolves the lookup provider selected for a field.
/// </summary>
public interface IReferenceLookupProviderResolver
{
    /// <summary>
    ///     Resolves a registered lookup provider.
    /// </summary>
    IReferenceLookupProvider Resolve(Type providerType);
}
