namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Supplies the items of a lookup editor for each request.
/// </summary>
/// <typeparam name="TEntity">The type of the items.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
public interface ILookupSource<TEntity, TValue>
{
    /// <summary>
    ///     Returns the items with the specified values.
    /// </summary>
    /// <param name="values">The values of the items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     The items, in any order. Values that don't match an item are left out.
    /// </returns>
    Task<IReadOnlyCollection<TEntity>> FindAsync(
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items that match a query.
    /// </summary>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    Task<LookupPage<TEntity>> SearchAsync(LookupQuery query, CancellationToken cancellationToken);
}

/// <summary>
///     Supplies the items of a lookup editor for each request, searching them with a value that
///     the lookup's field takes from its form.
/// </summary>
/// <typeparam name="TEntity">The type of the items.</typeparam>
/// <typeparam name="TValue">The type of the value that identifies an item.</typeparam>
/// <typeparam name="TArgument">
///     The type of the value the search needs, such as the ID of the country whose cities are
///     listed.
/// </typeparam>
/// <remarks>
///     A search that needs several values takes a record that holds them.
/// </remarks>
public interface ILookupSource<TEntity, TValue, in TArgument>
{
    /// <summary>
    ///     Returns the items with the specified values.
    /// </summary>
    /// <param name="values">The values of the items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     The items, in any order. Values that don't match an item are left out.
    /// </returns>
    Task<IReadOnlyCollection<TEntity>> FindAsync(
        IReadOnlyCollection<TValue> values,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Returns a page of items that match a query.
    /// </summary>
    /// <param name="argument">The value the field takes from its form.</param>
    /// <param name="query">The search term and the page of items to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>The matching items, and whether more items follow them.</returns>
    Task<LookupPage<TEntity>> SearchAsync(
        TArgument argument,
        LookupQuery query,
        CancellationToken cancellationToken
    );
}
