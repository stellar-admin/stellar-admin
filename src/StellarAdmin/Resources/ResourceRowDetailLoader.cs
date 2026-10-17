namespace StellarAdmin.Resources;

/// <summary>
///     Loads the model of a row detail from the row's key.
/// </summary>
/// <remarks>
///     Builders for a data source pass a loader to <c>RegisterRowDetails</c> or <c>AddPanel</c>.
/// </remarks>
public abstract class ResourceRowDetailLoader
{
    /// <summary>
    ///     Loads the model of a row detail.
    /// </summary>
    /// <param name="services">The request's service provider.</param>
    /// <param name="key">The key of the expanded row.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    ///     A task that returns the model, or <see langword="null" /> when the row no longer exists.
    /// </returns>
    public abstract Task<object?> LoadAsync(
        IServiceProvider services,
        string key,
        CancellationToken cancellationToken
    );
}
