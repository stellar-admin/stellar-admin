namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     A one-based page number and the number of resources per page.
/// </summary>
public sealed record ResourcePaging(int Page, int PageSize);
