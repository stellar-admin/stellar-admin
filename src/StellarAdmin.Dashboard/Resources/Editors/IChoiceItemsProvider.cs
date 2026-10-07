namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies choices for a field editor during the current request.
/// </summary>
public interface IChoiceItemsProvider
{
    /// <summary>
    ///     Returns the available choices.
    /// </summary>
    Task<IReadOnlyList<ChoiceItem>> GetItemsAsync(CancellationToken cancellationToken);
}
