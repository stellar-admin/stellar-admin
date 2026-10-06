namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     A page of lookup items.
/// </summary>
/// <param name="Items">The items on the page.</param>
/// <param name="HasMore">Whether more items follow the page.</param>
public sealed record LookupPage<TEntity>(IReadOnlyList<TEntity> Items, bool HasMore);
