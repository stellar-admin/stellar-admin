namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Creates a resource from a form model.
/// </summary>
/// <remarks>
///     Return validation errors for expected rejection without persisting changes.
///     Error field names are model property names without a form prefix. Unexpected failures should throw.
/// </remarks>
public interface IResourceCreateHandler<TModel>
{
    /// <summary>
    ///     Persists a new resource from the submitted model.
    /// </summary>
    Task<ResourceOperationResult> CreateAsync(TModel model, CancellationToken cancellationToken);
}
