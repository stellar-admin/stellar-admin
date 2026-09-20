namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides data operations for a resource.
/// </summary>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Persists a new resource.
    /// </summary>
    Task CreateAsync(TResource resource, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes the resource identified by the key, returning false if it does not exist.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

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
    ///     Updates the resource identified by the key, returning false if it no longer exists.
    /// </summary>
    Task<bool> UpdateAsync(string id, TResource resource, CancellationToken cancellationToken);
}
