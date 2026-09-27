using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies choices for a field editor during the current request.
/// </summary>
public interface IChoiceItemsProvider
{
    /// <summary>
    ///     Returns the available choices.
    /// </summary>
    Task<IReadOnlyList<SelectListItem>> GetItemsAsync(CancellationToken cancellationToken);
}
