namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The requested resources and the total matching count before paging.
/// </summary>
public sealed record ResourceListResult<TResource>(IReadOnlyList<TResource> Items, long TotalCount);
