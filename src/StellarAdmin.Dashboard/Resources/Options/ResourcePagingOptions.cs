namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures index paging.
/// </summary>
public sealed class ResourcePagingOptions
{
    /// <summary>
    ///     The default number of resources per page.
    /// </summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    ///     The page sizes available for selection.
    /// </summary>
    public int[] PageSizes { get; set; } = [10, 25, 50, 100];
}
