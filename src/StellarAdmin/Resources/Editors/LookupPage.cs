namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Represents a page of lookup items.
/// </summary>
/// <typeparam name="TEntity">The type of the items.</typeparam>
/// <param name="Items">The items on the page.</param>
/// <param name="HasMore">
///     <see langword="true" /> if more items follow the page; otherwise, <see langword="false" />.
/// </param>
public sealed record LookupPage<TEntity>(IReadOnlyList<TEntity> Items, bool HasMore);
