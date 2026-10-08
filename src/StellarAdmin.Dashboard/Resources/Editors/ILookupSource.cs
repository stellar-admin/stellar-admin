namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor during the current request.
/// </summary>
public interface ILookupSource<TEntity, TValue>
{
    /// <summary>
    ///     Returns the items with the specified values, in any order. Values that do not exist are left out.
    /// </summary>
    Task<IReadOnlyCollection<TEntity>> FindAsync(
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items matching the query.
    /// </summary>
    Task<LookupPage<TEntity>> SearchAsync(LookupQuery query, CancellationToken cancellationToken);
}
