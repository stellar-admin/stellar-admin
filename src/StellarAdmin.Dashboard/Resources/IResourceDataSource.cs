namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     Provides the resources displayed on the index page.
/// </summary>
public interface IResourceDataSource<TResource>
{
    /// <summary>
    ///     Returns the resources to display on the index page.
    /// </summary>
    Task<IReadOnlyList<TResource>> ListAsync(CancellationToken cancellationToken);
}
