namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The requested resources and listing settings.
/// </summary>
public sealed record ResourceListRequest
{
    /// <summary>
    ///     The requested page, or null to return all matching resources.
    /// </summary>
    public ResourcePaging? Paging { get; init; }

    /// <summary>
    ///     The requested ordering, or null to use the data source's default.
    /// </summary>
    public ResourceSort? Sort { get; init; }
}
