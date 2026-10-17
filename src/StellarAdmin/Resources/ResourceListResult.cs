namespace StellarAdmin.Resources;

/// <summary>
///     Represents the resources a data source returns.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
/// <param name="Items">The resources on the requested page.</param>
/// <param name="TotalCount">The total number of matching resources, before paging.</param>
public sealed record ResourceListResult<TResource>(IReadOnlyList<TResource> Items, long TotalCount);
