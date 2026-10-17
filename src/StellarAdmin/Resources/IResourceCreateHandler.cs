namespace StellarAdmin.Resources;

/// <summary>
///     Creates a resource from a form model.
/// </summary>
/// <typeparam name="TModel">The type of the form model.</typeparam>
/// <remarks>
///     Return validation errors when the input is rejected, and don't save any changes. Error
///     field names are the model's property names, without a form prefix. Throw for unexpected
///     failures.
/// </remarks>
public interface IResourceCreateHandler<TModel>
{
    /// <summary>
    ///     Saves a new resource from the submitted model.
    /// </summary>
    /// <param name="model">The submitted model.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A task that returns the outcome of the operation.</returns>
    Task<ResourceOperationResult> CreateAsync(TModel model, CancellationToken cancellationToken);
}
