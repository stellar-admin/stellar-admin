using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies the selectable values for a reference lookup editor.
/// </summary>
public interface IReferenceLookupProvider
{
    /// <summary>
    ///     Returns the selectable values for the current request.
    /// </summary>
    Task<IReadOnlyList<SelectListItem>> GetLookupsAsync(CancellationToken cancellationToken);
}
