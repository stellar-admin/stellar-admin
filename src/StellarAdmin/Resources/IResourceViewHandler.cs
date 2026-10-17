namespace StellarAdmin.Resources;

/// <summary>
///     Loads the model that a resource's view page shows.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IResourceViewHandler<TModel>
{
    /// <summary>
    ///     Returns the model for a resource.
    /// </summary>
    /// <param name="id">The key of the resource.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the model, or <see langword="null" /> when the resource doesn't
    ///     exist.
    /// </returns>
    Task<TModel?> FindAsync(string id, CancellationToken cancellationToken);
}
