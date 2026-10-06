namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor during the current request.
/// </summary>
public interface ILookupSource<TEntity, TValue>
{
    /// <summary>
    ///     Returns the item with the specified value, or null when it does not exist.
    /// </summary>
    Task<TEntity?> FindAsync(TValue value, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a page of items matching the query.
    /// </summary>
    Task<LookupPage<TEntity>> SearchAsync(LookupQuery query, CancellationToken cancellationToken);
}
