namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Deletes resources.
/// </summary>
/// <remarks>
///     Return validation errors for expected rejection without persisting changes.
///     Error field names are model property names without a form prefix. Unexpected failures should throw.
/// </remarks>
public interface IResourceDeleteHandler<TResource>
{
    /// <summary>
    ///     Deletes the resource identified by the key, reporting not-found if it does not exist.
    /// </summary>
    Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken);
}
