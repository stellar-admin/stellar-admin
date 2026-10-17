namespace StellarAdmin.Resources;

/// <summary>
///     Lists the resources shown on the index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Returns the requested resources and the total number that match.
    /// </summary>
    /// <param name="request">The request that specifies which resources to return.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the requested resources and the total number that match.
    /// </returns>
    /// <remarks>
    ///     Sort in a stable order when paging. Use a unique key to break ties.
    /// </remarks>
    Task<ResourceListResult<TResource>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
