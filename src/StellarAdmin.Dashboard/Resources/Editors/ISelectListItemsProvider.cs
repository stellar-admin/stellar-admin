using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies choices for a select list editor during the current request.
/// </summary>
public interface ISelectListItemsProvider : IChoiceItemsProvider
{
    /// <summary>
    ///     Returns the available choices.
    /// </summary>
    new Task<IReadOnlyList<SelectListItem>> GetItemsAsync(CancellationToken cancellationToken);
}
