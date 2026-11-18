namespace StellarAdmin.Resources;

/// <summary>
///     Loads the items of an index layout whose data differs from the resource's own rows, such as a
///     summary of them.
/// </summary>
/// <typeparam name="TItem">The type of the items the layout shows.</typeparam>
public interface IResourceLayoutHandler<TItem>
{
    /// <summary>
    ///     Returns the layout's items and their total number.
    /// </summary>
    /// <param name="request">
    ///     The request with the layout's key and the scope and search to apply.
    /// </param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the items and their total number.</returns>
    /// <remarks>
    ///     The handler applies the scope and search itself. A scope or search that the layout
    ///     ignores is <see langword="null" /> in the request.
    /// </remarks>
    Task<ResourceListResult<TItem>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
