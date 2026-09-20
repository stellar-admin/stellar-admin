namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides data operations for a resource.
/// </summary>
/// <remarks>
///     Write operations return validation errors for expected rejection and must not persist rejected changes.
///     Error field names are model property names without a form prefix. Unexpected failures should throw.
/// </remarks>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Persists a new resource.
    /// </summary>
    Task<ResourceOperationResult> CreateAsync(
        TResource resource,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Deletes the resource identified by the key, reporting not-found if it does not exist.
    /// </summary>
    Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a resource for editing, or null when the key does not exist.
    /// </summary>
    /// <remarks>
    ///     Changes to the returned instance must not be persisted until UpdateAsync succeeds.
    /// </remarks>
    Task<TResource?> FindAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the resources to display on the index page.
    /// </summary>
    Task<IReadOnlyList<TResource>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Updates the resource identified by the key, reporting not-found if it no longer exists.
    /// </summary>
    Task<ResourceOperationResult> UpdateAsync(
        string id,
        TResource resource,
        CancellationToken cancellationToken
    );
}
