namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The requested resources and paging settings.
/// </summary>
public sealed record ResourceListRequest
{
    /// <summary>
    ///     The requested page, or null to return all matching resources.
    /// </summary>
    public ResourcePaging? Paging { get; init; }
}
