namespace StellarAdmin.Resources;

/// <summary>
///     Loads the model for a row's details when the row is expanded.
/// </summary>
/// <typeparam name="TModel">The type of the details model.</typeparam>
public interface IResourceRowDetailHandler<TModel>
{
    /// <summary>
    ///     Returns the details model for a resource.
    /// </summary>
    /// <param name="id">The key of the resource.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the details model, or <see langword="null" /> when the resource
    ///     doesn't exist.
    /// </returns>
    Task<TModel?> FindAsync(string id, CancellationToken cancellationToken);
}
