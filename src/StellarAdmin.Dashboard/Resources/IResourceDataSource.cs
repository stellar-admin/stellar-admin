namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides data operations for a resource.
/// </summary>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Persists a new resource.
    /// </summary>
    Task CreateAsync(TResource resource, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the resources to display on the index page.
    /// </summary>
    Task<IReadOnlyList<TResource>> ListAsync(CancellationToken cancellationToken);
}
