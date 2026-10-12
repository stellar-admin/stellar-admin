namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Loads the model for a resource's row details when the row expands.
/// </summary>
public interface IResourceRowDetailHandler<TModel>
{
    /// <summary>
    ///     Returns the row details model, or null when the resource does not exist.
    /// </summary>
    Task<TModel?> FindAsync(string id, CancellationToken cancellationToken);
}
