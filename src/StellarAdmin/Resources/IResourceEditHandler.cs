namespace StellarAdmin.Resources;

/// <summary>
///     Loads and updates a resource through a form model.
/// </summary>
/// <typeparam name="TModel">The type of the form model.</typeparam>
/// <remarks>
///     Changes to the model that <see cref="IResourceViewHandler{TModel}.FindAsync" /> returns
///     must not be saved until <see cref="UpdateAsync" /> succeeds. Return validation errors when
///     the input is rejected, and don't save any changes. Error field names are the model's
///     property names, without a form prefix. Throw for unexpected failures.
/// </remarks>
public interface IResourceEditHandler<TModel> : IResourceViewHandler<TModel>
{
    /// <summary>
    ///     Updates a resource from the submitted model.
    /// </summary>
    /// <param name="id">The key of the resource to update.</param>
    /// <param name="model">The submitted model.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the outcome of the operation.</returns>
    /// <remarks>
    ///     Return <see cref="ResourceOperationResult.NotFound" /> when the resource no longer exists.
    /// </remarks>
    Task<ResourceOperationResult> UpdateAsync(
        string id,
        TModel model,
        CancellationToken cancellationToken
    );
}
