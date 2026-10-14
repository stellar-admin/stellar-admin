namespace StellarAdmin.Resources;

/// <summary>
///     Loads and updates a resource using a form model.
/// </summary>
/// <remarks>
///     FindAsync returns an editable model, or null when the resource does not exist; changes to it must not persist
///     until UpdateAsync succeeds. Return validation errors for expected rejection without persisting changes.
///     Error field names are model property names without a form prefix. Unexpected failures should throw.
/// </remarks>
public interface IResourceEditHandler<TModel> : IResourceViewHandler<TModel>
{
    /// <summary>
    ///     Updates the resource identified by the key, reporting not-found if it no longer exists.
    /// </summary>
    Task<ResourceOperationResult> UpdateAsync(
        string id,
        TModel model,
        CancellationToken cancellationToken
    );
}
