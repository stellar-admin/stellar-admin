namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the choices of a choice editor for each request.
/// </summary>
public interface IChoiceItemsProvider
{
    /// <summary>
    ///     Returns the available choices.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The choices, in display order.</returns>
    Task<IReadOnlyList<ChoiceItem>> GetItemsAsync(CancellationToken cancellationToken);
}
