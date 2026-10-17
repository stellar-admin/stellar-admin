namespace StellarAdmin.Resources;

/// <summary>
///     Specifies the page of resources to return.
/// </summary>
/// <param name="Page">The page number, starting at 1.</param>
/// <param name="PageSize">The number of resources on a page.</param>
public sealed record ResourcePaging(int Page, int PageSize);
