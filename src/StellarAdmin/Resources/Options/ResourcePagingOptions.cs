namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the paging configuration of the index page.
/// </summary>
public sealed class ResourcePagingOptions
{
    /// <summary>
    ///     Gets the number of resources per page when the user hasn't picked a page size.
    /// </summary>
    /// <value>
    ///     The number of resources. The default is 25.
    /// </value>
    /// <remarks>
    ///     The value must be one of <see cref="PageSizes" />.
    /// </remarks>
    public int PageSize { get; internal set; } = 25;

    /// <summary>
    ///     Gets the page sizes the user can pick from.
    /// </summary>
    /// <value>
    ///     The page sizes. The default is 10, 25, 50 and 100.
    /// </value>
    /// <remarks>
    ///     The sizes must be positive and distinct, and there must be at least one.
    /// </remarks>
    public IReadOnlyList<int> PageSizes { get; internal set; } = [10, 25, 50, 100];
}
