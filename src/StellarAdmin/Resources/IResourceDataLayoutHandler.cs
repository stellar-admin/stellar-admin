namespace StellarAdmin.Resources;

/// <summary>
///     Loads the items of an index data layout whose data differs from the resource's own rows,
///     such as a summary of them.
/// </summary>
/// <typeparam name="TItem">The type of the items the data layout shows.</typeparam>
public interface IResourceDataLayoutHandler<TItem>
{
    /// <summary>
    ///     Returns the data layout's items and their total number.
    /// </summary>
    /// <param name="request">
    ///     The request with the data layout's key and the scope and search to apply.
    /// </param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the items and their total number.</returns>
    /// <remarks>
    ///     The handler applies the scope and search itself. A scope or search that the data layout
    ///     ignores is <see langword="null" /> in the request.
    /// </remarks>
    Task<ResourceListResult<TItem>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
