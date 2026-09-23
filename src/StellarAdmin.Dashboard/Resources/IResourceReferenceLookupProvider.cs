using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides lookup items for resource reference fields.
/// </summary>
public interface IResourceReferenceLookupProvider<TResource>
{
    /// <summary>
    ///     Returns lookup items keyed by form field name.
    /// </summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<SelectListItem>>> GetLookupsAsync(
        Type modelType,
        IReadOnlyCollection<string> fieldNames,
        CancellationToken cancellationToken
    );
}
