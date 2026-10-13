namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Loads the model a resource's view page displays.
/// </summary>
public interface IResourceViewHandler<TModel>
{
    /// <summary>
    ///     Returns the model, or null when the resource does not exist.
    /// </summary>
    Task<TModel?> FindAsync(string id, CancellationToken cancellationToken);
}
