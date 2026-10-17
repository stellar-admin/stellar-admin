namespace StellarAdmin.Resources;

/// <summary>
///     Deletes resources.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
/// <remarks>
///     Return validation errors when the deletion is rejected, and don't save any changes. Throw
///     for unexpected failures.
/// </remarks>
public interface IResourceDeleteHandler<TResource>
{
    /// <summary>
    ///     Deletes a resource.
    /// </summary>
    /// <param name="id">The key of the resource to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the outcome of the operation.</returns>
    /// <remarks>
    ///     Return <see cref="ResourceOperationResult.NotFound" /> when the resource doesn't exist.
    /// </remarks>
    Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken);
}
