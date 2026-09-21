namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides the resources displayed on the index page.
/// </summary>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Returns the requested resources and the total matching count before paging.
    /// </summary>
    /// <remarks>
    ///     Use stable ordering when paging, with a unique key to break ties.
    /// </remarks>
    Task<ResourceListResult<TResource>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    );
}
